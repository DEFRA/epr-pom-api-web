using WireMock.Server;

namespace WebApiGateway.UnitTests.Support;

public class WireMockContext : IDisposable
{
    public WireMockContext()
    {
        Server = WireMockServer.Start();
        BaseAddress = Server.Urls[0];
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };
    }
    
    public WireMockServer Server { get; }
    public string BaseAddress { get; }
    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Server.Stop();
        Server.Dispose();
        HttpClient.Dispose();
    }
}