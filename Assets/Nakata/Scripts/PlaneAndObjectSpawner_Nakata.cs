using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Meta.XR.MRUtilityKit;
using System;
using System.Linq;
using UnityEditor.Overlays;

public class PlaneAndObjectSpawner_Nakata : MonoBehaviour
{
    enum ScriptState
    {
        NotInit,
        Initialized,
        Editable
    }
    public SendSerialData sendSerialData;
    public Camera cameraTr;
    public Material gridMaterial; // 网格材质
    public GameObject objectToSpawn; // 生成するオブジェクト
    private GameObject SpawnedObject; // 生成済みオブジェクト
    public OVRFaceExpressions faceExpressions;
    public AudioSource audioSource;
    public TextMeshPro mouthText;
    public OVRHand ovrHand;
    private Dictionary<OVRSkeleton.BoneId, Transform> boneTransforms = new Dictionary<OVRSkeleton.BoneId, Transform>();

    private OVRSkeleton ovrSkeleton;
    public float attachmentDistance = 0.15f; // 少于这个距离的时候然对象跟随手部移动
    private bool isAttached = false; // 用于判断对象是否已经附加
    private float objectOffsetFromHand = 0.1f; // 对象与手部的偏移距离
    private Vector3 deskPosition;
    private GameObject deskPlane;
    private Vector3 objectPosition;// 要生成的对象的位置
    private float bendThresholdAngle = 15.0f; // 角度阈值，降低阈值以提高检测敏感性
    private bool skeletonInitialized = false;

    private Vector3 velocity = Vector3.zero;

    private bool isMouthOpen = false;
    private float previousJawDropWeight = 0f; // 前回のJawDrop値
    private float jawDropRate = 0f;          // JawDropの変化率

    public float MouthOpenThreshold = 0.2f;  // 開口閾値
    public float MouthCloseThreshold = 0.1f; // 閉口閾値
    public float MouthOpenRateThreshold = 0.02f;  // 開口の変化率閾値
    public float MouthCloseRateThreshold = -0.02f; // 閉口の変化率閾値
    public float PopThreshold = 0.90f;

    private bool hasEated = false; // 一度食べた状態を保持
    private float eatDistance = 0.3f; // オブジェクトと口の距離閾値
    EatableAir eatableAir;
    ScriptState scriptState = ScriptState.NotInit;

    IEnumerator Start()
    {
        // 等待几帧
        yield return new WaitForSeconds(0.5f); // 或者等待几帧 yield return null;

        Init();
    }
    void Init()
    {
        if (MRUK.Instance == null) return;
        // 订阅场景加载事件，确保场景数据加载完成后执行后续操作
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
        if (faceExpressions == null)
        {
            faceExpressions = GetComponent<OVRFaceExpressions>();
        }

        // 初始化 MRUK 并加载设备上的场景数据
        try
        {
            MRUK.Instance.LoadSceneFromDevice();
        }
        catch (Exception ex)
        {
            //Debug.LogError("场景加载失败: " + ex.Message);
            // 处理错误，例如加载默认场景或提示用户
            noRoomCreatPlane();
        }

   
        // オブジェクトを初期化
     
        scriptState = ScriptState.Initialized;
    }
    void Update()
    {
        switch (scriptState)
        {
            case  ScriptState.NotInit:
                break; 
            case ScriptState.Initialized:
                DetectMouthState();
                bone();
                break;
            case ScriptState.Editable:
                break;
        }
        
    }

