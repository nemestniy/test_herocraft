using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 GetXZ(this Vector2 vector)
    {
        return new Vector3(vector.x, 0, vector.y);
    }

    public static Vector2 XZToV2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }
}
