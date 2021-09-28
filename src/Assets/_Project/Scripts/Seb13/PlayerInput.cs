using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof (Player))]
public class PlayerInput : MonoBehaviour {
    public float interactionRadius = 2.0f;

    DialogueRunner dialogue;
    Player player;
    PlayerShoot playerShoot;
    Controller2D controller2D;

    // Input
    Vector2 movementInput;
    bool jumpInput;
    bool previousJumpInput;

    bool bufferJump;
    float bufferTimer = 0f;
    [SerializeField]
    float bufferMaxTimer = 0.2f;

    bool bufferRelease = false;
    float bufferReleaseTimer = 0f;
    bool tempDisableBuffer;

    bool coyoteJump;
    float coyoteTimer = 0f;
    [SerializeField]
    float coyoteMaxTimer = 0.2f;
    bool tempDisableCoyote;

    bool shootInput;
    bool previousShootInput;

    PanicMode panicMode;
    bool endGame;

    public void OnMovement(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        //Debug.Log(movementInput);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        previousShootInput = shootInput;
        shootInput = context.ReadValueAsButton();

        var onButtonDown = shootInput && !previousShootInput;
        var onButtonUp = !shootInput && previousShootInput;

        //Debug.Log($"previousShootInput: {previousShootInput} | shootInput: {shootInput}");

        if (!CanMove()) return;

        playerShoot.Shoot(onButtonDown, onButtonUp);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        previousJumpInput = jumpInput;
        jumpInput = context.ReadValueAsButton();

        var onButtonDown = jumpInput && !previousJumpInput;
        var onButtonUp = !jumpInput && previousJumpInput;

        //Debug.Log(previousJumpInput + " " + jumpInput);

        // Checks dialogue if it's running, and the jump input was just done
        if (dialogue.IsDialogueRunning && onButtonDown)
        {
            var dialogueUI = FindObjectOfType<DialogueUI>();
            if (dialogueUI)
            {
                Debug.Log("Requested next line");
                dialogueUI.userRequestedNextLine = true;
            }
            else
            {
                Debug.LogWarning("DialogueUI not found");
            }
        }

        if (!CanMove()) return;

        // Checks for onButtonDown or if a jump was buffered
        if (onButtonDown || bufferJump)
        {
            // Can we do a jump? (on the ground, or coyoto timed)
            if (coyoteJump)
            {
                if (bufferJump)
                {
                    Debug.Log("Buffer jump");
                }

                // If there's no NPC, try to jump
                // Else, we run the NPC dialogue code
                if (!CheckForNearbyNPC())
                {
                    DoJump();
                }

            } else
            {
                // We're in the air
                //if (!controller2D.collisions.below && !bufferJump)
                //{
                //    Debug.Log("Start Buffer");

                //    // Set flag for buffer jump, and setup timer
                //    bufferJump = true;
                //    bufferTimer = 0;

                //    bufferRelease = true;
                //}
            }
        }

        // OnButtonUp
        if (onButtonUp)
        {
            // If there's a buffer release, trigger the OnJumpInputUp()
            // after the buffer period of time
            if (bufferRelease)
            {
                //StartCoroutine(BufferRelease());
            } else
            {
                //Debug.Log("OnJumpInputUp from OnJump");

                // Handle the limit y velocity
                player.OnJumpInputUp();

                // Resets input
                previousJumpInput = false;
            }

            // Resets the buffer release
            bufferRelease = false;
        }
    }

    IEnumerator BufferRelease()
    {
        Debug.Log($"Waiting for {bufferReleaseTimer}s");
        yield return new WaitForSeconds(bufferReleaseTimer);
        Debug.Log($"velocity: {player.Velocity}");
        player.OnJumpInputUp();
        Debug.Log("OnJumpInputUp from BufferRelease");
        Debug.Log($"velocity 2: {player.Velocity}"); 
        bufferReleaseTimer = 0f;
    }

    void DoJump()
    {
        //Debug.Log("DoJump");
        player.OnJumpInputDown(coyoteJump);

        // Make sure coyote jumps don't work
        coyoteJump = false;
        coyoteTimer = coyoteMaxTimer;
        tempDisableCoyote = true;

        // Disable buffer
        DisableBuffer();
    }

    void DisableBuffer()
    {
        // Disable buffer
        bufferJump = false;
        bufferTimer = bufferMaxTimer;
        tempDisableBuffer = true;
    }

