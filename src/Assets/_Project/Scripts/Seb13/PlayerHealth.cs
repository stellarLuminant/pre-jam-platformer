using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] LayerMask hurtLayerMask;
    [SerializeField] int framesWaitOnHit;
    public int DefaultMaxHealth = 3;
    [SerializeField] SpriteRenderer characterSpriteRenderer;
    [SerializeField] float invincibleFramesTimerLength;
    [SerializeField] Color invincibleFramesHitColor = Color.white;
    bool doInvincibleTimer;
    float invincibleFramesTimer;
    Color characterOriginalColor;

    [Header("Sound")]
    [SerializeField] AudioClip hurtClip;
    [SerializeField] float hurtClipVolume;

    [Header("Debugging")]
    [SerializeField] bool triggerAddHeart;
    [SerializeField] bool triggerRemoveHeart;
    [SerializeField] bool triggerHeal;
    [SerializeField] bool triggerHurt;
    [SerializeField] bool godMode;
    [SerializeField] int howMuch = 1;

    HeartContainer heartContainer;

    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }
        set
        {
            // Sets previous max health 
            previousMaxHealth = maxHealth;

            // Set health
            maxHealth = value;
            if (maxHealth < 1)
            {
                maxHealth = 1;
            }

            // Changes max health in HeartContainer
            if (heartContainer)
            {
                int changeInMaxHealth = maxHealth - previousMaxHealth;

                if (changeInMaxHealth > 0)
                {
                    StartCoroutine(heartContainer.AddHeart(changeInMaxHealth));
                }
                else if (changeInMaxHealth < 0)
                {
                    GetComponent<Player>().
                    StartCoroutine(heartContainer.RemoveHeart(changeInMaxHealth));
                }
            }
            else
            {
                Debug.LogWarning("[Health] No heartcontainer found");
            }
        }
    }
    
    int maxHealth;
    int previousMaxHealth;
    public int CurrentHealth
    {
        get
        {
            return health;
        }
        set
        {
            // Sets previous health
            previousHealth = health;

            // Sets health
            if (!(godMode || doInvincibleTimer))
            {
                //Debug.Log($"_maxHealth: {_maxHealth}");
                health = Mathf.Clamp(value, 0, maxHealth);
            }

            int changeInHealth = health - previousHealth;

            // Sets the value to heart container, if this is the player's
            if (heartContainer)
            {
                heartContainer.Health = health;

                if (changeInHealth < 0)
                {
                    int id = EazySoundManager.PlaySound(hurtClip, hurtClipVolume, false, transform);
                    //Audio audio = EazySoundManager.GetSoundAudio(id);
                    //audio.Set3DDistances(10, 20);

                    StartCoroutine(FreezeTimeOnHit());
                }
            }

            //Debug.Log($"CurrentHealth: {CurrentHealth}");

            // If health reaches zero, it dies
            if (health == 0 && !deadTrigger)
            {
                deadTrigger = true;
                Debug.Log("Dead");
                GetComponent<Player>().Die();
            }
        }
    }
    private int health;
    private int previousHealth;
    private bool deadTrigger;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;

        // Max Health
        maxHealth = DefaultMaxHealth;
        health = maxHealth;

        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("[Health] Detected player health");
            heartContainer = UIManager.instance.heartContainer;

            if (heartContainer)
            {
                Debug.Log("[Health] Detected heart container");
                heartContainer.Init(DefaultMaxHealth);
            }
        }

        if (characterSpriteRenderer)
            characterOriginalColor = characterSpriteRenderer.color;
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (triggerAddHeart)
        {
            Debug.Log($"Adding heart TRIGGER");
            MaxHealth += howMuch;
            triggerAddHeart = false;
        }

        if (triggerRemoveHeart)
        {
            Debug.Log($"Remove heart TRIGGER");
            MaxHealth -= howMuch;
            triggerRemoveHeart = false;
        }

        if (triggerHeal)
        {
            Debug.Log($"Heal heart TRIGGER");
            CurrentHealth += howMuch;
            triggerHeal = false;
        }

        if (triggerHurt)
        {
            Debug.Log($"Hurt heart TRIGGER");
            CurrentHealth -= howMuch;
            triggerHurt = false;
        }
        #endif
    }

    void FixedUpdate()
    {
        if (doInvincibleTimer)
        {
            invincibleFramesTimer += Time.fixedDeltaTime;

            if (characterSpriteRenderer)
            {
                characterSpriteRenderer.color = invincibleFramesHitColor;
            }

            if (invincibleFramesTimer > invincibleFramesTimerLength)
            {
                // Resets all
                invincibleFramesTimer = 0;
                doInvincibleTimer = false;

                if (characterSpriteRenderer)
                {
                    characterSpriteRenderer.color = characterOriginalColor;
                }
            }
        }
    }

    public IEnumerator FreezeTimeOnHit()
    {
        Time.timeScale = 0;
        if (invincibleFramesTimerLength > 0)
        {
            doInvincibleTimer = true;
        }

        var player = GetComponent<Player>();
        player.StartHurtAnimation();
        for (int i = 0; i < framesWaitOnHit; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        player.StopHurtAnimation();
        Time.timeScale = 1;
    }
}
