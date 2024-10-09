using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Linq;


public class PlaneAndObjectSpawner : MonoBehaviour
{
    public SendSerialData sendSerialData;
    public Camera cameraTr;
    public Material gridMaterial; // 网格材质
    public GameObject objectToSpawn; // 要生成的对象
    private GameObject SpawnedObject; // 已经生成的对象
    private Vector3 objectPosition;// 要生成的对象的位置
    public OVRFaceExpressions faceExpressions;
    public AudioSource audioSource;

    public OVRHand ovrHand;
    private OVRSkeleton ovrSkeleton;
    private float bendThresholdAngle = 15.0f; // 角度阈值，降低阈值以提高检测敏感性
    private Dictionary<OVRSkeleton.BoneId, Transform> boneTransforms = new Dictionary<OVRSkeleton.BoneId, Transform>();
    private bool skeletonInitialized = false;
    private float objectOffsetFromHand = 0.1f; // 对象与手部的偏移距离
    private float followSpeed = 5f;     // 跟随速度
    private float rotationSpeed = 5f;   // 旋转速度
    private float inertia = 0.1f;       // 惯性系数
    private float minimumDistance = 0.1f; // 检测手是否可见的最小距离
    private float maximumDistance = 2.0f; // 检测手是否可见的最大距离
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastKnownPosition;
    private bool isHandTracked = true;
    private Vector3 deskPosition;

    private float attachmentDistance = 0.15f; // 少于这个距离的时候然对象跟随手部移动
    private bool isAttached = false; // 用于判断对象是否已经附加
    private float eatDistance = 0.3f; // 距离阈值，例如 10cm
    private bool isInitialized = false; // 标志位

    private bool isMouthOpen = false;
    private float mouthOpenThreshold = 0.5f;
    private float mouthOpenTime = 0f;
    private float requiredCloseTime = 0.1f; // 需要嘴巴闭合的时间来确认吃东西的动作

    public GameObject Restaurant;
    private GameObject deskPlane;
    public GameObject bottle1;//放在桌上的瓶子
    public GameObject bottle2;//放在桌上的瓶子
    public GameObject bottle3;//放在桌上的瓶子
    public GameObject bottle4;//放在桌上的瓶子
    IEnumerator Start()
    {
        // 等待几帧
        yield return new WaitForSeconds(0.5f); // 或者等待几帧 yield return null;

        if (faceExpressions == null)
        {
            faceExpressions = GetComponent<OVRFaceExpressions>();
        }

        // 订阅场景加载事件，确保场景数据加载完成后执行后续操作
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
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
        isInitialized = true; // 初始化完成，设置标志位
    }

