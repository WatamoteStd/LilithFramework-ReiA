using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Core.Owerwrite;
using SPSC.Packets;

namespace Core;

public class ReiClient
{
    
    private Socket? _socket;
    private Thread? _receiveThread;
    private volatile bool _isWork = false;
    private ReiEndPoint _serverEndPoint;
    private readonly SpscRingBuffer _buffer;
    private byte[] _receiveBuffer = new byte[1500];

    public ReiClient(ReiEndPoint serverEnd, ushort bufferSize = 4096)
    {
        _serverEndPoint = serverEnd;
        _buffer = new SpscRingBuffer(bufferSize); // ^2 always
    }

    public void Start()
    {
        
        if(_socket != null)
        {
            _socket.Close();
        }

        _receiveThread = new Thread(Receive);
        _receiveThread.IsBackground = true;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(_serverEndPoint);
        _isWork = true;

        _receiveThread.Start();

    }

    public void Receive()
    {
        int headerSize = Unsafe.SizeOf<PacketHeader>();
        
        try
        {
            
            while(_isWork)
            {
            
                int writenBytes = _socket!.Receive(_receiveBuffer); 

                if(writenBytes < headerSize) continue;

                _buffer.Write(_receiveBuffer.AsSpan(0, writenBytes));

            }

        }
        catch(SocketException)
        {
            
        }

    }

    public bool TryReadPacket(Span<byte> destination, out int bytesRead)
    {
        bytesRead = 0;
        _buffer.Read(destination, out bytesRead);
        return bytesRead > 0;
    }   

    public void Stop()
    {
        _isWork = false;
        _socket?.Close();
        _socket?.Dispose();
        _receiveThread?.Join();
    }

}