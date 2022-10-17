using System.Security.Cryptography;

namespace ShaFlyRest.Core.Streams;

public class StreamService : IStreamService
{
    private readonly HttpClient _client = new();
    private readonly IncrementalHash _hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);

    public async Task Send(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(fileStream),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendAndProcess(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[4096];
        await using var proxyStream = new MemoryStream();

        int read;
        while ((read = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            _hasher.AppendData(buffer);
            await proxyStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        proxyStream.Seek(0, SeekOrigin.Begin);
        var response = await _client.PostAsync(Constants.ScanCloudEndpoint,
            new StreamContent(proxyStream), cancellationToken);

        response.EnsureSuccessStatusCode();
        _ = _hasher.GetHashAndReset();
    }

    public async Task SendAndProcessCustomBuffer(Stream fileStream, int bufferSize, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[bufferSize];
        await using var proxyStream = new MemoryStream();

        int read;
        while ((read = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            _hasher.AppendData(buffer);
            await proxyStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        proxyStream.Seek(0, SeekOrigin.Begin);
        var response = await _client.PostAsync(Constants.ScanCloudEndpoint, new StreamContent(proxyStream, bufferSize),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        _ = _hasher.GetHashAndReset();
    }
}