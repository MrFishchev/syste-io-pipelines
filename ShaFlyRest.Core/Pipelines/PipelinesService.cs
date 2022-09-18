using System.IO.Pipelines;
using System.Security.Cryptography;

namespace ShaFlyRest.Core.Pipelines;

public class PipelinesService : IPipelinesService
{
    private readonly Pipe _proxyPipe = new();
    private readonly HttpClient _client = new();
    private readonly IncrementalHash _hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);

    private PipeReader ProxyReader => _proxyPipe.Reader;
    private PipeWriter ProxyWriter => _proxyPipe.Writer;
    
    public async Task Send(PipeReader fileReader, CancellationToken cancellation = default)
    {
        var response = await _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(fileReader.AsStream()), cancellation);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendAndProcess(PipeReader fileReader, CancellationToken cancellation = default)
    {
        var proxyTask = _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(ProxyReader.AsStream()), cancellation);
        while (true)
        {
            var fileReadResult = await fileReader.ReadAsync(cancellation);
            var fileReadBuffer = fileReadResult.Buffer;

            if (fileReadBuffer.IsSingleSegment)
            {
                await ProcessBlock(fileReadBuffer.First, cancellation);
            }
            else
            {
                foreach (var segment in fileReadBuffer)
                {
                    await ProcessBlock(segment, cancellation);
                }
            }

            fileReader.AdvanceTo(fileReadBuffer.End);
            if (fileReadResult.IsCompleted)
            {
                await ProxyWriter.FlushAsync(cancellation);
                await ProxyWriter.CompleteAsync();
                break;
            }
        }

        var scanResponse = await proxyTask;
        scanResponse.EnsureSuccessStatusCode();
        _ = _hasher.GetHashAndReset();
    }

    public async Task SendAndProcessOptimizedSpan(PipeReader fileReader, CancellationToken cancellation = default)
    {
        var proxyTask = _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(ProxyReader.AsStream()),
            cancellation);
        while (true)
        {
            var fileReadResult = await fileReader.ReadAsync(cancellation);
            var fileReadBuffer = fileReadResult.Buffer;

            if (fileReadBuffer.IsSingleSegment)
            {
                ProcessBlockOptimized(fileReadBuffer.FirstSpan);
                await ProxyWriter.FlushAsync(cancellation);
            }
            else
            {
                foreach (var segment in fileReadBuffer)
                {
                    ProcessBlockOptimized(segment.Span);
                    await ProxyWriter.FlushAsync(cancellation);
                }
            }

            fileReader.AdvanceTo(fileReadBuffer.End);
            if (fileReadResult.IsCompleted)
            {
                await ProxyWriter.FlushAsync(cancellation);
                await ProxyWriter.CompleteAsync();
                break;
            }
        }

        var scanResponse = await proxyTask;
        scanResponse.EnsureSuccessStatusCode();
        _ = _hasher.GetHashAndReset();
    }

    public async Task SendAndProcessOptimizedMemory(PipeReader fileReader, CancellationToken cancellation = default)
    {
        var proxyTask = _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(ProxyReader.AsStream()),
            cancellation);
        while (true)
        {
            var fileReadResult = await fileReader.ReadAsync(cancellation);
            var fileReadBuffer = fileReadResult.Buffer;

            if (fileReadBuffer.IsSingleSegment)
            {
                await ProcessBlockOptimizedAsync(fileReadBuffer.First, cancellation);
                await ProxyWriter.FlushAsync(cancellation);
            }
            else
            {
                foreach (var segment in fileReadBuffer)
                {
                    await ProcessBlockOptimizedAsync(segment, cancellation);
                    await ProxyWriter.FlushAsync(cancellation);
                }
            }

            fileReader.AdvanceTo(fileReadBuffer.End);
            if (fileReadResult.IsCompleted)
            {
                await ProxyWriter.FlushAsync(cancellation);
                await ProxyWriter.CompleteAsync();
                break;
            }
        }

        var scanResponse = await proxyTask;
        scanResponse.EnsureSuccessStatusCode();
        _ = _hasher.GetHashAndReset();
    }

    #region Private Methods

    private async Task ProcessBlock(ReadOnlyMemory<byte> memory, CancellationToken cancellation = default)
    {
        _hasher.AppendData(memory.Span);
        await ProxyWriter.WriteAsync(memory, cancellation);
    }
    
    private void ProcessBlockOptimized(ReadOnlySpan<byte> span)
    {
        _hasher.AppendData(span);
        span.CopyTo(ProxyWriter.GetSpan(span.Length));
        ProxyWriter.Advance(span.Length);
    }
    
    private async Task ProcessBlockOptimizedAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellation = default)
    {
        _hasher.AppendData(memory.Span);
        memory.CopyTo(ProxyWriter.GetMemory(memory.Length));
        ProxyWriter.Advance(memory.Length);
        await ProxyWriter.FlushAsync(cancellation);
    }
    
    #endregion
}