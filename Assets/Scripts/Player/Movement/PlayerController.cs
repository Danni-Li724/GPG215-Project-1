using UnityEngine;
using UnityEngine.Serialization;

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

    private void Update()
    {
        // create a command from the current input 
        MoveCommand command = new MoveCommand(latestJoystick);
        // execute the command with PlayerMovement
        if (movement != null)
            movement.Move(command.Direction);
    }

    private void HandleJoystickDirection(Vector2 direction)
    {
        // save latest joystick direction to apply each frame
        latestJoystick = direction;
    }
}
