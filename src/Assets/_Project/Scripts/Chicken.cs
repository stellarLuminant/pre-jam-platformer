using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Controller2D))]
public class Chicken : MonoBehaviour
{
    public enum ChickenState
    {
        Wander, SeePlayer, WaitingForLaunch, Launch, HitLaunch
    }
    public ChickenState State {
        get
        {
            return _state;
        }
        set
        {
            cluckTimer = 0;
            cluckTimerRandomRange = 0;
            hitFirst = false;

            switch (value)
            {
                case ChickenState.Wander:
                    if (standStill)
                    {
                        anim.Play(idleAnimName);
                    }
                    moveDirection = Random.Range(0, 1) == 0 ? Vector2.right : Vector2.left;
                    useControllerSystem = true;
                    UseCircleCollider(false);
                    rbody.freezeRotation = true;
                    break;
                case ChickenState.SeePlayer:
                    useControllerSystem = true;
                    UseCircleCollider(false);
                    rbody.freezeRotation = true;
                    break;
                case ChickenState.WaitingForLaunch:
                    useControllerSystem = false;
                    UseCircleCollider(true);
                    rbody.isKinematic = true;
                    anim.Play(launchedAnimName);
                    rbody.freezeRotation = false;
                    break;
                case ChickenState.Launch:
                    useControllerSystem = false;
                    UseCircleCollider(true);
                    rbody.isKinematic = false;
                    anim.Play(launchedAnimName);
                    rbody.freezeRotation = false;
                    break;
            }
            _state = value;
        }
    }
    ChickenState _state;
    bool useControllerSystem;

    Animator anim;
    BoxCollider2D boxCollider;
    CircleCollider2D circleCollider;
    Controller2D controller;
    Rigidbody2D rbody;

    public bool isGolden;

    [Header("Movement")]
    public bool standStill;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector2 moveDirection;
    Vector3 velocity = Vector2.zero;
    public float moveSpeed = 5;
    public float timeToJumpApex = .4f;

    float velocityXSmoothing = 0;
    public float accelerationTimeAirborne = .1f;
    public float accelerationTimeGrounded = .05f;

    [Header("Settings")]
    public Vector2 velocityOnHitLaunch = new Vector2(300, 500);
    public float interactionRange = 6f;

    [Header("Animation")]
    public string idleAnimName = "Idle";
    public string runAnimName = "Run";
    public string jumpAnimName = "Jump";
    public string launchedAnimName = "Launched";
    public string idlePanicAnimName = "IdlePanic";
    public string runPanicAnimName = "RunPanic";

    [Header("Audio")]
    public AudioClip[] cluckClips;
    public AudioClip[] screamClips;
    public AudioClip[] hitClips;
    public float clipVolumeBase = 1;
    public float minDistance = 0;
    public float maxDistance = 60;
    float cluckTimer;
    float cluckTimerRandomRange;

    float cluckTimerLowRange = 3f;
    float cluckTimerHighRange = 7f;

    float seePlayerLowRange = 2f;
    float seePlayerHighRange = 5f;

    float screamTimerLowRange = 1f;
    float screamTimerHighRange = 2f;

    int audioSourceId = -1;
    bool hitFirst;

    PanicMode panicMode;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        controller = GetComponent<Controller2D>();
        panicMode = FindObjectOfType<PanicMode>();
        rbody = GetComponent<Rigidbody2D>();

        Debug.Assert(anim);
        Debug.Assert(boxCollider);
        Debug.Assert(circleCollider);
        Debug.Assert(controller);
        Debug.Assert(panicMode);
        Debug.Assert(rbody);

        cluckTimerRandomRange = Random.Range(cluckTimerLowRange, cluckTimerHighRange);

        RecalculateJumpAndGravity();

