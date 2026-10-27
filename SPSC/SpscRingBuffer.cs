

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public unsafe struct SpscRingBuffer : IDisposable
{
    [FieldOffset(0)] ulong Count;
    [FieldOffset(8)] ushort Capacity;
    [FieldOffset(16)] byte* MemoryPtr;
    [FieldOffset(32)] byte* PtrReader;
    [FieldOffset(96)] byte* PtrWriter;

    public SpscRingBuffer(ushort capacity)
    {
        
        if ((capacity & (capacity - 1)) != 0 || capacity < 4096)
        {
            throw new ArgumentException($"[SpscRingBuffer| Initialization] Invalid capacity. ( It might be always ^2 and now lower than 4096 bytes.)");
        }

        MemoryPtr = (byte*)NativeMemory.Alloc(capacity);

        Capacity = capacity;
        Count = 0;

        PtrReader = MemoryPtr;
        PtrWriter = MemoryPtr;

    }

    public void Write(ReadOnlySpan<byte> packetSpan)
    {


        if (Count + (ulong)packetSpan.Length + 2 > Capacity)
        {
            Console.WriteLine($"[Packet Drop] Packet bigger than capacity of the buffer. {packetSpan.Length} bytes");
            return;
        }
        
        if (PtrWriter + packetSpan.Length + 2 > MemoryPtr + Capacity)
        {
            ulong tailSpace = (ulong)((MemoryPtr + Capacity) - PtrWriter);
            if (tailSpace >= 2)
            {
                *(ushort*)PtrWriter = 0;
            }
            Count += tailSpace;
            PtrWriter = MemoryPtr;
        }

        *(ushort*)PtrWriter = (ushort)packetSpan.Length;
        PtrWriter += 2;

        fixed(byte* packetPtr = packetSpan)
        {
            
            Buffer.MemoryCopy(packetPtr, PtrWriter, Capacity, packetSpan.Length);
            PtrWriter += packetSpan.Length;

            Count += (ulong)packetSpan.Length + 2;

        }

    }

    public void Read(Span<byte> readBuffer)
    {
        
        if(PtrReader == PtrWriter && Count == 0) return;

        if (PtrReader == MemoryPtr + Capacity - 1)
        {
            Count -= 1;
            PtrReader = MemoryPtr;
        }

        ushort packetLength = *(ushort*)PtrReader;

        if(packetLength == 0)
        {
            ulong skippedTail = (ulong)((MemoryPtr + Capacity) - PtrReader);
            Count -= skippedTail;

            PtrReader = MemoryPtr;
            packetLength = *(ushort*)PtrReader;
        }

        PtrReader += 2;

        fixed(byte* bufferPtr = readBuffer)
        {
            
            Buffer.MemoryCopy(PtrReader, bufferPtr, readBuffer.Length, packetLength);

        }

        PtrReader += packetLength;
        Count -= (ulong)packetLength + 2;

    }


    public void Dispose()
    {
        
        if(PtrReader != null)
        {
            NativeMemory.Free(MemoryPtr);
            MemoryPtr = null;
            PtrReader = null;
            PtrWriter = null;
        }

    }


}