using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform playerHead; // 这里指向 OVRCameraRig 的 CenterEyeAnchor
    public float distance = 1.5f; // Avatar 距离玩家的距离

    void Start()
    {
        if (playerHead != null)
        {
            // 计算 Avatar 应在的位置
            Vector3 targetPosition = playerHead.position + playerHead.forward * distance;

            // 设置 Avatar 的位置
            transform.position = targetPosition;

            // 让 Avatar 面向玩家
            transform.LookAt(playerHead);

            // 如果 Avatar 的正方向是 Z 轴正方向（通常是前方向），为了让它正面朝向玩家，旋转 180 度
            transform.Rotate(0, 180, 0);
        }
    }
}
