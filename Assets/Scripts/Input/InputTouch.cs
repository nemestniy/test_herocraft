using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTouch : InputBase
{
    public override Vector2 TouchDelta()
    {
        if (Input.touchCount == 0) return Vector2.zero;

        var touch = Input.GetTouch(0);

        if(touch.phase == TouchPhase.Moved)
        {
            return touch.deltaPosition;
        }

        return Vector2.zero;
    }

    public override int TouchSide()
    {
        if (Input.touchCount == 0) return 0;

        var touch = Input.GetTouch(0);

        if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)
        {
            if (touch.position.x >= Screen.width * 0.5f) return 1;
            else return -1;
        }

        return 0;
    }
}
