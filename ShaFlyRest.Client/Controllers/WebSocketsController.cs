using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShaFlyRest.Core.Pipelines;
using ShaFlyRest.Core.WebSockets;

namespace ShaFlyRest.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketsController : ControllerBase
{
    private readonly IWebSocketService _webSocketService;
    private readonly Stopwatch _stopwatch = new();

    public WebSocketsController(IWebSocketService webSocketService)
    {
        _webSocketService = webSocketService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> SendAndProcess(CancellationToken cancellation)
    {
        _stopwatch.Start();
        await _webSocketService.SendAndProcess(Request.BodyReader.AsStream(), cancellation);
        _stopwatch.Stop();
        
        return Ok(_stopwatch.Elapsed.TotalSeconds);
    }
}