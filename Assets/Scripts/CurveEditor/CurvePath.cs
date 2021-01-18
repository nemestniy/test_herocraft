using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CurvePath
{
    [SerializeField, HideInInspector]
    private List<Vector2> _points;
    [SerializeField, HideInInspector]
    private bool _isClosed;
    [SerializeField, HideInInspector]
    private bool _autoSetControlPoints;

    public CurvePath(Vector2 centre)
    {
        _points = new List<Vector2>
        {
            centre + Vector2.left * 10f,
            centre + (Vector2.left + Vector2.up)*5f,
            centre + (Vector2.right + Vector2.down)*5f,
            centre + Vector2.right * 10f
        };
    }

    public void AddSegment(Vector3 anchorPosition)
    {
        Vector2 position = new Vector2(anchorPosition.x, anchorPosition.z);
        _points.Add(_points[_points.Count-1] * 2 - _points[_points.Count-2]);
        _points.Add((_points[_points.Count-1] + position)*.5f);
        _points.Add(position);

        if(_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(_points.Count - 1);
        }
    }

    public void AddSegment(Vector2[] points)
    {
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        _points.Add(2*points[1] - points[0]);
        _points.Add(points[1]);

        if (_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(_points.Count - 1);
        }
    }

    public void SplitSegment(Vector3 position, int segmentIndex)
    {
        Vector2 anchorPosition = new Vector2(position.x, position.z);

        _points.InsertRange(segmentIndex*3 + 2, new Vector2[]
        {
            Vector2.zero, anchorPosition, Vector2.zero 
        });

        if(_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex*3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex*3 + 3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if(NumberPoints > 2 || !_isClosed && NumberSegments > 1)
        {
            if(anchorIndex == 0)
            {
                if(_isClosed)
                {
                    _points[_points.Count - 1] = _points[2];     
                }
                _points.RemoveRange(0, 3);
            }
            else if(anchorIndex == _points.Count - 1 && !_isClosed)
            {
                _points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                _points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(_points[0]);
        Vector2 previousPoint = _points[0];
        float distanceSinceLastEvenPoint = 0;

        for(int i = 0; i < NumberSegments; i++)
        {
            Vector2[] p = GetPointsInSegment(i);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2])+ Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);

            float t = 0;
            while(t <= 1)
            {
                t += 1f/divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                distanceSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (distanceSinceLastEvenPoint >= spacing)
                {
                    float overshootDistance = distanceSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance; 
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    distanceSinceLastEvenPoint = overshootDistance;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public Vector2[] GetPointsInSegment(int index)
    {
        return new Vector2[]
        {
            _points[index*3],
            _points[index*3 + 1],
            _points[index*3 + 2],
            _points[LoopIndex(index*3 + 3)]
            };
    }

    public Vector2 this[int index]
    {
        get
        {
            return _points[index];
        }
    }

    public bool IsClosed
    {
        get
        {
            return _isClosed;
        }
        set
        {
            if(_isClosed != value)
            {
                 _isClosed = value;

                if(_isClosed)
                {
                    _points.Add(_points[_points.Count-1] * 2 - _points[_points.Count-2]);
                    _points.Add(_points[0] * 2 - _points[1]);
                    if(_autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(_points.Count-3);
                    }
                }
                else
                {
                    _points.RemoveRange(_points.Count - 2, 2);
                    if(_autoSetControlPoints)
                        AutoSetStartAndEndControls();
                }
                    }
                }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return _autoSetControlPoints;
        }
        set
        {
            if(_autoSetControlPoints != value)
            {
                _autoSetControlPoints = value;
                if(_autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    public Vector3 XZ(int index)
    {
        return new Vector3(_points[index].x, 0 ,_points[index].y);
    }

    public int NumberPoints
    {
        get
        {
            return _points.Count;
        }
    }

    public int NumberSegments
    {
        get
        {
            return _points.Count/3;
        }
    }

    public void MovePoint(int index, Vector2 position)
    {
        Vector2 deltaMove = position - _points[index];
        _points[index] = position;

        if(index % 3 == 0 || !_autoSetControlPoints)
        {
            _points[index] = position;
        

            if(_autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(index);
            }
            else
            {
                if(index % 3 == 0)
                {
                    if(index + 1 < _points.Count || _isClosed)
                        _points[LoopIndex(index + 1)] += deltaMove;
                    
                    if(index - 1 >= 0 || _isClosed)
                        _points[LoopIndex(index - 1)] += deltaMove;
                }
                else
                {
                    bool nextPointIsAnchor = (index + 1) % 3 == 0;
                    int correspondingControlIndex = nextPointIsAnchor ? index + 2 : index - 2;
                    int anchorIndex = nextPointIsAnchor ? index + 1 : index - 1;

                    if(correspondingControlIndex >= 0 && correspondingControlIndex < _points.Count || _isClosed)
                    {
                        float distance = (_points[LoopIndex(anchorIndex)] - _points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector2 direction = (_points[LoopIndex(anchorIndex)] - position).normalized;
                        _points[LoopIndex(correspondingControlIndex)] = _points[LoopIndex(anchorIndex)] + direction * distance;
                    }
                }
            }
        }
    }

    private void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPosition = _points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if(anchorIndex - 3 >= 0 || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex - 3)] - anchorPosition;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if(anchorIndex + 3 < _points.Count || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex + 3)] - anchorPosition;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for(int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if(controlIndex >= 0 && controlIndex < _points.Count || _isClosed)
            {
                _points[LoopIndex(controlIndex)] = anchorPosition + dir * neighbourDistances[i] * .5f;
            }
        }
    }

    private void AutoSetStartAndEndControls()
    {
        if(!_isClosed)
        {
            _points[1] = (_points[0] + _points[2]) * .5f;
            _points[_points.Count-2] = (_points[_points.Count - 1] + _points[_points.Count - 3]) *.5f;
        }
    }

    private void AutoSetAllControlPoints()
    {
        for(int i = 0; i < _points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }

        AutoSetStartAndEndControls();
    }

    private void AutoSetAllAffectedControlPoints(int updateAcnhorIndex)
    {
        for(int i = updateAcnhorIndex - 3; i < updateAcnhorIndex + 3; i += 3)
        {
            if(i >= 0 && i < _points.Count || _isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls(); 
    }

    private int LoopIndex(int index)
    {
        return (index + _points.Count) % _points.Count;
    }
}
