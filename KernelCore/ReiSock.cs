
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KernelCore;

public unsafe class ReiSock
{
    private const string LibName = "ws2_32.dll";
    private const uint SIO_GET_MULTIPLE_EXTENSION_FUNCTION_POINTER = 0xC8000024;

    
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

    [StructLayout(LayoutKind.Sequential)]
    public struct RIO_EXTENSION_FUNCTION_TABLE
    {
        public uint cbSize;
        public nint RIOReceive;
        public nint RIOReceiveEx;
        public nint RIOSend;
        public nint RIOSendEx;
        public nint RIOCloseCompletionQueue;
        public nint RIOCreateCompletionQueue;
        public nint RIOCreateRequestQueue;
        public nint RIODequeueCompletion;
        public nint RIODeregisterBuffer;
        public nint RIONotify;
        public nint RIORegisterBuffer;
        public nint RIOResizeCompletionQueue;
        public nint RIOResizeRequestQueue;

    }

    public static RIO_EXTENSION_FUNCTION_TABLE LoadRioFunctions(nint socket)
    {
        
        Guid rioGuid = new Guid("6f8350ec-f0ac-4456-a1fd-1bb1f961e68e");
        byte[] guidBytes = rioGuid.ToByteArray();

        int tableSize = Marshal.SizeOf<RIO_EXTENSION_FUNCTION_TABLE>();

        nint pTableOut = Marshal.AllocHGlobal(tableSize);
        var initTable = new RIO_EXTENSION_FUNCTION_TABLE
        {
            cbSize = (uint)tableSize
        };
        Marshal.StructureToPtr(initTable, pTableOut, false);

        uint bytesReturend = 0;
        int result;

        fixed(byte* pGuid = guidBytes)
        {
            
            result = WSAIoctl(
                socket,
                SIO_GET_MULTIPLE_EXTENSION_FUNCTION_POINTER,
                pGuid,
                (uint)guidBytes.Length,
                (byte*)pTableOut,
                (uint)tableSize,
                out bytesReturend,
                IntPtr.Zero,
                IntPtr.Zero
            );

        }

        if (result == -1)
        {
            int error = Marshal.GetLastWin32Error();
            Marshal.FreeHGlobal(pTableOut);
            throw new InvalidOperationException($"Can't reach RIO-TABLE. Win32 error: {error}");
        }

        RIO_EXTENSION_FUNCTION_TABLE table = Marshal.PtrToStructure<RIO_EXTENSION_FUNCTION_TABLE>(pTableOut);

        Marshal.FreeHGlobal(pTableOut);

        return table;

 
    }


}