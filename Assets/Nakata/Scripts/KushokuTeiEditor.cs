using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class KushokuTeiEditorClass : MonoBehaviour
{
    [SerializeField] PlaneAndObjectSpawner_Nakata planeAndObjectSpawner_Nakata;
    [SerializeField] GameObject originObject;

    public float controllerMoveSpeed = 1;



    private void Update()
    {
        // コントローラーの入力を処理
        HandleControllerInput();

    }

    // コントローラーからの入力を処理
    private void HandleControllerInput()
    {
        if (!OVRInput.IsControllerConnected(OVRInput.Controller.Touch))
        {
            return;
        }
        // Button.One でハンドトラッキングの追従状態を切り替え
        if (OVRInput.GetUp(OVRInput.Button.One)) // コントローラーのボタン1
        {
            planeAndObjectSpawner_Nakata.ForceAttach();
            originObject.SetActive(false);
        }
        if (OVRInput.GetDown(OVRInput.Button.One)) // コントローラーのボタン1
        {

            originObject.SetActive(true);
        }
        // Button.Two でオブジェクトを初期位置に戻す
        if (OVRInput.GetDown(OVRInput.Button.Two)) // コントローラーのボタン2
        {
            ResetObjectPosition();
        }

        // 左スティックの方向
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // 右スティックの方向
        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector3 delta = new Vector3(rightStick.x, leftStick.y, rightStick.y) * controllerMoveSpeed;
        planeAndObjectSpawner_Nakata.objectPosition += delta;
        originObject.transform.position = planeAndObjectSpawner_Nakata.objectPosition;
    }



  

    // オブジェクトを初期位置に戻す
    private void ResetObjectPosition()
    {
        planeAndObjectSpawner_Nakata.ChangeAttaching(false, null);
        planeAndObjectSpawner_Nakata.ResetAir();
    }


}
