using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct Inputs
{
    public Vector2 Position;
    public bool Space;
    public Vector2 MouseDir;

    public void Serialize(ref BitBuffer bitBuffer)
    {
        MouseDir = MouseDir.normalized;
        QuantizedVector2 qPosition = BoundedRange.Quantize(Position, Constants.WORLD_BOUNDS);
        QuantizedVector2 qMouseDir = BoundedRange.Quantize(MouseDir, Constants.MOUSEDIR_BOUNDS);
        
        bitBuffer
            .AddUInt(qPosition.x)
            .AddUInt(qPosition.y)
            .AddBool(Space)
            .AddUInt(qMouseDir.x)
            .AddUInt(qMouseDir.y);
    }

    public void Deserialize(ref BitBuffer bitBuffer)
    {
        QuantizedVector2 qPosition = new QuantizedVector2(bitBuffer.ReadUInt(), bitBuffer.ReadUInt());
        Position = BoundedRange.Dequantize(qPosition, Constants.WORLD_BOUNDS);
        
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
        return (otherInputs.Position == Position &&
                otherInputs.Space == Space);
    }
}
