using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShaFlyRest.Core.Pipelines;

namespace ShaFlyRest.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class PipesController : ControllerBase
{
    private readonly IPipelinesService _pipelinesService;
    private readonly Stopwatch _stopwatch = new();

    public PipesController(IPipelinesService pipelinesService)
    {
        _pipelinesService = pipelinesService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _pipelinesService.Send(Request.BodyReader, cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }

    [HttpPost("process")]
    public async Task<IActionResult> SendAndProcess(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _pipelinesService.SendAndProcess(Request.BodyReader, cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
    
    [HttpPost("process/span")]
    public async Task<IActionResult> SendAndProcessOptimizedSpan(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _pipelinesService.SendAndProcessOptimizedSpan(Request.BodyReader, cancellation);
        _stopwatch.Stop();

        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
    
    [HttpPost("process/memory")]
    public async Task<IActionResult> SendAndProcessOptimizedMemory(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _pipelinesService.SendAndProcessOptimizedMemory(Request.BodyReader, cancellation);
        _stopwatch.Stop();

        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
}