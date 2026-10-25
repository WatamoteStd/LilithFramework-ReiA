
using System.Runtime.InteropServices;

namespace KernelCore;

public unsafe class RioNativeBuffer : IDisposable
{
    
    public byte* Ptr {get; private set;}
    public uint Size {get; private set;}
    public nint BufferId {get; private set;}

    private readonly nint _registerFuncPtr;
    private readonly nint _deregisterFuncPtr;

    public RioNativeBuffer(nint registerFuncPtr, uint size)
    {
        Size = size;
        _registerFuncPtr = registerFuncPtr;

        Ptr = (byte*)NativeMemory.Alloc(size);
        BufferId = ReiSock.RegisterMemoryBuffer(_registerFuncPtr, Ptr, Size);
    }

    public void Dispose()
    {
        
        if(Ptr != null)
        {
            NativeMemory.Free(Ptr);
            Ptr = null;
        }
        GC.SuppressFinalize(this);

    }

}