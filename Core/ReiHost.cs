using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using Core.Owerwrite;
using Entities;
using Entities.Network;
using SPSC.Packets;

namespace Core;

public class ReiHost
{
    
    private  Socket? _socket;
    private  Thread? _thread;
    private volatile bool _isRunning;

    private readonly int _port;
    private readonly Dictionary<NetAdress, NetworkPeer> _peers = new();
    private uint _nextPeerId = 0;

    public readonly ushort PerPeerMemory = 4096; // def value

    public ReiHost(int port, ushort peerMemory)
    {
        _port = port;
        PerPeerMemory = peerMemory;
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

        while(_isRunning)
        {
            
            try
            {
                
                int receivedBytes = _socket!.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref remoteEp);

                if(receivedBytes >= headerSize)
                {
                    HandleIncomingData(buffer.AsSpan(0, receivedBytes), (ReiEndPoint)remoteEp);
                }

            }
            catch(SocketException ex) when (_isRunning)
            {
                Console.WriteLine($"[ReiHost] Socket error: {ex.Message}");
            }
            catch(ObjectDisposedException)
            {
                break;
            }

        }

    }

    private void HandleIncomingData(ReadOnlySpan<byte> data, ReiEndPoint senderEndPoint)
    {
        
        NetAdress remoteAddr = senderEndPoint.GetNetAdress();
        if(!_peers.TryGetValue(remoteAddr, out var peer))
        {
            peer = new NetworkPeer(senderEndPoint, _nextPeerId, PerPeerMemory);
            _peers[remoteAddr] = peer;
            _nextPeerId++;
        }

        peer.ReceivePacket(data);

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