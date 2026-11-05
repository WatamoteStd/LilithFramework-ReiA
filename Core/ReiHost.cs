using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Core.Owerwrite;
using Entities;
using Entities.Network;
using ReiA;
using SPSC.Packets;

namespace Core;

public class ReiHost
{

    private  Socket? _socket;
    private  Thread? _thread;
    private volatile bool _isRunning;

    private readonly int _port;
    private readonly Dictionary<NetAdress, NetworkPeer> _peers = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private volatile NetworkPeer[] _cachedPeersArray = new NetworkPeer[0];

    private uint _sequenceId = 0;
    private readonly int _headerSize;

    private uint _nextPeerId = 0;

    public readonly ushort PerPeerMemory = 4096; // def value
    public int PeerCount => _peers.Count;

    public ReiHost(int port, ushort peerMemory)
    {
        _port = port;
        PerPeerMemory = peerMemory;
        _headerSize = Unsafe.SizeOf<PacketHeader>();
    }

    public void Start()
    {

        if(_isRunning) return;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.ReceiveBufferSize = 64 * 1024;
        _socket.SendBufferSize = 64 * 1024;

        _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
        _isRunning = true;

        _thread = new Thread(ReceiveLoop)
        {
            IsBackground = true,
            Name = "ReiAlloc-ReceiveLoop"
        };
        _thread.Start();

        Console.WriteLine($"[ReiHost] Started on port:{_port}");

    }

    private void ReceiveLoop()
    {

        byte[] buffer = new byte[1500];
        EndPoint remoteEp = new ReiEndPoint(0, 0);
        int headerSize = Unsafe.SizeOf<PacketHeader>();

        while (_isRunning)
        {

            try
            {

                int receivedBytes = _socket!.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref remoteEp);

                if (receivedBytes >= headerSize)
                {
                    HandleIncomingData(buffer.AsSpan(0, receivedBytes), (ReiEndPoint)remoteEp);
                }

            }
            catch (SocketException ex) when (_isRunning)
            {
                Console.WriteLine($"[ReiHost] Socket error: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                break;
            }

        }

    }

    private void HandleIncomingData(ReadOnlySpan<byte> data, ReiEndPoint senderEndPoint)
    {

        NetAdress remoteAddr = senderEndPoint.GetNetAdress();
        
        if (!_peers.TryGetValue(remoteAddr, out var peer))
        {
            peer = new NetworkPeer(new ReiEndPoint(remoteAddr.Ip, remoteAddr.Port), _nextPeerId, PerPeerMemory);
            _lock.EnterWriteLock();
            try
            {
                _peers[remoteAddr] = peer;
                _cachedPeersArray = _peers.Values.ToArray();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            _nextPeerId++;
        }

        peer.ReceivePacket(data);

    }

    /// <summary>
    /// Under testing
    /// </summary>
    /// <returns></returns>
    public NetworkPeer[] GetPeers()
    {
        return _cachedPeersArray;
    }


    /// <summary>
    /// Send UnreliableUnordered packet with data limit 1495 bytes.
    /// </summary>
    /// <param name="peer"></param>
    /// <param name="data"></param>
    public void SendTo(NetworkPeer peer, ReadOnlySpan<byte> data)
    {
        Socket? socket = _socket;
        
        if(socket is null || !_isRunning || data.Length is 0 or > ReiProtocol.PAYLOAD_MAXIMUM_SIZE) return;


        Span<byte> buffer = stackalloc byte[_headerSize + data.Length];

        uint seq = Interlocked.Increment(ref _sequenceId);
        PacketHeader header = new PacketHeader(DeliveryTypes.UnreliableUnordered, seq);

        MemoryMarshal.Write(buffer[0.._headerSize], in header);
        data.CopyTo(buffer[_headerSize..]);

        try
        {
            
            socket.SendTo(buffer, peer.EndPoint);

        }
        catch(SocketException)
        {

        }
        catch(ObjectDisposedException)
        {

        }

    }



    public void Stop()
    {

        _isRunning = false;
        _socket?.Close();
        _socket?.Dispose();
        _thread?.Join();
        Console.WriteLine("[ReiHost] Stopped.");

    }

}
