# System.IO.Pipelines
How to send file from a proxy to a remote server with System.IO.Pipelines library. Client needs to proxy a file from request to a remote server with and calculate SHA1 simultaneously.

Projects:
- ShaFlyRest.Client - client's proxy to send incoming file and calculate its hash on the fly.
- ShaFlyRest.Cloud - just a server who does something with the file and reads body data (in this scenario, it calculates SHA1 as well).
- ShaFlyRest.Core - core logic to work with Pipelines / Streams.
- ShaFlyRest.Benchmark - benchmark with the copy-paste code of ShaFlyRest.Client, because we are interested in the benchmarking of this part, but we cannot test the logic separately.

## How to test with Postman

1. Import the collection and environment from the `postman` folder. To avoid local network optimization, you can use [ngrok](https://ngrok.com) for proxy to ShaFlyRest.Client API.
2. Put your Ngrok address to the imported environment
3. Run Client + Cloud projects
4. Make requests from the imported collection (you need to select file from your machine).

## How to benchmark

1. Run Cloud project.
2. In Core project, go to `Constants.cs` and change:
   1. FilePath - to the file to be sent
   2. CloudEndpoint (if you want to use Ngrok).
3. Run the benchmark
    ```
    dotnet run -c Release (Windows)
    sudo dotnet run -c Release (Mac)
    ```