    /// Draw the range at which we'll start talking to people.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        // Flatten the sphere into a disk, which looks nicer in 2D games
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 1, 0));

        // Need to draw at position zero because we set position in the line above
        Gizmos.DrawWireSphere(Vector3.zero, interactionRadius);
    }

    void Start () {
        dialogue = FindObjectOfType<DialogueRunner>();
        Debug.Assert(dialogue, "DialogueRunner wasn't found on player");

        player = GetComponent<Player>();

        UnityEngine.InputSystem.PlayerInput input 
            = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (input.notificationBehavior != PlayerNotifications.InvokeUnityEvents) {
            Debug.LogWarning("Setting input notification behavior to InvokeUnityEvents");
            input.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        controller2D = player.GetComponent<Controller2D>();
        Debug.Assert(controller2D, "Controller2D wasn't found on player");

        playerShoot = player.GetComponent<PlayerShoot>();
        Debug.Assert(playerShoot, "PlayerShoot wasn't found on player");

        panicMode = FindObjectOfType<PanicMode>();
    }

    // Update is called once per frame
    void Update () {
        // Movement
        if (CanMove())
        {
            player.SetDirectionalInput(movementInput);
        } else
        {
            // Kill any previous movement
            player.SetDirectionalInput(Vector2.zero);
        }

        // Coyote jump logic
        if (controller2D.collisions.below)
        {
            // If the coyote has been disabled, but the player is falling,
            // re-enable it. (This is so it doesn't get immediately reset
            // when the bottom rays touch at the bottom)
            if (tempDisableCoyote && player.Velocity.y <= 0f)
            {
                tempDisableCoyote = false;
            }

            // Resets the coyote jump, if not disabled
            if (!tempDisableCoyote)
            { 
                coyoteJump = true;
                coyoteTimer = 0;
            }
        }
        else
        {
            // Handles coyote jumps
            coyoteTimer += Time.deltaTime;
            // Clamp
            if (coyoteTimer > coyoteMaxTimer)
            {
                coyoteTimer = coyoteMaxTimer;
            }
            coyoteJump = coyoteTimer < coyoteMaxTimer;
        }

        // Buffer jump logic
        if (bufferJump) 
        { 
            // If there's ground, use the buffer jump.
            // DoJump sets bufferJump to false
            if (controller2D.collisions.below)
            {
                DoJump();
            } else
            {
                // Buffer jump timers
                bufferTimer += Time.deltaTime;
                // Clamp
                if (bufferTimer > bufferMaxTimer)
                {
                    bufferTimer = bufferMaxTimer;
                }
                bufferJump = bufferTimer < bufferMaxTimer;
            }
        }

        // Buffer jump release logic
        // Starts a timer to get how long in-between the jump button was held
        if (bufferRelease)
        {
            // If the coyote has been disabled, but the player is falling,
            // re-enable it. (This is so it doesn't get immediately reset
            // when the bottom rays touch at the bottom)
            if (tempDisableBuffer && player.Velocity.y <= 0f)
            {
                tempDisableBuffer = false;
            }

            if (!tempDisableBuffer)
            {
                if (!controller2D.collisions.below)
                {
                    bufferReleaseTimer += Time.deltaTime;
                } else
                {
                    bufferRelease = false;
                    bufferReleaseTimer = 0;

                    Debug.Log("Disable Buffer Release");
                }
            }
        }
    }

    /// Find all DialogueParticipants
    /** Filter them to those that have a Yarn start node and are in range; 
     * then start a conversation with the first one
     */
    public bool CheckForNearbyNPC()
    {
        var allParticipants = new List<NPC>(FindObjectsOfType<NPC>());
        var target = allParticipants.Find(delegate (NPC p) {
            return string.IsNullOrEmpty(p.talkToNode) == false && // has a conversation node?
            (p.transform.position - transform.position) // is in range?
            .magnitude <= interactionRadius;
        });
        if (target != null)
        {
            // Kick off the dialogue at this node.
            FindObjectOfType<DialogueRunner>().StartDialogue(target.talkToNode);
            
            return true;
        }

        var rangeAllParticipants = new List<NPCWithCustomRange>(
            FindObjectsOfType<NPCWithCustomRange>()
        );
        var target2 = rangeAllParticipants.Find(delegate (NPCWithCustomRange p) {
            return string.IsNullOrEmpty(p.talkToNode) == false && // has a conversation node?
            p.IsInRange(controller2D.collider);
        });
        if (target2 != null)
        {
            if (!target2.endGame)
            {
                // Kick off the dialogue at this node.
                FindObjectOfType<DialogueRunner>().StartDialogue(target2.talkToNode);
            } else
            {
                endGame = true;
                FindObjectOfType<FadeManager>().FadeOut(3);
                //FindObjectOfType<MusicManager>().StopAllCoroutines();
                //EazySoundManager.StopAllMusic();
                UIManager.instance.EndGame();
            }

            return true;
        }

        return false;
    }
    public bool CanMove()
    {
        return !dialogue.IsDialogueRunning && !player.IsDead && !endGame;
    }

    public void ResetInputs()
    {
        // Jump input is sent even though NPC was called, so we reset it
        // so you don't need to press twice to reset the jump
        jumpInput = false;

        previousJumpInput = false;
    }
}
