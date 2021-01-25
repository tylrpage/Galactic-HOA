using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector2 PolarToRect(float theta, float radius)
    {
        return new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
    }

    public static Vector2 RectToPolar(Vector2 rectPosition)
    {
        float r = Mathf.Sqrt(Mathf.Pow(rectPosition.x, 2) + Mathf.Pow(rectPosition.y, 2));
        
        float theta = Mathf.Atan2(rectPosition.y, rectPosition.x);
        if (theta < 0)
            theta += Mathf.PI * 2;
        
        return new Vector2(theta, r);
    }

    public static float DegreeToRadians(float degrees)
    {
        return degrees * ((2 * Mathf.PI) / 360f);
    }

    public static float RadiansToDegree(float radians)
    {
        return radians * 180f / Mathf.PI;
    }
}
