﻿using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using UnityEngine;

public static class Constants
{
    public static readonly ushort GAME_PORT = 9003;
    public static BoundedRange[] WORLD_BOUNDS = new BoundedRange[]
    {
        new BoundedRange(-10f, 10f, 0.0001f),
        new BoundedRange(-10f, 10f, 0.0001f),
        new BoundedRange(-10f, 10f, 0.0001f),
    };
    public static BoundedRange[] MOUSEDIR_BOUNDS = new BoundedRange[]
    {
        new BoundedRange(-1f, 1f, 0.005f),
        new BoundedRange(-1f, 1f, 0.005f),
        new BoundedRange(-1f, 1f, 0.005f),
    };

    public static readonly int TICK = 30;
    public static readonly float STEP = 1f / TICK;

    public static readonly ushort FINE_PER_LEAF = 10;
    public static readonly short PLAYERS_NEEDED = 2;
    public static readonly short ROUND_BEGIN = 5;
    public static readonly short FLYING_LENGTH = 4;
    public static readonly short ROUND_LENGTH = 15;
    public static readonly short LANDING_LENGTH = 4;
}
