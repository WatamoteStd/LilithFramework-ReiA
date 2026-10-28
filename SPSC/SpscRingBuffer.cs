using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public unsafe sealed class SpscRingBuffer : IDisposable
{
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

        PtrReader = MemoryPtr;
        PtrWriter = MemoryPtr;

    }

    public void Write(ReadOnlySpan<byte> packetSpan)
    {

        int requiredSize = packetSpan.Length + 2;
        if(requiredSize > Capacity)
        {
            Console.WriteLine($"[PacketDrop] Packet was bigger than actual capacity of buffer. Size:{requiredSize} Capacity:{Capacity}");
            return;
        }

        long corruped = PtrWriter >= PtrReader
            ? (PtrWriter - PtrReader)
            : (Capacity - (PtrReader - PtrWriter));

        long freeSpace = Capacity - corruped;

        if(requiredSize > freeSpace)
        {
            Console.WriteLine($"[PacketDrop] Buffer full. Needed:{requiredSize} Free:{freeSpace}");
            return;

        }

        if((MemoryPtr + Capacity) - PtrWriter < requiredSize)
        {
            *(ushort*)PtrWriter = 0;
            PtrWriter = MemoryPtr;
        }

        if(PtrReader - PtrWriter < requiredSize)
        {
            return;
        }

        *(ushort*)PtrWriter = (ushort)packetSpan.Length;
        PtrWriter += 2;

        fixed(byte* packetPtr = packetSpan)
        {

            Buffer.MemoryCopy(packetPtr, PtrWriter, Capacity, packetSpan.Length);
            PtrWriter += packetSpan.Length;

        }

    }

    public void Read(Span<byte> readBuffer)
    {

        if (PtrReader == PtrWriter) return;

        ushort packetLength = *(ushort*)PtrReader; 

        if(packetLength == 0)
        {
            PtrReader = MemoryPtr;
            packetLength = *(ushort*)PtrReader;

            if (PtrReader == PtrWriter) return;
        }

        if(packetLength > readBuffer.Length) return; // small buffer size

        PtrReader += 2;

        fixed(byte* bufferPtr = readBuffer)
        {
            Buffer.MemoryCopy(PtrReader, bufferPtr, readBuffer.Length, packetLength);
        }

        PtrReader += packetLength;

    }


    public void Dispose()
    {

        if (PtrReader != null)
        {
            NativeMemory.Free(MemoryPtr);
            MemoryPtr = null;
            PtrReader = null;
            PtrWriter = null;
        }
        GC.SuppressFinalize(this);

    }


}
