using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MovementSpeed;
    public float Height;

    private Transform target;
    private Transform pointView;
    private InputBase input;
    public void Initialize(Transform target)
    {
        this.target = target;

        pointView = new GameObject(nameof(pointView)).transform;
        pointView.position = target.position;
        transform.parent = pointView;
        transform.localPosition = Vector3.up * Height;

        input = InputManager.Instance.Input;
    }

    private void Update()
    {
        pointView.position = Vector3.Lerp(pointView.position, target.position, MovementSpeed * Time.deltaTime);

        transform.LookAt(target.transform);
        var delta = input.TouchDelta();
        pointView.localEulerAngles += new Vector3(delta.y, delta.x, 0) * 0.1f;
        transform.position = new Vector3(transform.position.x,
                                         Mathf.Clamp(transform.position.y, 0, Height),
                                         transform.position.z);
    }
}
