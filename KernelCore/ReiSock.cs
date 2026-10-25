
using System.Runtime.InteropServices;

namespace KernelCore;

public unsafe class ReiSock
{
    private const string LibName = "ws2_32.dll";
    
    [DllImport(LibName, SetLastError = true)]
    public static extern nint WSASocketW(int af, int type, int protocol, nint lpProtocolInfo, uint g, uint dwFlags);

    [DllImport(LibName, SetLastError = true)]
    public static extern int closeSocket(nint a);

    [DllImport(LibName, SetLastError = true)]
    public static extern int WSAIoctl(
        nint s,
        uint dwIoControlCode,
        byte* lpvInBuffer,
        uint cbInBuffer,
        byte* lpvOutBuffer,
        uint cbOutBuffer,
        out uint lpcbBytesReturned,
        nint lpOverlapped,
        nint lpCompletionRoutine
    );


}