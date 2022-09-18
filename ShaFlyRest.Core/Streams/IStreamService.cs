namespace ShaFlyRest.Core.Streams;

/// <summary>
/// Handles all the logic with Streams
/// </summary>
public interface IStreamService
{
    /// <summary>
    /// Proxies file to the server
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Send(Stream fileStream, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using Stream with a simple buffer
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAndProcess(Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using Stream with configurable buffer size
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="bufferSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAndProcessCustomBuffer(Stream fileStream, int bufferSize, CancellationToken cancellationToken = default);
}