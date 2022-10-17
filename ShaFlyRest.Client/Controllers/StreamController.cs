using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShaFlyRest.Core.Pipelines;
using ShaFlyRest.Core.Streams;

namespace ShaFlyRest.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamController : ControllerBase
{
    private readonly IStreamService _streamService;
    private readonly Stopwatch _stopwatch = new();

    public StreamController(IStreamService streamService)
    {
        _streamService = streamService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _streamService.Send(Request.BodyReader.AsStream(), cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }

    [HttpPost("process")]
    public async Task<IActionResult> SendAndProcess(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _streamService.SendAndProcess(Request.BodyReader.AsStream(), cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
    
    [HttpPost("process/buffer")]
    public async Task<IActionResult> SendAndProcessCustomBuffer([FromQuery] int bufferSize, CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _streamService.SendAndProcessCustomBuffer(Request.BodyReader.AsStream(), bufferSize, cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
}