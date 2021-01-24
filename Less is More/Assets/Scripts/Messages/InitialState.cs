﻿using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct InitialState : BitSerializable
    {
        public const ushort id = 1;

        public int YourId;
        public Dictionary<int, PeerState> States;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddInt(YourId);
            data.AddInt(States.Count);
            foreach (var peerState in States)
            {
                data.AddInt(peerState.Key);
                peerState.Value.Serialize(ref data);
            }
        }

        public void Deserialize(ref BitBuffer data)
        {
            data.ReadUShort();
            
            YourId = data.ReadInt();
            int count = data.ReadInt();
            for (int i = 0; i < count; i++)
            {
                int peerId = data.ReadInt();
                
                if (!States.ContainsKey(peerId))
                    States[peerId] = new PeerState();
                States[peerId].Deserialize(ref data);
            }
        }
    }
}
