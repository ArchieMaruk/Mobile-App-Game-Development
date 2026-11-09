using UnityEngine;

//Made by: Claude





// Animates the burger with code: running legs, body bounce and a wobbly tomato tongue.
// Put this on the empty "Burger" object and drag in the four parts.
// The slide squish comes from RunnerController, which squashes the Player while sliding.
public class BurgerAnimator : MonoBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] Transform tongue;
    [SerializeField] Transform legL;
    [SerializeField] Transform legR;

    [Header("Run")]
    public float runSpeed = 12f;    // how fast the legs swing
    public float legSwing = 35f;    // degrees forward/back
    public float bobHeight = 0.05f; // body bounce

    [Header("Tongue")]
    public float tongueTilt = 25f;   // how far it hangs down
    public float tongueWobble = 12f; // how much it flaps

    float bodyY, phase;

    void Awake() => bodyY = body.localPosition.y;

    void Update()
    {
        // deltaTime is 0 while paused, so the animation freezes with the game
        phase += Time.deltaTime * runSpeed;
        float s = Mathf.Sin(phase);

        legL.localRotation = Quaternion.Euler( s * legSwing, 0f, 0f);
        legR.localRotation = Quaternion.Euler(-s * legSwing, 0f, 0f);

        Vector3 p = body.localPosition;
        p.y = bodyY + Mathf.Abs(s) * bobHeight;
        body.localPosition = p;

        tongue.localRotation = Quaternion.Euler(tongueTilt + Mathf.Sin(phase * 2f) * tongueWobble, 0f, 0f);
    }
}
