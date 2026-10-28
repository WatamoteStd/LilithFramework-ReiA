
using System.Net;
using System.Net.Sockets;

namespace Core.Owerwrite;

public class ReiEndPoint : EndPoint
{

    private SocketAddress _socketAdress;

    public ReiEndPoint(uint ip, ushort port)
    {
        
        IPEndPoint tempEp = new IPEndPoint(ip, port);
        
        _socketAdress = tempEp.Serialize();

    }

    public override SocketAddress Serialize() => _socketAdress;

    public override AddressFamily AddressFamily => _socketAdress.Family;

    public override EndPoint Create(SocketAddress socketAddress)
    {
        _socketAdress = socketAddress;
        return this;
    }

}