
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Owerwrite;
using SPSC.Packets;

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

        _connectionToken = (ulong)Random.Shared.NextInt64();

        _spscBuffer = new SpscRingBuffer(memory);

    }

    public void ReceivePacket(ReadOnlySpan<byte> packetData)
    {
        
        PacketHeader header = MemoryMarshal.Read<PacketHeader>(packetData);
        
        int headerSize = Unsafe.SizeOf<PacketHeader>();
        ReadOnlySpan<byte> payload = packetData[headerSize..];

        _spscBuffer.Write(packetData);

    }


}