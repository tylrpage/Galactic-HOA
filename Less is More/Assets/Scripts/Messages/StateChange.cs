using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct StateChange : BitSerializable
    {
        public const ushort id = 7;

        public short StateId;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddShort(StateId);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            StateId = data.ReadShort();
        }
    }
}

