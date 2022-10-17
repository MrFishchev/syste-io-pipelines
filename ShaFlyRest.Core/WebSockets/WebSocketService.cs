using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
#pragma warning disable SYSLIB0014

namespace ShaFlyRest.Core.WebSockets;

[SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
public class WebSocketService : IWebSocketService
{
    private readonly IncrementalHash _hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
    
    public async Task SendAndProcess(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
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
            while ((read = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                _hasher.AppendData(buffer);
                await proxyStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }

        using var response = await request.GetResponseAsync();
    }
}