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
    // 浮遊の設定
    [SerializeField] float floatSpeed = 1f;    // 浮遊速度
    [SerializeField] float floatRange = 0.5f;  // 浮遊範囲
    [SerializeField] float shrinkSpeed = 0.5f;
    [SerializeField] float moveSpeed = 0.02f;

    
    void Update()
    {
        // Idle状態で浮遊処理
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
                airState = AirState.Moving; // Movingに遷移
                break;

            case AirState.Popped:
                airState = AirState.Popped; // Poppedに遷移
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
        // 現在のY位置を基準にして上下の変動を計算
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
        Vector3 initialScale = transform.localScale; // 現在のスケール
        Vector3 targetScale = new Vector3(initialScale.x, 0f, initialScale.z); // 縮む目標スケール
        Vector3 startPosition = transform.position; // 現在の位置
        Vector3 targetPos = targetTransform.position; // 移動先の位置
        transform.SetParent(null);
        float elapsedTime = 0f;
        while (transform.localScale.y > 0.01f || Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            Vector3 worldScale = transform.lossyScale; // ワールドスケールを取得
            float newWorldY = Mathf.Lerp(worldScale.y, 0f, elapsedTime * shrinkSpeed);
            worldScale.y = newWorldY;

            SetLossyScale(transform, worldScale);


            // 移動
            transform.position = Vector3.Lerp(transform.position, targetPos, elapsedTime * moveSpeed);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に縮んで移動完了したら
        transform.localScale = targetScale;
        transform.position = targetPos;

        if(onComplete != null) onComplete();
    }
}
