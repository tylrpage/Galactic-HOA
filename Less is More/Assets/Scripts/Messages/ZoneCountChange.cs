using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct ZoneCountChange : BitSerializable
    {
        public const ushort id = 6;

        public short NewZoneCount;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddShort(NewZoneCount);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();

            NewZoneCount = data.ReadShort();
        }
    }
}

