using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AirState
{
    Idle,
    Stabbed,
    Moving,
    Popped,
    Eated
}

public class EatableAir : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip stabbedSE;
    [SerializeField] AudioClip[] poppingSEs;
    AirState airState;

    public void ChangeState(AirState newAirState)
    {
        switch (newAirState)
        {
            case AirState.Stabbed:
                if (stabbedSE != null)
                {
                    PlaySound(stabbedSE);
                }
                airState = AirState.Moving; // Moving‚É‘JˆÚ
                break;

            case AirState.Popped:
                if (poppingSEs != null && poppingSEs.Length > 0)
                {
                    AudioClip poppingSE = poppingSEs[Random.Range(0, poppingSEs.Length)];
                    PlaySound(poppingSE);
                }
                airState = AirState.Popped; // Popped‚É‘JˆÚ
                break;

            default:
                airState = newAirState;
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
