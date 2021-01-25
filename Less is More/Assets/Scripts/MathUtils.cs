using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector2 PolarToRect(float theta, float radius)
    {
        return new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
    }

    public static float DegreeToRadians(float degrees)
    {
        return degrees * ((2 * Mathf.PI) / 360f);
    }
}
