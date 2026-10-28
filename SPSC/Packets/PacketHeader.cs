using System.Runtime.InteropServices;

namespace SPSC.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PacketHeader
{
    private readonly byte _flags;
    public readonly uint SequenceId;
    public readonly DeliveryTypes Type => (DeliveryTypes)(_flags & 0x03);
    public readonly bool IsACK => (_flags & 0x04) != 0;

    public PacketHeader(DeliveryTypes type, uint id, bool isAck = false)
    {
        SequenceId = id;
        byte flags = (byte)((byte)type & 0x03);

        if(isAck)
        {
            flags |= (1 << 2);
        }
        _flags = flags; 
    }

}