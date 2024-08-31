using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Linq;

public class DeskDetector : MonoBehaviour
{
    public GameObject virtualObjectPrefab; // 要放置在桌面上的虚拟对象
    public Material gridMaterial; // 网格材质
    void Start()
    {
        // 订阅场景加载事件，确保场景数据加载完成后执行后续操作
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
        // 初始化 MRUK 并加载设备上的场景数据
        MRUK.Instance.LoadSceneFromDevice();
    }

    void OnSceneLoaded()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            // 使用 Anchors 属性获取所有锚点
            var deskAnchors = currentRoom.Anchors.Where(anchor => anchor.HasLabel("TABLE")).ToList();

            foreach (var deskAnchor in deskAnchors)
            {
                // 获取桌面锚点的位置和大小
                //Vector3 deskPosition = deskAnchor.GetAnchorCenter();
                Vector3 deskPosition = deskAnchor.PlaneRect.Value.center;
                Vector3 anchorSize = deskAnchor.PlaneRect.Value.size;
                Quaternion deskRotation = deskAnchor.transform.rotation; // 获取锚点的旋转信息

                Debug.Log("---x---" + deskPosition.x+"---y---"+deskPosition.y+ "---z---" + deskPosition.z);

                if (deskAnchor.PlaneRect.HasValue)
                {
                    Vector2 planeSize = deskAnchor.PlaneRect.Value.size;
                    Debug.Log("桌面平面尺寸: " + planeSize);
                    Debug.Log("桌面平面尺寸: " + planeSize.x+ "----"+ planeSize.y);
                }

                // 创建一个平面并将其定位到桌面锚点的位置
                GameObject deskPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                deskPlane.transform.position = new Vector3(deskPosition.x, deskPosition.y, deskPosition.z); // 保持与锚点一致的高度
                deskPlane.transform.rotation = deskRotation; // 设置平面的旋转，使其方向与锚点一致


                // 根据锚点的尺寸调整平面的大小
                deskPlane.transform.localScale = new Vector3(anchorSize.y  / 10f, 1 , anchorSize.x / 10f); // Unity 的平面默认大小是 10x10 单位

                // 应用网格材质到桌面
                if (gridMaterial != null)
                {
                    deskPlane.GetComponent<Renderer>().material = gridMaterial;
                }
                else
                {
                    Debug.LogWarning("未指定网格材质！");
                }

                // 使生成的平面仅用于可视化（如需要可以禁用其碰撞器）
                //Destroy(deskPlane.GetComponent<Collider>());
/*                if (deskAnchor.HasVolume)
                {
                    Vector3 volumeSize = deskAnchor.VolumeBounds.Value.size;
                    Debug.Log("桌面体积尺寸: " + volumeSize);
                }*/

                // 在桌面上生成虚拟对象
                GameObject virtualObject = Instantiate(virtualObjectPrefab, deskAnchor.GetAnchorCenter(), Quaternion.identity);
                virtualObject.transform.localScale = anchorSize; // 根据锚点尺寸调整虚拟物体大小


            }
        }
        else
        {
            Debug.Log("no room");
        }


    }
}
