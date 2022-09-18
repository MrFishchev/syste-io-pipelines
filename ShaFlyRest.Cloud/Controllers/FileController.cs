using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShaFlyRest.Core;

namespace ShaFlyRest.Cloud.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFile(CancellationToken ct)
    {
        var bytes = await System.IO.File.ReadAllBytesAsync(Constants.FilePath, ct);
        return File(bytes, MediaTypeNames.Application.Octet);
    }
}