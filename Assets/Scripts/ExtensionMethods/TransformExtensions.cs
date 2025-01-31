using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static Vector3 SetY(this Vector3 v, float newY)
    {
        v.y = newY;
        return v;
    }
    public static Vector3 SetZ(this Vector3 v, float newZ)
    {
        v.z = newZ;
        return v;
    }
    public static Vector3 SetX(this Vector3 v, float newX)
    {
        v.x = newX;
        return v;
    }

    public static bool HasTimeElapsedSince(this float checking, float intervalChecking) {
        return checking + intervalChecking < Time.time;
    }

    public static Vector3 directionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

    public static Vector3 directionTo(this Transform from, Vector3 to) => (to - from.position).normalized;

    public static Vector3 directionTo(this Vector3 from, Transform to) => (to.position - from).normalized;

    public static Vector3 directionTo(this Transform from, Transform to) => (to.position - from.position).normalized;

    public static float dotProduct(this Vector3 dirA, Vector3 dirB) => Vector3.Dot(dirA, dirB);

    public static bool areAligned(this Vector3 dirA, Vector3 dirB, float threshold = 0.1f) => Vector3.Dot(dirA, dirB) > 1.0f - threshold;
}
