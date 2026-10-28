
using System.Net;
using System.Net.Sockets;
using System.Security;
using Core.Owerwrite;
using Entities;
using Entities.Network;

namespace Core;

public class ReiHost
{
    
    private  Socket? _socket;
    private  Thread? _thread;
    private volatile bool _isRunning;

    private readonly int _port;
    private readonly Dictionary<NetAdress, NetworkPeer> _peers = new();

    public ReiHost(int port)
    {
        _port = port;
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

        while(_isRunning)
        {
            
            try
            {
                
                int receivedBytes = _socket!.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref remoteEp);

                if(receivedBytes > 0)
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