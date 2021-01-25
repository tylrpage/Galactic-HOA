using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NonLinearTransforms
{
    public static float BounceClampTop(float t)
    {
        return 1f - Mathf.Abs(1 - t);
    }
}
