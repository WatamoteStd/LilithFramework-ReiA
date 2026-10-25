
namespace Inetrfaces;

public interface INetworkTransport : IDisposable
{
    
    void Start();
    void Send(ReadOnlySpan<byte> data);
    void Stop();

}