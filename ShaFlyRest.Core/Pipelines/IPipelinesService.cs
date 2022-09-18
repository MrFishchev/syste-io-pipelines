using System.IO.Pipelines;

namespace ShaFlyRest.Core.Pipelines;

/// <summary>
/// Handles all the logic with System.IO.Pipelines
/// </summary>
public interface IPipelinesService
{
    /// <summary>
    /// Proxies file to the server
    /// </summary>
    /// <param name="fileReader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Send(PipeReader fileReader, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using PipeWriter.WriteAsync (not optimal)
    /// </summary>
    /// <param name="fileReader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAndProcess(PipeReader fileReader, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using Span.CopyTo(PipeWriter.GetSpan) synchronously
    /// </summary>
    /// <param name="fileReader"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task SendAndProcessOptimizedSpan(PipeReader fileReader, CancellationToken cancellation = default);

    /// <summary>
    /// Proxies file and calculates hash simultaneously,
    /// by using Memory.CopyTo(PipeWriter.GetMemory) asynchronously
    /// </summary>
    /// <param name="fileReader"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task SendAndProcessOptimizedMemory(PipeReader fileReader, CancellationToken cancellation = default);
}