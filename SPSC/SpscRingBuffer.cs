
using System.Runtime.InteropServices;

namespace SPSC;

[StructLayout(LayoutKind.Explicit, Size = 192)]
public unsafe struct SpscRingBuffer
{
    
    [FieldOffset(0)] private readonly byte* _buffer;
    [FieldOffset(8)] private readonly int _capacityMask;
    [FieldOffset(16)] private volatile int _head;
    [FieldOffset(80)] private volatile int _tail;

    public SpscRingBuffer(int capacity)
    {
        
        if(capacity <= 0 || (capacity & (capacity - 1)) != 0)
        {
            throw new ArgumentException("Capacity must be a positive power of 2");
        }

        _capacityMask = capacity - 1;   
        _head = 0;
        _tail = 0;

        _buffer = (byte*)NativeMemory.Alloc((nuint)capacity);


    }


    public bool Write(ReadOnlySpan<byte> packet)
    {
        
        int packetLength = packet.Length;
        int totalSize = 2 + packetLength;
        int capacity = _capacityMask + 1;

        if(_tail - _head + totalSize > capacity)
        {
            return false; // drop packet
        }

        int writeIndex = _tail & _capacityMask;

        *(ushort*)(_buffer + writeIndex) = (ushort)packetLength;

        fixed(byte* src = packet)
        {
            Buffer.MemoryCopy(src, _buffer + writeIndex + 2, capacity - (writeIndex + 2), packetLength);
        }

        _tail += totalSize;

        return true;

    }



    public void Dispose()
    {
        if(_buffer != null)
        {
            NativeMemory.Free(_buffer);
        }
    }

}