using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct Inputs
{
    public bool W;
    public bool A;
    public bool S;
    public bool D;
    public bool Space;
    public Vector2 MouseDir;

    public void Serialize(ref BitBuffer bitBuffer)
    {
        MouseDir = MouseDir.normalized;
        QuantizedVector2 qMouseDir = BoundedRange.Quantize(MouseDir, Constants.MOUSEDIR_BOUNDS);
        
        bitBuffer.AddBool(W)
            .AddBool(A)
            .AddBool(S)
            .AddBool(D)
            .AddBool(Space)
            .AddUInt(qMouseDir.x)
            .AddUInt(qMouseDir.y);
    }

    public void Deserialize(ref BitBuffer bitBuffer)
    {
        W = bitBuffer.ReadBool();
        A = bitBuffer.ReadBool();
        S = bitBuffer.ReadBool();
        D = bitBuffer.ReadBool();
        Space = bitBuffer.ReadBool();

        QuantizedVector2 qMouseDir = new QuantizedVector2(bitBuffer.ReadUInt(), bitBuffer.ReadUInt());
        MouseDir = BoundedRange.Dequantize(qMouseDir, Constants.MOUSEDIR_BOUNDS);
    }

    public static Inputs EmptyInputs()
    {
        return new Inputs();
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Inputs))
            return false;

        Inputs otherInputs = (Inputs) obj;
        return (otherInputs.W != W ||
                otherInputs.A != A ||
                otherInputs.S != S ||
                otherInputs.D != D ||
                otherInputs.Space != Space);
    }
}
