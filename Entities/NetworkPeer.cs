
using System.Diagnostics;
using System.Net;

namespace Entities;

public class NetworkPeer
{
    
    public uint Id {get; private set;}
    public IPEndPoint EndPoint {get; private set;} 
    private readonly ulong _connectionToken;
    private long _connectionTime;
    private uint _packetsReceived;
    private uint _packetsSended;

    public NetworkPeer(IPEndPoint endPoint, uint id)
    {
        EndPoint = endPoint;
        Id = id;
        _connectionTime = Stopwatch.GetTimestamp();

        _connectionToken = (ulong)Math.Abs(Random.Shared.NextInt64());

    }

}