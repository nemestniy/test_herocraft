using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadGenerator : MonoBehaviour
{
    [Range(.05f, 1.5f)]
    public float Spacing = 1;
    public float RoadWidth = 1;
    public float BoardWidth = 0.25f;
    public float Delimiter;
    public bool AutoUpdate;
    public float Tiling = 1;
    public bool IsSalient;
    public bool IsBoard;
    public bool BoardInside;
    public bool UpFirstAndLastPoints;
    public LayerMask BumpsLayer;
    public bool MoveOnSide;
    public bool LeftSide;

    private Mesh lastMesh;
    private Transform pointToConnect;

    public Vector2[] MeshToPath(Mesh mesh)
    {
        var vertices = mesh.vertices;

        Vector2[] points = new Vector2[(int)(vertices.Length * .5f)];

        for(int i = 0; i < vertices.Length; i+=2)
        {
            var firstPoint = vertices[i];
            var secondPoint = vertices[i + 1];

            var dir = (secondPoint - firstPoint).normalized;
            var distance = Vector3.Distance(firstPoint, secondPoint);
            Vector3 point = firstPoint + dir * distance * 0.5f;
            Vector2 resultPoint = new Vector2(point.x, point.z);

            points[i / 2] = resultPoint;
        }

        return points;
    }

    public void UpdateRoad()
    {
        CurvePath path = GetComponent<PathCreator>().Path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(Spacing);
        if (MoveOnSide)
        {
            points = MovePointsOnSide(points, LeftSide, path.IsClosed);
        }
        lastMesh = CreateMesh(points, path.IsClosed, IsBoard);
        GetComponent<MeshFilter>().mesh = lastMesh;
        pointToConnect = GetComponent<PathCreator>().PointToConnect;
    }

    public Vector2[] MovePointsOnSide(Vector2[] points, bool leftSide, bool isClosed)
    {
        Vector2[] newPoints = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];

            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();

            Vector2 left = new Vector2(-forward.y, forward.x);

            if (leftSide)
                newPoints[i] = points[i] - left * Delimiter * .5f;
            else
                newPoints[i] = points[i] + left * Delimiter * .5f;
        }

        return newPoints;
    }

    private Mesh CreateMesh(Vector2[] points, bool isClosed, bool isBoard)
    {
        Vector3[] vertices = new Vector3[points.Length*2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numberTriangles = 2*(points.Length-1) + ((isClosed) ? 2:0);
        int [] triangles = new int[numberTriangles*3];
        int vertIndex = 0;
        int triIndex = 0;

        for(int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if(i < points.Length - 1 || isClosed)
            {
                forward += points[(i+1) % points.Length] - points[i];

            }
            if(i > 0 || isClosed)
            {
                forward += points[i] - points[(i-1 + points.Length) % points.Length];
            }
            forward.Normalize();

            Vector2 left = new Vector2(-forward.y, forward.x);


            if (IsBoard)
            {
                if (BoardInside)
                {
                    var point  = PointToXZ(points[i] - left * RoadWidth * .5f);
                    vertices[vertIndex] = point - Vector3.up * BoardWidth * .5f;
                    vertices[vertIndex + 1] = point + Vector3.up * BoardWidth * .5f;
                }
                else
                {
                    var point = PointToXZ(points[i] + left * RoadWidth * .5f);
                    vertices[vertIndex] = point + Vector3.up * BoardWidth * .5f;
                    vertices[vertIndex + 1] = point - Vector3.up * BoardWidth * .5f;
                }
            }
            else
            {
                vertices[vertIndex] = points[i] + left * RoadWidth * .5f;
                vertices[vertIndex + 1] = points[i] - left * RoadWidth * .5f;
            }

            if (IsSalient)
            {
                float y = CheckHeight(PointToXZ(points[i]));
                vertices[vertIndex] += Vector3.up * y;
                vertices[vertIndex + 1] += Vector3.up * y;
            }

            float completionPercent = i / (float)(points.Length-1);
            float v = 1 - Mathf.Abs(2*completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v*2);
            uvs[vertIndex+1] = new Vector2(1, v*2); 

            if(i < points.Length - 1 || isClosed)
            {
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 5] = (vertIndex + 3) % vertices.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();

        if(!IsBoard)
            PointsToXZ(vertices);

        if(pointToConnect != null && UpFirstAndLastPoints)
        {
            int endIndex = vertices.Length - 1;
            int startIndex = 0;
            float height = pointToConnect.position.y;
            float x = -2;
            float yMin = height * (1 / Mathf.Sqrt(2 * Mathf.PI)) * (1 / Mathf.Sqrt(Mathf.Exp(x * x)));
            while (x < 2 && endIndex > startIndex)
            {
                float y = height * (1 / Mathf.Sqrt(2 * Mathf.PI)) * (1/Mathf.Sqrt(Mathf.Exp(x * x))) - yMin;
                vertices[endIndex] += Vector3.up * y;
                vertices[endIndex - 1] += Vector3.up * y;
                //vertices[startIndex] += Vector3.up * height;
                //vertices[startIndex + 1] += Vector3.up * height;

                endIndex -= 2;
                //startIndex += 2;

                x += 0.01f;
            }
        }

        if(IsSalient)
            PutOnGround(vertices);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }

    private void PutOnGround(Vector3[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {
            var origin = points[i];
            float y = CheckHeight(points[i]) + origin.y;
            points[i] = new Vector3(origin.x, y, origin.z);
        }
    }

    private void PointsToXZ(Vector3[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {
            var origin = points[i];
            points[i] = new Vector3(origin.x, 0, origin.y);
        }
    }

    private Vector3 PointToXZ(Vector2 point)
    {
        return new Vector3(point.x, 0, point.y);
    }

    public Vector3[] PointsToXZ(Vector2[] points)
    {
        var newPoints = new Vector3[points.Length];
        for(int i = 0; i < points.Length; i++)
        {
            var origin = points[i];
            newPoints[i] = new Vector3(origin.x, 0, origin.y);
        }

        return newPoints;
    }

    private float CheckHeight(Vector3 origin)
    {
        RaycastHit hit;
        var position = new Vector3(origin.x, 250, origin.z);
        if(Physics.Raycast(position, Vector3.down, out hit, 1000, BumpsLayer))
        {
            return hit.point.y;
        }

        return origin.y;
    }
}
