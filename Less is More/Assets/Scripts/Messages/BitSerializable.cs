using System;
using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public interface BitSerializable
{
    int Serialize(ref BitBuffer data);
    void Deserialize(ref BitBuffer data);
}

public static class Writer
{
    public static ArraySegment<byte> SerializeToByteSegment(BitSerializable message)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        int size = message.Serialize(ref bitBuffer);
        byte[] byteBuffer = BufferPool.GetByteBuffer();
        bitBuffer.ToArray(byteBuffer);
        return new ArraySegment<byte>(byteBuffer, 0, size);
    }
}

