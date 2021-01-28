using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct ClearLeafs : BitSerializable
    {
        public const ushort id = 8;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
        }
    }
}

