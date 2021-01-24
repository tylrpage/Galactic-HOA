using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct NewPlayer : BitSerializable
    {
        public const ushort id = 4;

        public int TheirId;
        public PeerState State;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddInt(TheirId);
            State.Serialize(ref data);
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            TheirId = data.ReadInt();
            PeerState state = new PeerState();
            state.Deserialize(ref data);
            State = state;
        }
    }
}

