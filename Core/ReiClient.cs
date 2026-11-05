using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Owerwrite;
using ReiA;
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

    private readonly int _headerSize;
    private uint _sequenceId;

    public ReiClient(ReiEndPoint serverEnd, ushort bufferSize = 4096)
    {
        _serverEndPoint = serverEnd;
        _buffer = new SpscRingBuffer(bufferSize); // ^2 always
        _headerSize = Unsafe.SizeOf<PacketHeader>();
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
        
        try
        {
            
            while(_isWork)
            {
            
                int writenBytes = _socket!.Receive(_receiveBuffer);

                if(writenBytes is 0 or > ReiProtocol.PAYLOAD_MAXIMUM_SIZE || writenBytes < _headerSize) continue;

                PacketHeader header = MemoryMarshal.Read<PacketHeader>(_receiveBuffer.AsSpan(0, _headerSize));

                ReadOnlySpan<byte> payload = _receiveBuffer[_headerSize..writenBytes];

                switch(header.Type)
                {
                    
                    case DeliveryTypes.UnreliableUnordered: _buffer.Write(payload); break;
                    default: break;

                }


            }

        }
        catch(SocketException)
        {
            _isWork = false;
        }
        catch(ObjectDisposedException )
        {
            _isWork = false;
        }

    }

    public bool TryReadPacket(Span<byte> destination, out int bytesRead)
    {
        bytesRead = 0;
        _buffer.Read(destination, out bytesRead);
        return bytesRead > 0;
    }   


    /// <summary>
    /// Send UnreliableUnordered packet to server EndPoint.
    /// With data limit 1495 bytes.
    /// </summary>
    /// <param name="data"></param>
    public void Send(ReadOnlySpan<byte> data)
    {
        Socket socket = _socket!;
        if(socket is null || !_isWork || data.Length is 0 or > ReiProtocol.PAYLOAD_MAXIMUM_SIZE) return;

        Span<byte> buffer = stackalloc byte[_headerSize + data.Length];

        uint seq = Interlocked.Increment(ref _sequenceId);
        PacketHeader header = new PacketHeader(DeliveryTypes.UnreliableUnordered, seq);

        MemoryMarshal.Write(buffer[0.._headerSize], in header);
        data.CopyTo(buffer[_headerSize..]);

        try
        {
            socket.Send(buffer);
        }
        catch(SocketException ex)
        {
            Console.WriteLine($"[ReiClient] : Socket droped. Error:{ex.Message}");
        }
        catch(ObjectDisposedException)
        {

        }

    }

    public void Stop()
    {
        _isWork = false;
        _socket?.Close();
        _socket?.Dispose();
        _receiveThread?.Join();
    }

}
