using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Handles pure UI visual (moving joystick sprite)
/// </summary>
public class JoystickView : MonoBehaviour
{
    // [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform joystick;
    [SerializeField] private Vector2 centerAnchoredPos;
    private float visualRadius;
    
    public void ConfigureJoystickVisual(float radiusPixels)
    {
        visualRadius = Mathf.Max(1f, radiusPixels);
    }

    public void SetCenter(Vector2 anchoredPosition)
    {
        centerAnchoredPos = anchoredPosition;
        // if (background != null)
        //     background.anchoredPosition = centerAnchoredPos;
        SnapHandleToCenter();
    }
    
    public void SetHandleFromValue(Vector2 normalizedValue)
    {
        // convert (-1..1) into (-radius..radius) pixels
        Vector2 handleOffset = normalizedValue * visualRadius;
        if (joystick != null)
            joystick.anchoredPosition = centerAnchoredPos + handleOffset;
    }
    
    public void SnapHandleToCenter() // resets joystick visually
    {
        if (joystick != null)
            joystick.anchoredPosition = centerAnchoredPos;
    }
    
    public Vector2 GetCurrentCenter()
    {
        return centerAnchoredPos;
    }
}
