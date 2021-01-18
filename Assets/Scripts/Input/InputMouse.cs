using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMouse : InputBase
{
    private Vector2 currentPosition = Vector2.zero;
    private Vector2 lastPosition = Vector2.zero;

    public override Vector2 TouchDelta()
    {
        currentPosition = Input.mousePosition;
        var deltaPosition = currentPosition - lastPosition;
        lastPosition = currentPosition;
        return deltaPosition; 
    }

    public override int TouchSide()
    {
        if(Input.GetMouseButton(0))
        {
            var mouseCoord = Input.mousePosition;
            if (mouseCoord.x - Screen.width * 0.5f > 0) return 1;
            else return -1;
        }

        return 0;
    }
}
