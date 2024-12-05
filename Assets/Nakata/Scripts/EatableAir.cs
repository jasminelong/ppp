using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] float shrinkSpeed = 0.5f;
    [SerializeField] float moveSpeed = 0.02f;

    
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
    void SetLossyScale(Transform self, Vector3 scale)
    {
        self.localScale = new Vector3(
            scale.x / self.lossyScale.x * scale.x, 
            scale.y / self.lossyScale.y * scale.y, 
            scale.z / self.lossyScale.z  * scale.z
        );
    }
    public IEnumerator Pop(System.Action onComplete,Transform targetTransform)
    {
        Vector3 initialScale = transform.localScale; // ���݂̃X�P�[��
        Vector3 targetScale = new Vector3(initialScale.x, 0f, initialScale.z); // �k�ޖڕW�X�P�[��
        Vector3 startPosition = transform.position; // ���݂̈ʒu
        Vector3 targetPos = targetTransform.position; // �ړ���̈ʒu
        transform.SetParent(null);
        float elapsedTime = 0f;
        while (transform.localScale.y > 0.01f || Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            Vector3 worldScale = transform.lossyScale; // ���[���h�X�P�[�����擾
            float newWorldY = Mathf.Lerp(worldScale.y, 0f, elapsedTime * shrinkSpeed);
            worldScale.y = newWorldY;

            SetLossyScale(transform, worldScale);


            // �ړ�
            transform.position = Vector3.Lerp(transform.position, targetPos, elapsedTime * moveSpeed);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���S�ɏk��ňړ�����������
        transform.localScale = targetScale;
        transform.position = targetPos;

        if(onComplete != null) onComplete();
    }
}
