using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

namespace Messages
{
    public struct PeerState : BitSerializable
    {
        public Vector2 position;

        public void Serialize(ref BitBuffer data)
        {
            QuantizedVector2 qPosition = BoundedRange.Quantize(position, Constants.WORLD_BOUNDS);

            data.AddUInt(qPosition.x)
                .AddUInt(qPosition.y);
        }

        public void Deserialize(ref BitBuffer data)
        {
            QuantizedVector2 qPosition = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());
            position = BoundedRange.Dequantize(qPosition, Constants.WORLD_BOUNDS);
        }
    }
    
    public struct PeerStates : BitSerializable
    {
        public const ushort id = 3;
        public Dictionary<int, PeerState> States;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
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
            int count = data.ReadInt();
            for (int i = 0; i < count; i++)
            {
                int peerId = data.ReadInt();
                PeerState peerState = new PeerState();
                peerState.Deserialize(ref data);
                States[peerId] = peerState;
            }
        }
    }
}

