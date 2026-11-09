using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TapSwipeInput : MonoBehaviour
{
    public float swipeDp = 50f;
    public RunnerController runner; // the player object, dragged in via the Inspector

    // fingers that have already swiped, so one swipe only triggers once
    readonly HashSet<int> usedTouches = new HashSet<int>();

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        if (LifecycleGuard.IsPaused) return;

        float px = swipeDp * Mathf.Max(Screen.dpi, 160f) / 160f;

        foreach (var t in Touch.activeTouches)
        {
            int id = t.touchId;

            // new finger down: forget anything from before
            if (t.phase == TouchPhase.Began) { usedTouches.Remove(id); continue; }

            bool fingerUp = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;

            if (!usedTouches.Contains(id))
            {
                Vector2 d = t.screenPosition - t.startScreenPosition;

                // fire as soon as the finger has moved far enough
                if (d.magnitude >= px)
                {
                    usedTouches.Add(id);
                    DoSwipe(d);
                }
            }

            if (fingerUp) usedTouches.Remove(id);
        }
    }

    void DoSwipe(Vector2 d)
    {
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        {
            if (d.x > 0) runner.MoveRight();
            else runner.MoveLeft();
        }
        else
        {
            if (d.y > 0)
            {
                runner.Jump();
                Haptics.Pulse();
            }
            else runner.Slide();
        }
    }
}