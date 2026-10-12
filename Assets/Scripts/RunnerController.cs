using UnityEngine;

public class RunnerController : MonoBehaviour
{
    [Header("Lanes")]
    public float laneWidth = 2f;
    public float laneChangeSpeed = 10f;
    private int currentLane = 1; // 0 = left, 1 = middle, 2 = right

    [Header("Jump")]
    public float jumpForce = 7f;
    public float gravity = -20f;
    private float verticalVelocity;
    private bool isGrounded = true;
    private float groundY;

    [Header("Slide")]
    public float slideDuration = 0.6f;
    private bool isSliding;
    private float slideTimer;

    void Awake()
    {
        groundY = transform.position.y;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        // Smoothly move toward the target lane
        float targetX = (currentLane - 1) * laneWidth;
        pos.x = Mathf.Lerp(pos.x, targetX, laneChangeSpeed * Time.deltaTime);





        // Gravity and landing (lower floor while sliding)
        float floor = isSliding ? groundY - 0.5f : groundY;
        if (!isGrounded) verticalVelocity += gravity * Time.deltaTime;
        pos.y += verticalVelocity * Time.deltaTime;
        if (pos.y <= floor)
        {
            pos.y = floor;
            verticalVelocity = 0f;
            isGrounded = true;
        }



        // Slide timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f) EndSlide();
        }

        transform.position = pos;
    }





    public void MoveLeft()  { if (currentLane > 0) currentLane--; }
    public void MoveRight() { if (currentLane < 2) currentLane++; }

    public void Jump()
    {
        if (!isGrounded) return;
        if (isSliding) EndSlide();
        isGrounded = false;
        verticalVelocity = jumpForce;
    }

    public void Slide()
{
    if (!isGrounded || isSliding) return;
    isSliding = true;
    slideTimer = slideDuration;
    transform.localScale = new Vector3(1f, 0.5f, 1f);
}

void EndSlide()
{
    isSliding = false;
    transform.localScale = Vector3.one;
}
}