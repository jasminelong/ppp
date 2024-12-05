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
    // ���V�̐ݒ�
    [SerializeField] float floatSpeed = 1f;    // ���V���x
    [SerializeField] float floatRange = 0.5f;  // ���V�͈�
    void Update()
    {
        // Idle��Ԃŕ��V����
        if (airState == AirState.Idle)
        {
            Float();
        }
    }


    public void ChangeState(AirState newAirState)
    {
        switch (newAirState)
        {
            case AirState.Stabbed:
                if (stabbedSE != null)
                {
                    PlaySound(stabbedSE);
                }
                airState = AirState.Moving; // Moving�ɑJ��
                break;

            case AirState.Popped:
                if (poppingSEs != null && poppingSEs.Length > 0)
                {
                    AudioClip poppingSE = poppingSEs[Random.Range(0, poppingSEs.Length)];
                    PlaySound(poppingSE);
                }
                airState = AirState.Popped; // Popped�ɑJ��
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
    private void Float()
    {
        // ���݂�Y�ʒu����ɂ��ď㉺�̕ϓ����v�Z
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = new Vector3(transform.position.x, transform.position.y + newY, transform.position.z);
    }
}
