using UnityEngine;

public class BurgerAnimator : MonoBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] Transform tongue;
    [SerializeField] Transform legL;
    [SerializeField] Transform legR;

    [Header("Mode")]
    public bool idle;              // tick in the Menu, leave off in the Game

    [Header("Run")]
    public float runSpeed = 12f;
    public float legSwing = 35f;
    public float bobHeight = 0.05f;

    [Header("Idle")]
    public float idleSpeed = 2.5f;
    public float breathe = 0.04f;   // how much it squashes
    public float sway = 4f;         // degrees side to side

    [Header("Tongue")]
    public float tongueTilt = 25f;
    public float tongueWobble = 12f;

    float bodyY, phase;
    Vector3 bodyScale;

    void Awake()
    {
        bodyY = body.localPosition.y;
        bodyScale = body.localScale;
    }

    void Update()
    {
        if (idle) Idle();
        else Run();
    }

    void Run()
    {
        phase += Time.deltaTime * runSpeed;
        float s = Mathf.Sin(phase);

        legL.localRotation = Quaternion.Euler(s * legSwing, 0f, 0f);
        legR.localRotation = Quaternion.Euler(-s * legSwing, 0f, 0f);

        Vector3 p = body.localPosition;
        p.y = bodyY + Mathf.Abs(s) * bobHeight;
        body.localPosition = p;

        tongue.localRotation = Quaternion.Euler(tongueTilt + Mathf.Sin(phase * 2f) * tongueWobble, 0f, 0f);
    }

    void Idle()
    {
        phase += Time.deltaTime * idleSpeed;
        float s = Mathf.Sin(phase);

        legL.localRotation = Quaternion.identity;
        legR.localRotation = Quaternion.identity;

        // breathe: squash down and wider, then stretch up and thinner
        body.localScale = new Vector3(
            bodyScale.x * (1f + s * breathe),
            bodyScale.y * (1f - s * breathe),
            bodyScale.z * (1f + s * breathe));

        // slow sway side to side
        body.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase * 0.5f) * sway);

        // lazy tongue wobble
        tongue.localRotation = Quaternion.Euler(tongueTilt + Mathf.Sin(phase) * tongueWobble * 0.5f, 0f, 0f);
    }
}