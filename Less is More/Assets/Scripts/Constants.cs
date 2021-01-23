using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using UnityEngine;

public static class Constants
{
    public static readonly ushort GAME_PORT = 9003;
    public static BoundedRange[] WORLD_BOUNDS = new BoundedRange[]
    {
        new BoundedRange(-10f, 10f, 0.05f),
        new BoundedRange(-10f, 10f, 0.05f),
        new BoundedRange(-10f, 10f, 0.05f),
    };

    public static readonly int TICK = 60;
    public static readonly float STEP = 1f / TICK;
}
