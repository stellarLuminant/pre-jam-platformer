using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Container : MonoBehaviour
{
    public GameObject chickenAnchor;
    Animator anim;
    Chicken chickenSucked;

    public string redAnimName = "Red";
    public string greenAnimName = "Green";

    bool hasSucked;

    public UnityEvent onRed;
    public UnityEvent onGreen;

    public AudioClip redClip;
    public float redClipVolume = 0.8f;
    public AudioClip greenClip;
    public float greenClipVolume = 0.8f;

    Audio eazyAudio;
    bool isGreen;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        Debug.Assert(anim);
        Debug.Assert(chickenAnchor);
    }

    // Update is called once per frame
    void Update()
    {
        if (chickenSucked)
        {
            if (chickenSucked.FullySucked && chickenSucked.suckPoint == chickenAnchor.transform)
            {
                if (!isGreen)
                {
                    anim.Play(greenAnimName);
                    hasSucked = true;
                    onGreen?.Invoke();

                    PlayAudio(greenClip, greenClipVolume);
                }

                // Sets to proper value, locking out the above
                isGreen = true;
            }
            else
            {
                if (isGreen)
                {
                    anim.Play(redAnimName);
                    PlayAudio(redClip, redClipVolume);

                    // Reset
                    if (hasSucked)
                    {
                        chickenSucked = null;
                        hasSucked = false;
                        onRed?.Invoke();
                    }
                }

                // Sets to proper value, locking out the above
                isGreen = false;
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var tempChickenSucked = collision.GetComponent<Chicken>();
            if (!tempChickenSucked) return;

            // If we already have a chicken, and it's been sucked to this container, skip
            if (chickenSucked && tempChickenSucked.suckPoint == chickenAnchor.transform)
            {
                //Debug.Log("already sucked, won't suck again");
                return;
            }

            chickenSucked = tempChickenSucked;

            if (chickenSucked.State == Chicken.ChickenState.Launch)
            {
                //Debug.Log("sucking towards");
                chickenSucked.SuckTowards(chickenAnchor.transform);
            }
        }
    }

    void PlayAudio(AudioClip clip, float volume)
    {
        if (eazyAudio == null)
        {
            int id = EazySoundManager.PlaySound(clip, volume, false, transform);
            eazyAudio = EazySoundManager.GetAudio(id);
            eazyAudio.DopplerLevel = 0;
        }
        else
        {
            eazyAudio.Clip = clip;
            eazyAudio.Stop();
            eazyAudio.Play();
        }
    }
}
