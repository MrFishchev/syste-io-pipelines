namespace ShaFlyRest.Core.WebSockets;

public interface IWebSocketService
{
    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using WebSocket and SendChunked
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAndProcess(Stream fileStream, CancellationToken cancellationToken = default);
}