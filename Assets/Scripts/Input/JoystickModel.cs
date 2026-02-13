using UnityEngine;

public class JoystickModel
{
   public Vector2 direction {get; private set;}
   public bool isActive {get; private set;}
   private float radius; // defines of area of movement for the joystick
   private float deadzone;

   public void ConfigureJoystick(float radiusPx, float deadzonePx)
   {
      radius = Mathf.Max(1, radiusPx); // none zero radius
      deadzone = Mathf.Clamp01(deadzonePx);
   }

   public void Begin()
   {
      isActive = true;
      direction = Vector2.zero;
   }

   public void UpdateDrag(Vector2 fingerDelta)
   {
      if (!isActive) return;
      Vector2 clamped = Vector2.ClampMagnitude(fingerDelta, radius);
      Vector2 normalized = clamped / radius;
      if (normalized.magnitude < deadzone)
         normalized = Vector2.zero;
      direction = normalized;
   }

   public void End()
   {
      isActive = false;
      direction = Vector2.zero;
   }
}
