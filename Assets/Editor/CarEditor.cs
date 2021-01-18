using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Car))]
public class CarEditor : Editor
{
    private Car car;

    private void OnEnable()
    {
        car = (Car)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        GUILayout.Space(10);

        if(GUILayout.Button("Choose configuration file of car"))
        {
            string path = EditorUtility.OpenFilePanel("Choose Json file", Directory.GetCurrentDirectory(), "json");
            using(StreamReader reader = new StreamReader(path))
            {
                var data = reader.ReadToEnd();
                car.Data = JsonUtility.FromJson<CarData>(data);
            }
        }

        if (GUILayout.Button("Save configuration file of car"))
        {
            using(StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "//" + car.transform.name + ".json"))
            {
                writer.Write(JsonUtility.ToJson(car.Data));
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }
}
