using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            var objects = FindObjectsOfType<T>();
            if (objects.Length < 1)
            {
                Debug.LogError("Object of " + nameof(T) + " is missing.");
            }
            else
            {
                instance = objects[0];
                if (objects.Length > 1)
                    Debug.LogError("There are " + nameof(T) + " objects more than 1.");
            }

            return instance;
        }
    }
}
