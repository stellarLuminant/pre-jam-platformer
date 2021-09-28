using UnityEngine;
using System.Collections;
using Cinemachine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;

    public bool wallJumping = true;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    bool wasGroundedLastFrame = true;

    public bool IsDead { get; private set; }

    public bool disableVariableJump = false;

    PlayerSounds playerSounds;

    [Header("Animation")]
    public string idleAnimName = "Idle";
    public string runAnimName = "Run";
    public string jumpAnimName = "Jump";
    public string fallAnimName = "Fall";
    public string hurtAnimName = "Hurt";
    Animator anim;
    bool hurtAnim;

    [Header("Debug")]
    public bool recalculateJumpAndGravity = false;

    void Start() {
        anim = GetComponent<Animator>();
        controller = GetComponent<Controller2D>();
        playerSounds = GetComponent<PlayerSounds>();
        RecalculateJumpAndGravity();
    }

    void RecalculateJumpAndGravity()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update() {
        if (recalculateJumpAndGravity)
        {
            RecalculateJumpAndGravity();
            recalculateJumpAndGravity = false;
        }

        CalculateVelocity ();
        if (wallJumping) HandleWallSliding ();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        // Land Sound
        if (!wasGroundedLastFrame && controller.collisions.below)
        {
            playerSounds.DoWalkSound();
        }
        wasGroundedLastFrame = controller.collisions.below;

        // Animation logic, because using the animation tools in Unity sucks

        // Flips the player character by scale
        // Is moving left
        if (directionalInput.x < 0)
        {
            controller.TurnTowards(Controller2D.ControllerDirection.Left);
        }
        // Is moving right
        else if (directionalInput.x > 0)
        {
            controller.TurnTowards(Controller2D.ControllerDirection.Right);
        }

        if (anim)
        {
            if (hurtAnim)
            {
                anim.Play(hurtAnimName);
            }
            // Is jumping or falling
            else if (!controller.collisions.below)
            {
                // Going up
                if (velocity.y > 0)
                {
                    anim.Play(jumpAnimName);
                }
                // Going down
                else
                {
                    anim.Play(fallAnimName);
                }
            }
            // Is moving
            else if (Mathf.Abs(directionalInput.x) > 0)
            {
                anim.Play(runAnimName);
            }
            else
            {
                // Isn't moving
                anim.Play(idleAnimName);
            }
        }
    }

    public void SetDirectionalInput (Vector2 input) {
        directionalInput = input;
    }

    public void OnJumpInputDown(bool below) {
        if (wallSliding) {
            if (wallDirX == directionalInput.x) {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0) {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (below) {
            if (controller.collisions.slidingDownMaxSlope) {
                if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            } else {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp() {
        if (disableVariableJump) return;
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }
        
    public void StartHurtAnimation()
    {
        hurtAnim = true;
        anim.Play(hurtAnimName);
    }

    public void StopHurtAnimation()
    {
        hurtAnim = false;
    }


    void HandleWallSliding() {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0) {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0) {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else {
                timeToWallUnstick = wallStickTime;
            }

        }

    }

    void CalculateVelocity() {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x, targetVelocityX, 
            ref velocityXSmoothing, 
            (controller.collisions.below) 
                ? accelerationTimeGrounded 
                : accelerationTimeAirborne
        );
        velocity.y += gravity * Time.deltaTime;
    }

    public void Die()
    {
        IsDead = true;
        var rbody = GetComponent<Rigidbody2D>();
        rbody.isKinematic = false;

        // Disable colliders
        GetComponent<Collider2D>().enabled = false;

        rbody.velocity = Vector2.zero;
        rbody.AddForce(new Vector2(300, 500));

        foreach (var item in FindObjectsOfType<CinemachineVirtualCamera>())
        {
            item.Follow = null;
        }

        StartCoroutine(DieSequence());
    }

    IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(3);
        UIManager.instance.StartGame();
    }
}
