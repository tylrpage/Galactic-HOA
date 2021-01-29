using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct NewChatMessage : BitSerializable
    {
        public const ushort id = 11;

        public int senderId;
        public string message;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddInt(senderId)
                .AddString(message);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            senderId = data.ReadInt();
            message = data.ReadString();
        }
    }
}