        State = ChickenState.Wander;
    }

    void UseCircleCollider(bool trueOrFalse)
    {
        boxCollider.enabled = !trueOrFalse;
        circleCollider.enabled = trueOrFalse;
    }

    void RecalculateJumpAndGravity()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void CalculateVelocity()
    {
        var tempMoveSpeed = moveSpeed;
        if (panicMode.Panicking)
        {
            tempMoveSpeed = moveSpeed * 2;
        }

        float targetVelocityX = moveDirection.x * tempMoveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x, targetVelocityX,
            ref velocityXSmoothing,
            controller.collisions.below
                ? accelerationTimeGrounded
                : accelerationTimeAirborne
        );
        velocity.y += gravity * Time.deltaTime;
    }

    float nextActionTimer;
    float nextActionTimerRandomRange;
    float nextActionTimerMaxRange = 3;
    float nextActionTimerMinRange = 1;

    enum WanderState
    {
        Idle, Run
    }
    WanderState wanderState;

    // Update is called once per frame
    void Update()
    {
        if (!standStill) {
            nextActionTimer += Time.deltaTime;

            if (!panicMode.Panicking)
            {
                if (nextActionTimer > nextActionTimerRandomRange)
                {
                    nextActionTimer = 0;
                    nextActionTimerRandomRange = Random.Range(nextActionTimerMinRange, nextActionTimerMaxRange);

                    var action = Random.Range(0, 2);
                    wanderState = (WanderState)action;
                }
            } else
            {
                wanderState = WanderState.Run;
            }

            var localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            switch (State)
            {
                case ChickenState.Wander:
                case ChickenState.SeePlayer:
                    if (!(controller.collisions.left && controller.collisions.right))
                    {
                        if (controller.collisions.left)
                        {
                            localScale.x = 1;
                            moveDirection = Vector2.right;
                        } else if (controller.collisions.right)
                        {
                            localScale.x = -1;
                            moveDirection = Vector2.left;
                        }
                    } else
                    {
                        // we're in-between something, give up
                    }
                    break;
            }
            transform.localScale = localScale;

            switch (State)
            {
                case ChickenState.Wander:
                    CalculateVelocity();

                    switch (wanderState)
                    {
                        case WanderState.Idle:
                            if (!panicMode.Panicking)
                                anim.Play(idleAnimName);
                            else
                                anim.Play(idlePanicAnimName);
                            break;
                        case WanderState.Run:
                            if (!panicMode.Panicking)
                                anim.Play(runAnimName);
                            else
                                anim.Play(runPanicAnimName);
                            controller.Move(velocity * Time.deltaTime, moveDirection);
                            break;
                        default:
                            break;
                    }
                    break;
                case ChickenState.SeePlayer:
                    break;
                case ChickenState.WaitingForLaunch:
                    break;
                case ChickenState.Launch:
                    break;
                case ChickenState.HitLaunch:
                    break;
                default:
                    break;
            }
        }

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

        // Suck logic
        if (suckPoint) 
        { 
            if (!FullySucked)
            {
                // Lerps its way to the point of the gun
                suckStep += (Time.deltaTime * suckSpeed);
                transform.position = Vector2.MoveTowards(transform.position, suckPoint.position, suckSpeed);
                transform.eulerAngles = Vector2.MoveTowards(transform.eulerAngles, suckPoint.eulerAngles, suckSpeed);

                if (suckStep >= suckSpeed
                    || (transform.position - suckPoint.transform.position).magnitude < 0.1f)
                {
                    FullySucked = true;
                }
            }
            else
            {
                // Sets chicken to point
                transform.position = suckPoint.position;

                if (lockRotationAndResetScale)
                {
                    // Locks rotation
                    transform.eulerAngles = suckPoint.transform.eulerAngles;

                    // Flips chicken
                    transform.localScale = transform.localScale;
                }
            }
        }

        // Audio
        cluckTimer += Time.deltaTime;

        if (cluckTimer > cluckTimerRandomRange)
        {
            cluckTimer = 0;

            AudioClip clip = null;

            if (!panicMode.Panicking) { 
                switch (State)
                {
                    case ChickenState.Wander:
                        clip = cluckClips[Random.Range(0, cluckClips.Length)];
                        cluckTimerRandomRange = Random.Range(cluckTimerLowRange, cluckTimerHighRange) + clip.length;
                        break;
                    case ChickenState.SeePlayer:
                        clip = cluckClips[Random.Range(0, cluckClips.Length)];
                        cluckTimerRandomRange = Random.Range(seePlayerLowRange, seePlayerHighRange) + clip.length;
                        break;
                    case ChickenState.WaitingForLaunch:
                    //case ChickenState.Launch:
                        clip = screamClips[Random.Range(0, screamClips.Length)];
                        cluckTimerRandomRange = Random.Range(screamTimerLowRange, screamTimerHighRange) + clip.length;
                        break;

                    default:
                        break;
                }
            } else
            {
                clip = screamClips[Random.Range(0, screamClips.Length)];
                cluckTimerRandomRange = Random.Range(screamTimerLowRange, screamTimerHighRange) + clip.length;
            }

            if (clip)
            {
                var audio = EazySoundManager.GetAudio(audioSourceId);
                if (audio != null)
                {
                    audio.Clip = clip;
                    audio.Play();
                } else
                {
                    audioSourceId = EazySoundManager.PlaySound(clip, clipVolumeBase, false, transform);
                    audio = EazySoundManager.GetAudio(audioSourceId);
                }

                if (audio != null)
                {
                    audio.Pitch = Random.Range(1 - 0.07f, 1 + 0.07f);
                    audio.Set3DDistances(minDistance, maxDistance);
                    audio.RolloffMode = AudioRolloffMode.Linear;
                    audio.SpatialBlend = 1;
                    audio.DopplerLevel = 0;
                }
            }
        }
    }

    public Transform suckPoint;
    float suckStep;
    float suckSpeed = 0.5f;
    public bool FullySucked { get; private set; }
    bool lockRotationAndResetScale;

    public void ResetSuck()
    {
        suckPoint = null;
        suckStep = 0;
        FullySucked = false;
    }

    public void SuckTowards(Transform point, bool _lockRotationAndResetScale = false)
    {
        rbody.velocity = Vector2.zero;

        suckPoint = point;
        suckStep = 0;
        FullySucked = false;
        lockRotationAndResetScale = _lockRotationAndResetScale;

        if (isGolden)
        {
            panicMode.Panicking = true;
        }
    }

    void PlayClip(AudioClip clip)
    {
        var audio = EazySoundManager.GetAudio(audioSourceId);
        if (audio != null)
        {
            audio.Play();
        }
        else
        {
            audioSourceId = EazySoundManager.PlaySound(clip, clipVolumeBase, false, transform);
            audio = EazySoundManager.GetAudio(audioSourceId);
        }

        if (audio != null)
        {
            audio.Pitch = Random.Range(1 - 0.07f, 1 + 0.07f);
            audio.Set3DDistances(minDistance, maxDistance);
            audio.RolloffMode = AudioRolloffMode.Linear;
            audio.SpatialBlend = 1;
            audio.DopplerLevel = 0;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (State == ChickenState.Launch)
        {
            if (!hitFirst)
            {
                EazySoundManager.PlaySound(hitClips[Random.Range(0, hitClips.Length)]);
                hitFirst = true;
            }

            if (collision.transform.CompareTag("Destructable")) { 
                State = ChickenState.HitLaunch;
                Die();
            }
        }
        else if (State != ChickenState.HitLaunch && collision.transform.CompareTag("Player"))
        {
            var playerHealth = collision.transform.GetComponent<PlayerHealth>();

            if (playerHealth)
            {
                playerHealth.CurrentHealth--;
            }
        }

        if (collision.transform.CompareTag("Enemy"))
        {
            var chickenCollided = collision.transform.GetComponent<Chicken>();
            var newChickenRbody = chickenCollided.GetComponent<Rigidbody2D>();

            // Hit another chicken 
            if (chickenCollided)
            {
                if (chickenCollided.State == ChickenState.Launch && 
                    Mathf.Abs(newChickenRbody.velocity.x) > 1f &&
                    chickenCollided.suckPoint == null)
                {
                    anim.Play(launchedAnimName);
                    Die();
                }
                else
                {
                    if (!standStill) FlipMoveDirection();
                }
            }
        }
    }

    void FlipMoveDirection()
    {
        moveDirection *= Vector2.left;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void Die()
    {
        ResetSuck();

        var clip = screamClips[Random.Range(0, screamClips.Length)];
        PlayClip(clip);

        // Golden chicken cannot die
        if (isGolden)
        {
            return;
        }

        State = ChickenState.HitLaunch;
        Debug.Log("Dying");

        // Launch right if velocity's going over right, left if going left
        var launchDirection = Mathf.Sign(rbody.velocity.x);
        Launch(launchDirection);
    }

    void Launch(float launchDirection)
    {
        // Disable colliders
        boxCollider.enabled = false;
        circleCollider.enabled = false;

        rbody.velocity = Vector2.zero;
        rbody.AddForce(velocityOnHitLaunch * new Vector2(launchDirection, 1));
        Destroy(gameObject, 3);
    }
}
