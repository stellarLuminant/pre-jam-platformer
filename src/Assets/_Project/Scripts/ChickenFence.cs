using System.Collections;
using UnityEngine;

public class ChickenFence : MonoBehaviour
{
    public int redLayerMask = 8;
    public int greenLayerMask = 0;

    SpriteRenderer spriteRenderer;

    public Color redColor = new Color(255, 66, 66, 255);
    public Color greenColor = new Color(99, 171, 63, 255);

    public bool onExitPlayDrums;
    public bool pushPlayerUp;
    public float pushPlayerUpVelocity = 25f;

    Coroutine coroutine;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(spriteRenderer);

        TurnRed();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnGreen()
    {
        spriteRenderer.color = greenColor;
        gameObject.layer = greenLayerMask;
    }

    public void TurnRed() 
    {
        spriteRenderer.color = redColor;
        gameObject.layer = redLayerMask;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pushPlayerUp)
        {
            if (collision.CompareTag("Player") && spriteRenderer.color == greenColor)
            {
                var player = collision.GetComponentInParent<Player>();

                if (player && player.Velocity.y > 0)
                {
                    Debug.Log("disableVariableJump");
                    if (coroutine != null) StopCoroutine(coroutine);
                    player.disableVariableJump = true;
                    player.Velocity = new Vector3(player.Velocity.x, pushPlayerUpVelocity, player.Velocity.z);
                }
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (onExitPlayDrums)
        {
            if (collision.CompareTag("Player") && spriteRenderer.color == greenColor)
            {
                var player = collision.GetComponentInParent<Player>();
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(EnableVariableJump(player));

                if (player && player.Velocity.y < 0)
                {
                    FindObjectOfType<MusicManager>().PlayDrumVariant = true;
                }
            }
        }
    }

    IEnumerator EnableVariableJump(Player player)
    {
        yield return new WaitForSeconds(0.5f);
        player.disableVariableJump = false;
        Debug.Log("EnableVariableJump");
    }
}
