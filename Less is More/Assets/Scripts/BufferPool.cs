using System;
using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

// Create a one-time allocation buffer pool
static class BufferPool {
    [ThreadStatic]
    private static BitBuffer bitBuffer;
    [ThreadStatic]
    private static byte[] byteBuffer;

    public static BitBuffer GetBitBuffer() {
        if (bitBuffer == null)
            bitBuffer = new BitBuffer(1024);

        return bitBuffer;
    }
    
    public static byte[] GetByteBuffer() {
        if (byteBuffer == null)
            byteBuffer = new byte[1024];

        return byteBuffer;
    }
}