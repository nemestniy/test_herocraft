using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator _pathCreator;
    CurvePath _path;

    static float _pointSize;
    int _selectedSegmentIndex = -1;

    void OnEnable()
    {
        _pathCreator = (PathCreator)target;
        if(_pathCreator.Path == null)
            _pathCreator.CreatePath();
        _path = _pathCreator.Path;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create new path"))
        {
            Undo.RecordObject(_pathCreator, "Path created");
            _pathCreator.CreatePath();
            _path = _pathCreator.Path;
        }

        if (GUILayout.Button("Connect to other curve"))
        {
            Undo.RecordObject(_pathCreator, "Path connected");
            _pathCreator.ConnectToOtherCurve();
        }

        if (GUILayout.Button("Connect to point"))
        {
            Undo.RecordObject(_pathCreator, "Path connected");
            _pathCreator.ConnectToPoint();
        }

        bool isClosed = GUILayout.Toggle(_path.IsClosed, "Closed");
        if(isClosed != _path.IsClosed)
        {
            Undo.RecordObject(_pathCreator, "Toogle closed");
            _path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(_path.AutoSetControlPoints, "Auto Set Control Points");
        if(autoSetControlPoints != _path.AutoSetControlPoints)
        {
            Undo.RecordObject(_pathCreator, "Toogle auto set controls");
            _path.AutoSetControlPoints = autoSetControlPoints;
        }

        GUILayout.Label("\nSize of red points");
        _pointSize = GUILayout.HorizontalSlider(_pointSize, 0f, 10f);
        GUILayout.Space(10);

        if(EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI() 
    {
        Input(); 
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0;
        float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if(_selectedSegmentIndex != -1)
            {
                Undo.RecordObject(_pathCreator, "Split segment");
                _path.SplitSegment(mousePosition, _selectedSegmentIndex);
            }
            else if(!_path.IsClosed)
            {
                Undo.RecordObject(_pathCreator, "Add segment");
                _path.AddSegment(mousePosition);
            }
        }

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistanceToAnchor = 0.5f;
            int closestAnchorIndex = -1;

            for(int i = 0; i < _path.NumberPoints; i += 3)
            {
                float distance = Vector2.Distance(mousePosition, GetPositionXZ(_path[i]));
                if(distance < minDistanceToAnchor)
                {
                    minDistanceToAnchor = distance;
                    closestAnchorIndex = i;
                }
            }

            if(closestAnchorIndex != -1)
            {
                Undo.RecordObject(_pathCreator, "Delete segment");
                _path.DeleteSegment(closestAnchorIndex);
            }

            HandleUtility.AddDefaultControl(0);
        }

        if(guiEvent.type == EventType.MouseMove)
        {
            float minDistanceToSegment = _pointSize * 2;
            int newSelectedSegmentIndex = -1;

            for(int i = 0; i < _path.NumberSegments; i++)
            {
                Vector3[] points = SegmentToXZ(_path.GetPointsInSegment(i));
                float distance = HandleUtility.DistancePointBezier(mousePosition, points[0], points[3], points[1], points[2]);
                if(distance < minDistanceToSegment)
                {
                    minDistanceToSegment = distance;
                    newSelectedSegmentIndex = i;
                }
            }

            if(newSelectedSegmentIndex != _selectedSegmentIndex)
            {
                _selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }

    void Draw()
    {
        Handles.color = Color.black;
        for(int i = 0; i < _path.NumberSegments; i++)
        {
            Vector3[] points = SegmentToXZ(_path.GetPointsInSegment(i));
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[3], points[2]);
            Color segmentColor = (i == _selectedSegmentIndex && Event.current.shift) ? Color.red : Color.green;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
        }
        
        Handles.color = Color.red;

        for(int i = 0; i < _path.NumberPoints; i++)
        {
            Vector3 newPosition = Handles.FreeMoveHandle(GetPositionXZ(_path[i]), Quaternion.identity, _pointSize, Vector2.zero, Handles.CylinderHandleCap);
            if(_path.XZ(i) != newPosition)
            {
                Undo.RecordObject(_pathCreator, "MovePoint");
                Vector2 position = new Vector2(newPosition.x, newPosition.z);
                _path.MovePoint(i, position);
            }
        }
    }

    private Vector3 GetPositionXZ(Vector2 position)
    {
        return new Vector3(position.x, 0, position.y);
    }

    private Vector3[] SegmentToXZ(Vector2[] points)
    {
        var newPoints = new Vector3[points.Length];

        for(int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Vector3(points[i].x, 0 ,points[i].y);
        }

        return newPoints;

    }
}
