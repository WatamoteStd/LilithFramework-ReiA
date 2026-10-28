
using System.Net;
using System.Net.Sockets;
using Entities.Network;

namespace Core.Owerwrite;

public class ReiEndPoint : EndPoint
{

    private SocketAddress _socketAdress;

    public ReiEndPoint(uint ip, ushort port)
    {
        
        IPEndPoint tempEp = new IPEndPoint(ip, port);
        
        _socketAdress = tempEp.Serialize();

    }

    public NetAdress GetNetAdress()
    {
        
        ushort port = (ushort)((_socketAdress[2] << 8) | _socketAdress[3]);

        uint ip = (uint)(_socketAdress[4] | (_socketAdress[5] << 8) | (_socketAdress[6] << 16) | (_socketAdress[7] << 24));

        return new NetAdress(ip, port);

    }

    public override SocketAddress Serialize() => _socketAdress;

    public override AddressFamily AddressFamily => _socketAdress.Family;

    public override EndPoint Create(SocketAddress socketAddress)
    {
        _socketAdress = socketAddress;
        return this;
    }

}