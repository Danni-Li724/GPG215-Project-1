using UnityEngine;

public struct MoveCommand
{
    // normalized direction from joystick
    public Vector2 Direction;

    public MoveCommand(Vector2 direction)
    {
        Direction = direction;
    }
}