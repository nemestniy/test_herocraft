using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public CurvePath Path;

    public float PointsSpace = 5.0f;
    public float SteepAngle = 30.0f;
    
    [Space]

    public PathCreator PathToConnect;
    public Transform PointToConnect;

    private Vector2[] points;
    private List<Vector3> Checkpoints;

    private void Awake()
    {
        if (PointsSpace > 0)
            points = Path.CalculateEvenlySpacedPoints(PointsSpace);
    }

    public void CreatePath()
    {
        Path = new CurvePath(transform.position);
    }

    public void ConnectToOtherCurve()
    {
        if (PathToConnect == null || Path.IsClosed) return;

        var nextPath = PathToConnect.Path;
        Vector2[] pointToConnect = { nextPath[1], nextPath[0]};

        Path.AddSegment(pointToConnect);
    }

    public void ConnectToPoint()
    {
        if (PointToConnect == null || Path.IsClosed) return;

        Path.AddSegment(PointToConnect.position);
    }

    public List<Vector3> CalculateCheckpoints()
    {
        var checkpoints = new List<Vector3>();

        if (points == null || points.Length == 0) return checkpoints;

        for (int i = 1; i < points.Length - 1; i+=2)
        {
            var angle = Utils.AngleBetweenPoints(points[i - 1], points[i], points[i + 1]);

            if (angle > SteepAngle)
                checkpoints.Add(points[i].GetXZ());
        }

        if (!checkpoints.Contains(points[points.Length - 3].GetXZ()))
            checkpoints.Add(points[points.Length - 3].GetXZ());

        return checkpoints;
    }

    public Vector3[] GetPath()
    {
        var path = new Vector3[points.Length];

        for(int i = 0; i < path.Length; i++)
        {
            path[i] = points[i].GetXZ();
        }

        return path;
    }

    private void OnDrawGizmosSelected()
    {
        if(PointsSpace > 0)
            points = Path.CalculateEvenlySpacedPoints(PointsSpace);

        for (int i = 1; i < points.Length - 1; i+=2)
        {
            var angle = Utils.AngleBetweenPoints(points[i - 1], points[i], points[i + 1]);

            if (angle > SteepAngle)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawSphere(points[i].GetXZ(), 5);
        }
    }
}
