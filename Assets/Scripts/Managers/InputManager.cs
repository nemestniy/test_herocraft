using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public InputBase Input { get; private set; }

    private Car car;
    public void Initialize(Car car)
    {
        Input = GetComponent<InputBase>();
        this.car = car;
    }

    private void Update()
    {
        var side = Input.TouchSide();

        if (side == 1)
            car.Accelerate();
        else if (side == -1)
            car.Brake();
        else if (side == 0)
            car.SetMiddleSpeed();
    }
}
