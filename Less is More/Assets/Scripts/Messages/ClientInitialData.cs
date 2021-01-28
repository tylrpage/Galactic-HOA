using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct ClientInitialData : BitSerializable
    {
        public const ushort id = 9;

        public string DisplayName;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddString(DisplayName);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            DisplayName = data.ReadString();
        }
    }
}

