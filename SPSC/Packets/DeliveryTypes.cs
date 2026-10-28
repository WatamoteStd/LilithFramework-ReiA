
using System.Runtime.InteropServices;

namespace SPSC.Packets;
public enum DeliveryTypes : byte
{
    UnreliableUnordered = 0,
    UnreliableOrdered = 1,
    ReliableUnordered = 2,
    ReliableOrdered = 3

}