using System;
using UnityEngine;
using UnityEngine.Serialization;

public class JoystickInput : MonoBehaviour
{
    public event Action<Vector2> OnJoystickMoveDirection;
    [SerializeField] private float holdThresholdSeconds = 0.05f;
    [SerializeField, Range(0f, 1f)] private float deadzone = 0.1f; 
    [SerializeField] private float maxDragRadius = 120f;

    [Header("Refs")]
    [SerializeField] private RectTransform joystickCanvasRoot;  // for screen to UI space conversion
    [SerializeField] private JoystickView joystickView;
    private JoystickModel joystickModel;

    // private press states
    private bool isPressed;          
    private float pressingTime;        
    private int fingerId = -1; 
    private Vector2 pressedPos;
    [SerializeField] private Vector2 centerAnchorPos;

    private void Awake()
    {
        // create and configure the model
        joystickModel = new JoystickModel();
        joystickModel.ConfigureJoystick(maxDragRadius, deadzone);
        // configure visual 
        if (joystickView != null)
            joystickView.ConfigureJoystickVisual(maxDragRadius);
        // center the joystick sprite
        if (joystickView != null)
            centerAnchorPos = joystickView.GetCurrentCenter();
    }

    private void Update()
    {
        // mouse or touch (easier to test in editor)
        if (Input.touchCount > 0)
        {
            UpdateTouch();
        }
        else
        {
            UpdateMouse();
        }
        // broadcast direction every frame
        if (OnJoystickMoveDirection != null)
            OnJoystickMoveDirection(joystickModel.direction);
    }

    private void UpdateTouch()
    {
        if (!isPressed)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    BeginPress(touch.fingerId, touch.position); // track fingerId from the first touch
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                // ignore other fingers for now
                if (touch.fingerId != fingerId) continue;
                // update touch states
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    Drag(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndPress();
                }

                break; 
            }
        }
    }

    private void UpdateMouse()
    {
        if (!isPressed)
        {
            if (Input.GetMouseButtonDown(0))
                BeginPress(-1, Input.mousePosition);
        }
        else
        {
            if (Input.GetMouseButton(0))
                Drag(Input.mousePosition);
            else
                EndPress();
        }
    }

    private void BeginPress(int fingerId, Vector2 screenPos)
    {
        // mark state
        isPressed = true;
        this.fingerId = fingerId;
        pressingTime = Time.unscaledTime; // unscaled so UI input isn't affected by slow motion/pause
        pressedPos = screenPos;
        // start model (state resets)
        joystickModel.Begin();
        if (joystickView != null)
            joystickView.SnapHandleToCenter();
    }

    private void Drag(Vector2 currentScreenPos)
    {
        // prevent accidental taps with a tiny hold time
        float heldFor = Time.unscaledTime - pressingTime;
        if (heldFor < holdThresholdSeconds)
        {
            // no input until threshold passes
            joystickModel.UpdateDrag(Vector2.zero);

            if (joystickView != null)
                joystickView.SnapHandleToCenter();

            return;
        }
        Vector2 currentAnchored = ScreenToAnchored(joystickCanvasRoot, currentScreenPos);
        // finger delta from joystick center
        Vector2 delta = currentAnchored - centerAnchorPos;
        // feed delta to model (clamps + normalizes + deadzone)
        joystickModel.UpdateDrag(delta);
        // inform visuals from model
        if (joystickView != null)
            joystickView.SetHandleFromValue(joystickModel.direction);
    }

    private void EndPress() // resets everything
    {
        isPressed = false;
        fingerId = -1;
        joystickModel.End();
        if (joystickView != null)
            joystickView.SnapHandleToCenter();
    }

    /// <summary>
    ///  // converts a screen pixel position to a local point inside the canvasRoot RectTransform
    /// </summary>
    /// <param name="canvasRoot"></param>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    private Vector2 ScreenToAnchored(RectTransform canvasRoot, Vector2 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRoot,
            screenPos,
            null,
            out localPoint
        );
        return localPoint;
    }
    
}
