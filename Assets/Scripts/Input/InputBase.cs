using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputBase : MonoBehaviour
{
    public abstract int TouchSide();
    public abstract Vector2 TouchDelta();
}
