using System.Runtime.InteropServices;

namespace SPSC.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PacketHeader
{
    private readonly byte _flags;
    public readonly uint SequenceId;
    public readonly PacketTypes Type => (PacketTypes)(_flags & 0x03);
    public readonly bool IsACK => (_flags & 0x04) != 0;

    public PacketHeader(PacketTypes type, uint id, bool isAck = false)
    {
        SequenceId = id;
        _flags = (byte)((byte)type & 0x03);

        if(isAck)
        {
            _flags |= (1 << 2);
        }
    }

}