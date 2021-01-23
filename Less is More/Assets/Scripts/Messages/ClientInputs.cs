using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct ClientInputs : BitSerializable
    {
        public const ushort id = 2;
        
        public Inputs inputs;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            inputs.Serialize(ref data);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            inputs.Deserialize(ref data);
        }
    }
}

