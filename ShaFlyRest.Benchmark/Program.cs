﻿using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using ShaFlyRest.Core;

BenchmarkRunner.Run<ShaFlyRestBenchmark>();

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 0, targetCount: 1)]
public class ShaFlyRestBenchmark
{
    private readonly CancellationTokenSource _cts = new();
    private readonly IncrementalHash Hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);

    private CancellationToken ct => _cts.Token;
    
    // [Benchmark]
    public async Task SendUsingPipes()
    {
        using var client = new HttpClient();
        var pipeReader = await GetFilePipeReaderAsync(ct);

        var response = await client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(pipeReader.AsStream()), ct);
        response.EnsureSuccessStatusCode();
    }

    // [Benchmark]
    public async Task SendUsingStream()
    {
        using var client = new HttpClient();
        await using var fs = await GetFileStreamAsync(ct);

        var response = await client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(fs), ct);
        response.EnsureSuccessStatusCode();
    }

    [Benchmark]
    public async Task SendAndProcessUsingPipes()
    {
        using var client = new HttpClient();
        var pipeReader = await GetFilePipeReaderAsync(ct);

        var proxyPipe = new Pipe();
        var proxyPipeReader = proxyPipe.Reader;
        var proxyPipeWriter = proxyPipe.Writer;

        var proxyTask = client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyPipeReader.AsStream()), ct);
        while (!ct.IsCancellationRequested)
        {
            var contextReadResult = await pipeReader.ReadAsync(ct);
            var contextBuffer = contextReadResult.Buffer;

            if (contextBuffer.IsSingleSegment)
            {
                await ProcessBlock(proxyPipeWriter, contextBuffer.First, ct);
            }
            else
            {
                foreach (var segment in contextBuffer)
                {
                    await ProcessBlock(proxyPipeWriter, segment, ct);
                }
            }

            pipeReader.AdvanceTo(contextBuffer.End);

            if (contextReadResult.IsCompleted)
            {
                await proxyPipeWriter.FlushAsync(ct);
                await proxyPipeWriter.CompleteAsync();
                break;
            }
        }

        var response = await proxyTask;
        response.EnsureSuccessStatusCode();
        _ = Hasher.GetHashAndReset();
    }

    [Benchmark]
    public async Task SendAndProcessUsingStream()
    {
        using var client = new HttpClient();
        await using var fs = await GetFileStreamAsync(ct);
        await using var proxyStream = new MemoryStream();

        var buffer = new byte[1024 * 8];
        int read;
        while ((read = await fs.ReadAsync(buffer, ct)) > 0)
        {
            Hasher.AppendData(buffer);
            await proxyStream.WriteAsync(buffer.AsMemory(0, read), ct);
        }

        await proxyStream.FlushAsync(ct);

        proxyStream.Seek(0, SeekOrigin.Begin);
        var response = await client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyStream), ct);
        response.EnsureSuccessStatusCode();
        _ = Hasher.GetHashAndReset();
    }
    
    [Benchmark]
    public async Task SendAndProcessUsingWebSockets()
    {
        using var client = new HttpClient();
        await using var fs = await GetFileStreamAsync(ct);
        
        var request = (HttpWebRequest)WebRequest.Create(Constants.ScanCloudEndpoint);
        request.Method = WebRequestMethods.Http.Post;
        request.SendChunked = true;
        request.AllowWriteStreamBuffering = false;
        request.AllowReadStreamBuffering = false;
        request.ContentType = MediaTypeNames.Application.Octet;

        var buffer = new byte[4096];
        await using (var proxyStream = await request.GetRequestStreamAsync())
        {
            int read;
            while ((read = await fs.ReadAsync(buffer, ct)) > 0)
            {
                Hasher.AppendData(buffer);
                await proxyStream.WriteAsync(buffer.AsMemory(0, read), ct);
            }
        }
    }

    // [Benchmark]
    public async Task SendAndProcessUsingOptimizedPipes()
    {
        using var client = new HttpClient();
        var pipeReader = await GetFilePipeReaderAsync(ct);

        var proxyPipe = new Pipe();
        var proxyPipeReader = proxyPipe.Reader;
        var proxyPipeWriter = proxyPipe.Writer;

        var proxyTask = client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyPipeReader.AsStream()), ct);

        while (!ct.IsCancellationRequested)
        {
            var contextReadResult = await pipeReader.ReadAsync(ct);
            var contextBuffer = contextReadResult.Buffer;

            if (contextBuffer.IsSingleSegment)
            {
                ProcessBlockOptimized(proxyPipeWriter, contextBuffer.FirstSpan);
                await proxyPipeWriter.FlushAsync(ct);
            }
            else
            {
                foreach (var segment in contextBuffer)
                {
                    ProcessBlockOptimized(proxyPipeWriter, segment.Span);
                    await proxyPipeWriter.FlushAsync(ct);
                }
            }

            pipeReader.AdvanceTo(contextBuffer.End);

            if (contextReadResult.IsCompleted)
            {
                await proxyPipeWriter.FlushAsync(ct);
                await proxyPipeWriter.CompleteAsync();
                break;
            }
        }
        
        var response = await proxyTask;
        response.EnsureSuccessStatusCode();
        _ = Hasher.GetHashAndReset();
    }

    // [Benchmark]
    public async Task SendAndProcessUsingOptimizedPipesAsync()
    {
        using var client = new HttpClient();
        var pipeReader = await GetFilePipeReaderAsync(ct);

        var proxyPipe = new Pipe();
        var proxyPipeReader = proxyPipe.Reader;
        var proxyPipeWriter = proxyPipe.Writer;

        var proxyTask = client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyPipeReader.AsStream()), ct);

        while (!ct.IsCancellationRequested)
        {
            var contextReadResult = await pipeReader.ReadAsync(ct);
            var contextBuffer = contextReadResult.Buffer;

            if (contextBuffer.IsSingleSegment)
            {
                await ProcessBlockOptimizedAsync(proxyPipeWriter, contextBuffer.First, ct);
            }
            else
            {
                foreach (var segment in contextBuffer)
                {
                    await ProcessBlockOptimizedAsync(proxyPipeWriter, segment, ct);
                }
            }

            pipeReader.AdvanceTo(contextBuffer.End);

            if (contextReadResult.IsCompleted)
            {
                await proxyPipeWriter.FlushAsync(ct);
                await proxyPipeWriter.CompleteAsync();
                break;
            }
        }

        var response = await proxyTask;
        response.EnsureSuccessStatusCode();
        _ = Hasher.GetHashAndReset();
    }

    // [Benchmark]
    public async Task SendAndProcessUsingOptimizedPipesBigBuffer()
    {
        var bufferSize = 8 * 1024;
        using var client = new HttpClient();
        var pipeReader = await GetFilePipeReaderBigBufferAsync(bufferSize, ct);

        var proxyPipe = new Pipe();
        var proxyPipeReader = proxyPipe.Reader;
        var proxyPipeWriter = proxyPipe.Writer;

        var proxyTask = client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyPipeReader.AsStream(), bufferSize), ct);

        while (!ct.IsCancellationRequested)
        {
            var contextReadResult = await pipeReader.ReadAsync(ct);
            var contextBuffer = contextReadResult.Buffer;

            if (contextBuffer.IsSingleSegment)
            {
                ProcessBlockOptimized(proxyPipeWriter, contextBuffer.FirstSpan);
                await proxyPipeWriter.FlushAsync(ct);
            }
            else
            {
                foreach (var segment in contextBuffer)
                {
                    ProcessBlockOptimized(proxyPipeWriter, segment.Span);
                    await proxyPipeWriter.FlushAsync(ct);
                }
            }

            pipeReader.AdvanceTo(contextBuffer.End);

            if (contextReadResult.IsCompleted)
            {
                await proxyPipeWriter.FlushAsync(ct);
                await proxyPipeWriter.CompleteAsync();
                break;
            }
        }

        var response = await proxyTask;
        response.EnsureSuccessStatusCode();
        _ = Hasher.GetHashAndReset();
    }

    private async Task ProcessBlock(PipeWriter pipeWriter, ReadOnlyMemory<byte> memory, CancellationToken ct)
    {
        Hasher.AppendData(memory.Span);
        await pipeWriter.WriteAsync(memory, ct);
    }

    private void ProcessBlockOptimized(PipeWriter pipeWriter, ReadOnlySpan<byte> span)
    {
        Hasher.AppendData(span);
        span.CopyTo(pipeWriter.GetSpan(span.Length));
        pipeWriter.Advance(span.Length);
    }

    private async Task ProcessBlockOptimizedAsync(PipeWriter pipeWriter, ReadOnlyMemory<byte> memory,
        CancellationToken ct)
    {
        Hasher.AppendData(memory.Span);
        memory.CopyTo(pipeWriter.GetMemory(memory.Length));
        pipeWriter.Advance(memory.Length);
        await pipeWriter.FlushAsync(ct);
    }

    private async Task<Stream> GetFileStreamAsync(CancellationToken ct)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, Constants.FileCloudEndpoint);
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStreamAsync(ct);
    }

    private async Task<PipeReader> GetFilePipeReaderAsync(CancellationToken ct)
    {
        var stream = await GetFileStreamAsync(ct);
        return PipeReader.Create(stream);
    }
    
    private async Task<PipeReader> GetFilePipeReaderBigBufferAsync(int bufferSize, CancellationToken ct)
    {
        var stream = await GetFileStreamAsync(ct);
        // You also can use here a custom MemoryPool
        return PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: bufferSize));
    }
}