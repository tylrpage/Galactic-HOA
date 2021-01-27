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
        public string currentAnimation;
        public bool spriteFlipped;
        public bool isPlaying;

        public void Serialize(ref BitBuffer data)
        {
            if (currentAnimation == null)
                currentAnimation = "idle";
            
            QuantizedVector2 qPosition = BoundedRange.Quantize(position, Constants.WORLD_BOUNDS);

            data.AddUInt(qPosition.x)
                .AddUInt(qPosition.y)
                .AddString(currentAnimation)
                .AddBool(spriteFlipped)
                .AddBool(isPlaying);
        }

        public void Deserialize(ref BitBuffer data)
        {
            QuantizedVector2 qPosition = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());
            position = BoundedRange.Dequantize(qPosition, Constants.WORLD_BOUNDS);
            currentAnimation = data.ReadString();
            spriteFlipped = data.ReadBool();
            isPlaying = data.ReadBool();
        }
    }

    public struct LeafState : BitSerializable
    {
        public Vector2 position;
        public Quaternion rotation;
        
        public void Serialize(ref BitBuffer data)
        {
            QuantizedVector2 qPosition = BoundedRange.Quantize(position, Constants.WORLD_BOUNDS);
            QuantizedQuaternion qRotation = SmallestThree.Quantize(rotation);

            data.AddUInt(qPosition.x)
                .AddUInt(qPosition.y)
                .AddUInt(qRotation.m)
                .AddUInt(qRotation.a)
                .AddUInt(qRotation.b)
                .AddUInt(qRotation.c);
        }

        public void Deserialize(ref BitBuffer data)
        {
            QuantizedVector2 qPosition = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());
            QuantizedQuaternion qRotation = new QuantizedQuaternion(data.ReadUInt(), data.ReadUInt(),data.ReadUInt(), data.ReadUInt());

            position = BoundedRange.Dequantize(qPosition, Constants.WORLD_BOUNDS);
            rotation = SmallestThree.Dequantize(qRotation);
        }
    }
    
    public struct PeerStates : BitSerializable
    {
        public const ushort id = 3;
        public Dictionary<int, PeerState> States;
        public Dictionary<int, LeafState> Leafs;
        public List<short> SegmentLeafCounts;

        public void Serialize(ref BitBuffer data)
        {
            data.AddUShort(id);
            
            data.AddInt(States.Count);
            foreach (var peerState in States)
            {
                data.AddInt(peerState.Key);
                peerState.Value.Serialize(ref data);
            }

            data.AddInt(Leafs.Count);
            foreach (var leaf in Leafs)
            {
                data.AddInt(leaf.Key);
                leaf.Value.Serialize(ref data);
            }

            data.AddShort((short)SegmentLeafCounts.Count);
            foreach (var segmentCount in SegmentLeafCounts)
            {
                data.AddShort(segmentCount);
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

            int leafCount = data.ReadInt();
            for (int i = 0; i < leafCount; i++)
            {
                int leafId = data.ReadInt();
                LeafState leafState = new LeafState();
                leafState.Deserialize(ref data);
                Leafs[leafId] = leafState;
            }

            int segmentCount = data.ReadShort();
            SegmentLeafCounts = new List<short>();
            for (int i = 0; i < segmentCount; i++)
            {
                SegmentLeafCounts.Add(data.ReadShort());
            }
        }
    }
}

