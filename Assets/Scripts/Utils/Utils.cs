using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utils
{
    public static float AngleBetweenPoints(Vector2 firstPoint, Vector2 secondPoint, Vector2 thirdPoint)
    {
        var firstVector = secondPoint - firstPoint;
        var secondVector = thirdPoint - secondPoint;

        var angle = Mathf.Acos((firstVector.x * secondVector.x + firstVector.y * secondVector.y)
                                / (firstVector.magnitude * secondVector.magnitude))
                                * Mathf.Rad2Deg;

        return angle;
    }

    public static float AngleBetweenVectors(Vector2 firstVector, Vector2 secondVector)
    {
        var angle = Mathf.Acos((firstVector.x * secondVector.x + firstVector.y * secondVector.y)
                                / (firstVector.magnitude * secondVector.magnitude))
                                * Mathf.Rad2Deg;

        return angle;
    }

    public static string ReadFile(string path)
    {
        using (StreamReader reader = new StreamReader(path))
        {
            return reader.ReadToEnd();
        }
    }

    public static void WriteFile(string path, string data)
    {
        using(StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(data);
        }
    }
}
