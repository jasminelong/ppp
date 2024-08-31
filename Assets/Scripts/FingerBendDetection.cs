using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerBendDetection : MonoBehaviour
{
    public OVRHand ovrHand;
    private OVRSkeleton ovrSkeleton;
    public float bendThresholdAngle = 15.0f; // 角度阈值，降低阈值以提高检测敏感性
    private Dictionary<OVRSkeleton.BoneId, Transform> boneTransforms = new Dictionary<OVRSkeleton.BoneId, Transform>();
    private bool skeletonInitialized = false;

    void Update()
    {
        // 骨骼数据未准备好时，定期检查是否已初始化
        if (!skeletonInitialized)
        {
            ovrSkeleton = ovrHand.GetComponent<OVRSkeleton>();

            if (ovrSkeleton != null && ovrSkeleton.Bones.Count > 0)
            {
                foreach (var bone in ovrSkeleton.Bones)
                {
                    boneTransforms[bone.Id] = bone.Transform;
                }

                skeletonInitialized = true;
                Debug.Log("Skeleton initialized successfully.");
            }
        }

        // 骨骼初始化后，进行手指弯曲检测
        if (skeletonInitialized)
        {
            bool isIndexFingerBent = IsFingerBent(OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3);
            bool isMiddleFingerBent = IsFingerBent(OVRSkeleton.BoneId.Hand_Middle1, OVRSkeleton.BoneId.Hand_Middle2, OVRSkeleton.BoneId.Hand_Middle3);
            bool isRingFingerBent = IsFingerBent(OVRSkeleton.BoneId.Hand_Ring1, OVRSkeleton.BoneId.Hand_Ring2, OVRSkeleton.BoneId.Hand_Ring3);
            bool isPinkyFingerBent = IsFingerBent(OVRSkeleton.BoneId.Hand_Pinky1, OVRSkeleton.BoneId.Hand_Pinky2, OVRSkeleton.BoneId.Hand_Pinky3);

            Debug.Log($"Index Finger Bent: {isIndexFingerBent}");
            Debug.Log($"Middle Finger Bent: {isMiddleFingerBent}");
            Debug.Log($"Ring Finger Bent: {isRingFingerBent}");
            Debug.Log($"Pinky Finger Bent: {isPinkyFingerBent}");
        }
    }

    bool IsFingerBent(OVRSkeleton.BoneId fingerBase, OVRSkeleton.BoneId fingerMid, OVRSkeleton.BoneId fingerTip)
    {
        Transform baseTransform = GetBoneTransform(fingerBase);
        Transform midTransform = GetBoneTransform(fingerMid);
        Transform tipTransform = GetBoneTransform(fingerTip);

        if (baseTransform == null || midTransform == null || tipTransform == null)
        {
            Debug.LogWarning("Bone transform not found");
            return false;
        }

        // 计算局部空间中每个关节的夹角
        float baseMidAngle = Quaternion.Angle(baseTransform.localRotation, midTransform.localRotation);
        float midTipAngle = Quaternion.Angle(midTransform.localRotation, tipTransform.localRotation);

        // 打印角度以便调试
        Debug.Log($"Base-Mid Angle: {baseMidAngle}, Mid-Tip Angle: {midTipAngle}");

        // 如果任意两个关节的夹角超过阈值，认为手指弯曲
        //return baseMidAngle > bendThresholdAngle || midTipAngle > bendThresholdAngle;
        return   midTipAngle > bendThresholdAngle;
    }

    Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
    {
        if (boneTransforms.ContainsKey(boneId))
        {
            return boneTransforms[boneId];
        }

        return null; // 找不到对应的骨骼
    }
}
