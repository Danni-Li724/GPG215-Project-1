using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private JoystickInput joystickInput;

    private PlayerMovement movement;
    private Vector2 latestJoystick;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (joystickInput != null)
            joystickInput.OnJoystickMoveDirection += HandleJoystickDirection;
    }

    private void OnDisable()
    {
        if (joystickInput != null)
            joystickInput.OnJoystickMoveDirection -= HandleJoystickDirection;
    }

    private void HandleJoystickDirection(Vector2 direction)
    {
        latestJoystick = direction;
    }

    private void Update()
    {
        MoveCommand command = new MoveCommand(latestJoystick);

        if (movement != null)
            movement.Move(command.Direction);
    }
}

