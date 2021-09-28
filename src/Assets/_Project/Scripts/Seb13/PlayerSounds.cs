using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    Player player;

    public AudioClip jumpClip;
    public float jumpVolume = 1f;
    public float jumpPitchVariation = 0.1f;

    [System.Serializable]
    public class AudioStepClip
    {
        public AudioClip stepClip;
        public float volume = 1;
        public float pitchVariation = 0.1f;
    }
    public AudioStepClip[] audioStep;

    void Start()
    {
        player = GetComponent<Player>();    
    }

    private void Update()
    {
        
    }

    public void DoJumpSound()
    {
        DoWalkSound();
        int id = EazySoundManager.PlaySound(jumpClip, jumpVolume);
        Audio audio = EazySoundManager.GetSoundAudio(id);
        if (audio != null)
        {
            audio.Pitch = Random.Range(1 - jumpPitchVariation, 1 + jumpPitchVariation);
        }
    }

    public void DoWalkSound()
    {
        var index = Random.Range(0, audioStep.Length);
        var randomAudio = audioStep[index];

        int id = EazySoundManager.PlaySound(randomAudio.stepClip, randomAudio.volume);
        Audio audio = EazySoundManager.GetSoundAudio(id);
        if (audio != null)
        {
            audio.Pitch = Random.Range(1 - randomAudio.pitchVariation, 1 + randomAudio.pitchVariation);
        }
    }
}
