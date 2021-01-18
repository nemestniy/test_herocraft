using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadGenerator))]
public class RoadEditor : Editor
{
    private RoadGenerator roadGenerator;

    private void OnSceneGUI() 
    {
        if(roadGenerator.AutoUpdate && Event.current.type == EventType.Repaint)
        {
            roadGenerator.UpdateRoad();
        }    
    }

    private void OnEnable() 
    {
        roadGenerator = (RoadGenerator)target;    
    }
}
