
namespace Entities.Network;

public readonly struct NetAdress : IEquatable<NetAdress>
{
    
    public readonly uint Ip;
    public readonly ushort Port;

    public NetAdress(uint ip, ushort port)
    {
        Ip = ip;
        Port = port;
    }

    public bool Equals(NetAdress other) => Ip == other.Ip && Port == other.Port;
    public override int GetHashCode() => HashCode.Combine(Ip, Port);

}