using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public LayerMask enemyLayerMask;
    public Chicken chickenSucked;
    bool chickenFullySucked;
    public float shootSpeed = 200;
    public float suckSpeed = 2;
    public float suckRange = 5f;
    float suckStep;
    bool suck = false;

    public Animator anim;
    public Transform shootPoint;

    public float timeBetweenShooting = 0.25f;
    float timer;

    ShootPointManager shootPointManager;

    [Header("Audio")]
    public AudioClip shootClip;
    public float shootVolume = 1;
    public AudioClip shootEmptyClip;
    public float shootEmptyVolume = 1;
    public float randomRangePitch = 0.1f;

    private void OnDrawGizmosSelected()
    {
        if (shootPoint)
        {
            Gizmos.color = Color.blue;

            // Flatten the sphere into a disk, which looks nicer in 2D games
            Gizmos.matrix = Matrix4x4.TRS(shootPoint.transform.position, Quaternion.identity, new Vector3(1, 1, 0));

            // Need to draw at position zero because we set position in the line above
            Gizmos.DrawWireSphere(Vector3.zero, suckRange);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(shootPoint, "shootPoint Transform not found!");

        shootPointManager = GetComponentInChildren<ShootPointManager>();
        Debug.Assert(shootPointManager);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        // Clamp
        if (timer > timeBetweenShooting)
        {
            timer = timeBetweenShooting;
        }

        // If a chicken was not sucked, and suck is there
        if (!chickenSucked && suck)
        {
            var allOverlaps = Physics2D.OverlapCircleAll(shootPoint.position, suckRange, enemyLayerMask);

            Collider2D candidateCollider = null;

            // Gets the closest collider
            foreach (var item in allOverlaps)
            {
                // Make sure it can't be null
                if (!candidateCollider) candidateCollider = item;

                if (Vector3.Distance(item.transform.position, transform.position) > 
                    Vector3.Distance(candidateCollider.transform.position, transform.position)
                )
                {
                    candidateCollider = item;
                }
                
                break;
            }

            if (candidateCollider)
            {
                chickenSucked = candidateCollider.GetComponent<Chicken>();

                if (chickenSucked)
                {
                    chickenSucked.State = Chicken.ChickenState.WaitingForLaunch;
                    chickenSucked.SuckTowards(shootPoint, true);
                } else
                {
                    Debug.LogError("aaaaaaaaa");
                }
            }
        }
    }

    public void Shoot(bool isButtonDown, bool isButtonUp)
    {
        // If no chicken has been sucked, and there's a button input to suck,
        // Set state to suck
        if (!chickenSucked && isButtonDown)
        {
            suck = true;
        } 
        // There is a chicken
        else
        {
            // Was it to let go?
            if (isButtonUp)
            {
                suck = false;

                // Launch the chicken
                if (chickenSucked) 
                {
                    chickenSucked.ResetSuck();

                    Debug.Log("Shooting held chicken");

                    var tempShootSpeed = shootSpeed;
                    if (shootPointManager.ShouldRetreat(shootPointManager.collider))
                    {
                        Debug.Log("Moved to retreated, decreased shoot");
                        shootPointManager.transform.localPosition = shootPointManager.retreatedPosition;
                        tempShootSpeed = tempShootSpeed - (tempShootSpeed / 2);
                    }

                    var chickenRbody = chickenSucked.GetComponent<Rigidbody2D>();
                    chickenSucked.State = Chicken.ChickenState.Launch;

                    chickenRbody.AddForce(new Vector2(Mathf.Sign(transform.localScale.x) * tempShootSpeed, 0), ForceMode2D.Impulse);

                    // Reset variables
                    chickenFullySucked = false;
                    suckStep = 0;
                    shootPointManager.transform.localPosition = shootPointManager.defaultPosition;

                    // Dereference the chicken
                    chickenSucked = null;
                }
            }
        }
    }
}
