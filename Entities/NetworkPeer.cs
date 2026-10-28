
using System.Diagnostics;
using System.Net;
using Core.Owerwrite;

namespace Entities;

public class NetworkPeer
{
    
    public uint Id {get; private set;}
    public ReiEndPoint EndPoint {get; private set;} 
    private readonly ulong _connectionToken;
    private long _connectionTime;
    private uint _packetsReceived;
    private uint _packetsSended;
    private readonly SpscRingBuffer _spscBuffer;

    public NetworkPeer(ReiEndPoint endPoint, uint id, ushort memory)
    {
        EndPoint = endPoint;
        Id = id;
        _connectionTime = Stopwatch.GetTimestamp();

        _connectionToken = (ulong)Math.Abs(Random.Shared.NextInt64());

        _spscBuffer = new SpscRingBuffer(memory);

    }

}