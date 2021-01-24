using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct PlayerDisconnected : BitSerializable
    {
        public const ushort id = 5;

        public int TheirId;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddInt(TheirId);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            TheirId = data.ReadInt();
        }
    }
}