    void Update()
    {
        if (isInitialized)
        {
            bone();
            DetectEatingAction();
        }

    }
    void DetectEatingAction()
    {
        // 检测对象与摄像头之间的距离
        Vector3 cameraPosition = cameraTr.transform.position;
        float distanceToMouth = Vector3.Distance(SpawnedObject.transform.position, cameraPosition);

        if (distanceToMouth < eatDistance)
        {
            if (faceExpressions != null && faceExpressions.ValidExpressions)
            {
                // 获取下巴开合的权重值
                float jawDropWeight = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.JawDrop);

                if (jawDropWeight > mouthOpenThreshold)
                {
                    if (!isMouthOpen)
                    {
                        // 检测到嘴巴从闭合变为张开
                        isMouthOpen = true;
                        mouthOpenTime = Time.time;
                    }
                }
                //else if (isMouthOpen && Time.time - mouthOpenTime >= requiredCloseTime)
                else if (isMouthOpen)
                {
                    // 检测到嘴巴闭合，并且时间间隔在合理范围内
                    Debug.Log("检测到吃东西的动作！");
                    sendSerialData.SendData("Eated");
                    isMouthOpen = false; // 重置状态
                    audioSource.Play(); // 播放音频
                    SpawnedObject.SetActive(false); // 禁用对象
                    
                    StartCoroutine(SpawnObjectAfterDelay());  // 开始协程，在等待指定时间后生成对象
                }
            }
        }
    }
    IEnumerator SpawnObjectAfterDelay()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(2f);

        SpawnedObject.transform.position = objectPosition;
        SpawnedObject.SetActive(true);
        isAttached = false;
        // 取消对象挂载
        SpawnedObject.transform.SetParent(null);

        // 保持当前的世界位置和旋转
        SpawnedObject.transform.position = SpawnedObject.transform.position;
        SpawnedObject.transform.rotation = SpawnedObject.transform.rotation;
        Debug.Log("对象已生成！");
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

            Transform Hand_Middle3Transform = GetBoneTransform(OVRSkeleton.BoneId.Hand_Middle3);
            if (isMiddleFingerBent&& isRingFingerBent&& isPinkyFingerBent && !isAttached) 
            {
                // 计算手和对象之间的距离
                float distanceToHand = Vector3.Distance(Hand_Middle3Transform.position, SpawnedObject.transform.position);
                // 获取对象的 Collider
                MeshCollider meshCollider = SpawnedObject.GetComponent<MeshCollider>();
                // 获取手指的正前方向
                Vector3 forwardDirection = Hand_Middle3Transform.forward.normalized;
                    
                // 使用世界坐标系中的上方向作为参考方向（你也可以选择其他方向）
                Vector3 upDirection = Vector3.up;
                // 计算垂直于手指正前方向的向量
                Vector3 perpendicularDirection = Vector3.Cross(forwardDirection, upDirection).normalized;
                // 获取手指的反方向
                //Vector3 backwardDirection = -forwardDirection;
                float distanceToSurface = distanceToHand; // 默认值为中心距离
                if (meshCollider != null)
                {
                    // 使用 Mesh Collider 的边界盒尺寸来近似对象的半径
                    float objectRadius = meshCollider.bounds.extents.magnitude;

                    // 计算手指尖到对象表面的距离
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
                    SpawnedObject.transform.SetParent(Hand_Middle3Transform);
                    // 计算相对于手部的垂直方向的偏移位置
                    Vector3 offsetPosition = forwardDirection * objectOffsetFromHand;

                    // 将对象的局部位置设置为计算出的偏移位置
                    //SpawnedObject.transform.localPosition = offsetPosition;
                    SpawnedObject.transform.localPosition = Vector3.SmoothDamp(SpawnedObject.transform.localPosition, offsetPosition, ref velocity, 0.2f);
                    // 重置对象的局部旋转
                    SpawnedObject.transform.localRotation = Quaternion.identity;

                }
 
            }
        }
    }
    // 检查手是否在摄像头视野内
    bool IsHandVisible(Transform IndexTipTransform)
    {
        // 你可以根据手部的跟踪状态或者距离摄像头的距离来检测是否可见
        float distance = Vector3.Distance(IndexTipTransform.position, Camera.main.transform.position);
        return distance > minimumDistance && distance < maximumDistance;
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
    void OnSceneLoaded()
    {
        Transform cameraTransform = cameraTr.transform;
        Debug.Log("cameraTr.transform-----"+ cameraTr.transform.position);
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
                    deskPlane.transform.position = new Vector3(deskPosition.x, deskPosition.y, deskPosition.z+0.1f); // 保持与锚点一致的高度
                    //deskPlane.transform.rotation = deskRotation; // 设置平面的旋转，使其方向与锚点一致

                    // 根据锚点的尺寸调整平面的大小
                    deskPlane.transform.localScale = new Vector3(anchorSize.y / 10f, 1, anchorSize.x / 10f); // Unity 的平面默认大小是 10x10 单位
 
                    // 应用网格材质
                    if (gridMaterial != null)
                    {
                        Renderer renderer = deskPlane.GetComponent<Renderer>();
                        renderer.enabled = false; // 禁用渲染器，完全隐藏平面
                /*        renderer.material = gridMaterial;
                        //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0f); // 设置透明度
                        renderer.material.SetFloat("_Mode", 2); // 2 = Transparent mode
                        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0f); // 设置透明度
                        renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        renderer.material.SetInt("_ZWrite", 0);
                        renderer.material.DisableKeyword("_ALPHATEST_ON");
                        renderer.material.EnableKeyword("_ALPHABLEND_ON");
                        renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        renderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;*/
                    }
                    // 使生成的平面仅用于可视化（如需要可以禁用其碰撞器）
                    //Destroy(deskPlane.GetComponent<Collider>());
                    /*                if (deskAnchor.HasVolume)
                                    {
                                        Vector3 volumeSize = deskAnchor.VolumeBounds.Value.size;
                                        Debug.Log("桌面体积尺寸: " + volumeSize);
                                    }*/
                    // 在平面上生成一个对象，距离摄像机水平距离40cm
                    objectPosition = cameraTransform.position + cameraTransform.forward * 0.5f; // 水平距离40cm
                   // objectPosition.y = deskPosition.y; // 保持与平面一致的高度
                    objectPosition.y = deskPosition.y + objectToSpawn.transform.localScale.y*2f; // 保持与平面一致的高度
                    // 在桌面上生成虚拟对象
                    SpawnedObject = Instantiate(objectToSpawn, objectPosition, Quaternion.identity);
                }
                bottleSpawn();
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
        bottleSpawn();
    }
    void bottleSpawn()
    {
        //Transform RestaurantTr = Restaurant.transform;
        //float RestaurantTrScaleX = RestaurantTr.localScale.x;
        float RestaurantTrScaleX = 0.11f;

        //GameObject newObject1 = Instantiate(bottle1, new Vector3(objectPosition.x - 0.5f, objectPosition.y, objectPosition.z+0.15f), Quaternion.identity);
        //newObject1.transform.localScale = new Vector3(newObject1.transform.localScale.x * RestaurantTrScaleX, newObject1.transform.localScale.y * RestaurantTrScaleX, newObject1.transform.localScale.z * RestaurantTrScaleX);
        //newObject1.transform.position = new Vector3(newObject1.transform.position.x, newObject1.transform.position.y , newObject1.transform.position.z);

        GameObject newObject2 = Instantiate(bottle2, new Vector3(deskPosition.x - 0.2f, deskPosition.y, deskPosition.z+0.1f), Quaternion.identity);
        newObject2.transform.localScale = new Vector3(newObject2.transform.localScale.x * RestaurantTrScaleX, newObject2.transform.localScale.y * RestaurantTrScaleX, newObject2.transform.localScale.z * RestaurantTrScaleX);
        newObject2.transform.position = new Vector3(newObject2.transform.position.x, newObject2.transform.position.y + newObject2.transform.localScale.y * 1f, newObject2.transform.position.z);

        /*GameObject newObject3 = Instantiate(bottle3, new Vector3(deskPosition.x - 0.2f, deskPosition.y, deskPosition.z), Quaternion.identity);
        newObject3.transform.localScale = new Vector3(newObject3.transform.localScale.x * RestaurantTrScaleX, newObject3.transform.localScale.y * RestaurantTrScaleX, newObject3.transform.localScale.z * RestaurantTrScaleX);
        newObject3.transform.position = new Vector3(newObject3.transform.position.x, newObject3.transform.position.y + newObject3.transform.localScale.y * 1f, newObject3.transform.position.z);

        GameObject newObject4 = Instantiate(bottle4, new Vector3(deskPosition.x - 0.15f, deskPosition.y, deskPosition.z), Quaternion.identity);
        newObject4.transform.localScale = new Vector3(newObject4.transform.localScale.x * RestaurantTrScaleX, newObject4.transform.localScale.y * RestaurantTrScaleX, newObject4.transform.localScale.z * RestaurantTrScaleX);
        newObject4.transform.position = new Vector3(newObject4.transform.position.x, newObject4.transform.position.y + newObject4.transform.localScale.y * 1f, newObject4.transform.position.z);*/
    }
}