    void DetectMouthState()
    {
        if (faceExpressions != null && faceExpressions.ValidExpressions)
        {
            // 現在のJawDrop値を取得
            float currentJawDropWeight = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.JawDrop);
            jawDropRate = currentJawDropWeight - previousJawDropWeight;

            // 開口判定
            if (!isMouthOpen && currentJawDropWeight > MouthOpenThreshold && jawDropRate > MouthOpenRateThreshold)
            {
                isMouthOpen = true;
                Debug.Log("Mouth Open Detected");

            }
            // 閉口判定
            else if (isMouthOpen && currentJawDropWeight < MouthCloseThreshold && jawDropRate < MouthCloseRateThreshold)
            {
                isMouthOpen = false;
                // オーディオ再生
                Debug.Log("Mouth Close Detected");
                if (currentJawDropWeight > PopThreshold && jawDropRate < MouthCloseRateThreshold)
                {
                    // オーディオ再生
                    audioSource.Play();
                    Debug.Log("Maximum JawDrop Detected and Rate Turned Negative");
                    if (eatableAir != null) eatableAir.ChangeState(AirState.Popped);
                }
                // 閉口時の処理
                if (Vector3.Distance(SpawnedObject.transform.position, cameraTr.transform.position) < eatDistance && !hasEated)
                {
                    hasEated = true;
                    sendSerialData.SendData("Eated");
                    Debug.Log("Eating Action Detected!");
                    HandleEating();
                }
            }
            

            // JawDrop値を更新
            previousJawDropWeight = currentJawDropWeight;

            // UI更新
        }
    }
    void bone()
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

            Transform Hand_Thumb2Transform = GetBoneTransform(OVRSkeleton.BoneId.Hand_Thumb2);
            // 获取食指的三个关键骨骼点
            Transform proximal = GetBoneTransform(OVRSkeleton.BoneId.Hand_Index1);  // 指根
            Transform middle = GetBoneTransform(OVRSkeleton.BoneId.Hand_Index2);    // 指节
            Transform tip = GetBoneTransform(OVRSkeleton.BoneId.Hand_Index3);       // 指尖
            if (isMiddleFingerBent && isRingFingerBent && isPinkyFingerBent && !isAttached)
            {
                // 计算形成面的向量
                Vector3 vector1 = middle.position - proximal.position;
                Vector3 vector2 = tip.position - middle.position;

                // 计算法向量（叉积）
                Vector3 normal = Vector3.Cross(vector1, vector2).normalized;

                // 计算平面方程中的常数项 D
                float D = -Vector3.Dot(normal, middle.position);
                // 点到平面的距离公式: d = |Ax + By + Cz + D| / sqrt(A^2 + B^2 + C^2)
                float distance = Mathf.Abs(normal.x * SpawnedObject.transform.position.x + normal.y * SpawnedObject.transform.position.y + normal.z * SpawnedObject.transform.position.z + D) /
                                 Mathf.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
                Debug.Log("distance------" + distance);
                // 计算手和对象之间的距离
                float distanceToHand = Vector3.Distance(Hand_Thumb2Transform.position, SpawnedObject.transform.position);
                // 获取对象的 Collider
                MeshCollider meshCollider = SpawnedObject.GetComponent<MeshCollider>();
                float distanceToSurface = distanceToHand; // 默认值为中心距离
                if (meshCollider != null)
                {
                    // 使用 Mesh Collider 的边界盒尺寸来近似对象的半径
                    float objectRadius = meshCollider.bounds.extents.magnitude;

                    // 计算大拇指到对象表面的距离
                    distanceToSurface = Mathf.Max(0, distanceToHand - objectRadius);
                }
                else
                {
                    Debug.LogWarning("Object does not have a Collider component.");
                }

                if (distanceToSurface <= attachmentDistance)
                {
                    isAttached = true;
                    // 将对象附加到手部对象上
                    SpawnedObject.transform.SetParent(Hand_Thumb2Transform);
                    Vector3 offsetPosition = normal * objectOffsetFromHand;
                    // 将对象的局部位置设置为计算出的偏移位置
                    SpawnedObject.transform.localPosition = Vector3.SmoothDamp(SpawnedObject.transform.localPosition, offsetPosition, ref velocity, 0.2f);
                    // 重置对象的局部旋转
                    SpawnedObject.transform.localRotation = Quaternion.identity;

                }

            }
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
        return midTipAngle > bendThresholdAngle;
    }

    Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
    {
        if (boneTransforms.ContainsKey(boneId))
        {
            return boneTransforms[boneId];
        }
        Debug.Log("not find---" + boneId);
        return null; // 找不到对应的骨骼
    }

    void HandleEating()
    {
        

        // オブジェクトを非アクティブ化
        SpawnedObject.SetActive(false);

        // コンポーネント取得
        

        // 一定時間後に再生成
        StartCoroutine(RespawnObject());
    }

    IEnumerator RespawnObject()
    {
        yield return new WaitForSeconds(2f);
        SpawnedObject.SetActive(true);
        hasEated = false;
        Debug.Log("Object Respawned!");
    }

    void OnSceneLoaded()
    {
        Transform cameraTransform = cameraTr.transform;
        Debug.Log("cameraTr.transform-----" + cameraTr.transform.position);
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            // 使用 Anchors 属性获取所有锚点
            var deskAnchors = currentRoom.Anchors.Where(anchor => anchor.HasLabel("TABLE")).ToList();
            if (deskAnchors == null || deskAnchors.Count == 0)
            {
                noRoomCreatPlane();
            }
            else
            {
                foreach (var deskAnchor in deskAnchors)
                {
                    // 获取桌面锚点的位置和大小
                    //Vector3 deskPosition = deskAnchor.PlaneRect.Value.center;
                    deskPosition = deskAnchor.transform.position;
                    Vector3 anchorSize = deskAnchor.PlaneRect.Value.size;
                    Quaternion deskRotation = deskAnchor.transform.rotation; // 获取锚点的旋转信息

                    Debug.Log("---x---" + deskPosition.x + "---y---" + deskPosition.y + "---z---" + deskPosition.z);
                    Debug.Log("锚点位置: " + deskPosition);
                    Debug.Log("桌面平面尺寸: " + anchorSize.x + "----" + anchorSize.y);

                    // 创建一个平面并将其定位到桌面锚点的位置
                    deskPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    deskPlane.transform.position = new Vector3(deskPosition.x, deskPosition.y, deskPosition.z + 0.1f); // 保持与锚点一致的高度
                    //deskPlane.transform.rotation = deskRotation; // 设置平面的旋转，使其方向与锚点一致

                    // 根据锚点的尺寸调整平面的大小
                    deskPlane.transform.localScale = new Vector3(anchorSize.y / 10f, 1, anchorSize.x / 10f); // Unity 的平面默认大小是 10x10 单位

                    // 应用网格材质
                    if (gridMaterial != null)
                    {
                        Renderer renderer = deskPlane.GetComponent<Renderer>();
                        renderer.enabled = false; // 禁用渲染器，完全隐藏平面

                    }
                    // 在平面上生成一个对象，距离摄像机水平距离40cm
                    objectPosition = cameraTransform.position + cameraTransform.forward * 0.5f; // 水平距离40cm
                                                                                                // objectPosition.y = deskPosition.y; // 保持与平面一致的高度
                    objectPosition.y = deskPosition.y + objectToSpawn.transform.localScale.y / 2; // 保持与平面一致的高度
                    // 在桌面上生成虚拟对象
                    SpawnedObject = Instantiate(objectToSpawn, objectPosition, Quaternion.identity);
                    eatableAir = SpawnedObject.GetComponent<EatableAir>();
                }
                //bottleSpawn();
            }
        }
        else
        {
            Debug.Log("no room");
            noRoomCreatPlane();
        }

    }
    void noRoomCreatPlane()
    {
        Transform cameraTransform = cameraTr.transform;
        // 设置平面的位置和尺寸
        Vector3 planePosition = cameraTransform.position + cameraTransform.forward * 0.1f; // 距离摄像机水平距离10cm
        planePosition.y -= 0.4f; // 垂直距离50cm

        // 创建一个平面
        deskPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        deskPlane.transform.position = planePosition;

        // 设置平面的大小 (Unity 的平面默认是 10x10 单位，按比例缩放)
        deskPlane.transform.localScale = new Vector3(2f / 10f, 1, 1.4f / 10f); // 宽70cm (7/10)，长1米4 (14/10)

        // 设置平面与摄像机的旋转一致
        deskPlane.transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

        // 应用网格材质
        if (gridMaterial != null)
        {
            Renderer renderer = deskPlane.GetComponent<Renderer>();
            renderer.material = gridMaterial;
            renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f); // 设置透明度
        }

        // 在平面上生成一个对象，距离摄像机水平距离40cm
        objectPosition = cameraTransform.position + cameraTransform.forward * 0.5f; // 水平距离40cm
        objectPosition.y = planePosition.y; // 保持与平面一致的高度

        if (objectToSpawn != null)
        {
            SpawnedObject = Instantiate(objectToSpawn, objectPosition, Quaternion.identity);
        }
        // bottleSpawn();
    }

}
