using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct Inputs
{
    public bool W;
    public bool A;
    public bool S;
    public bool D;
    public bool Space;

    public void Serialize(ref BitBuffer bitBuffer)
    {
        bitBuffer.AddBool(W)
            .AddBool(A)
            .AddBool(S)
            .AddBool(D)
            .AddBool(Space);
    }

    public void Deserialize(ref BitBuffer bitBuffer)
    {
        W = bitBuffer.ReadBool();
        A = bitBuffer.ReadBool();
        S = bitBuffer.ReadBool();
        D = bitBuffer.ReadBool();
        Space = bitBuffer.ReadBool();
    }

    public static Inputs EmptyInputs()
    {
        return new Inputs();
    }
}
