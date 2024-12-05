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
        // �R���g���[���[�̓��͂�����
        HandleControllerInput();

    }

    // �R���g���[���[����̓��͂�����
    private void HandleControllerInput()
    {
        if (!OVRInput.IsControllerConnected(OVRInput.Controller.Touch))
        {
            return;
        }
        // Button.One �Ńn���h�g���b�L���O�̒Ǐ]��Ԃ�؂�ւ�
        if (OVRInput.GetUp(OVRInput.Button.One)) // �R���g���[���[�̃{�^��1
        {
            planeAndObjectSpawner_Nakata.ForceAttach();
            originObject.SetActive(false);
        }
        if (OVRInput.GetDown(OVRInput.Button.One)) // �R���g���[���[�̃{�^��1
        {

            originObject.SetActive(true);
        }
        // Button.Two �ŃI�u�W�F�N�g�������ʒu�ɖ߂�
        if (OVRInput.GetDown(OVRInput.Button.Two)) // �R���g���[���[�̃{�^��2
        {
            ResetObjectPosition();
        }

        // ���X�e�B�b�N�̕���
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // �E�X�e�B�b�N�̕���
        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector3 delta = new Vector3(rightStick.x, leftStick.y, rightStick.y) * controllerMoveSpeed;
        planeAndObjectSpawner_Nakata.objectPosition += delta;
        originObject.transform.position = planeAndObjectSpawner_Nakata.objectPosition;
    }



  

    // �I�u�W�F�N�g�������ʒu�ɖ߂�
    private void ResetObjectPosition()
    {
        planeAndObjectSpawner_Nakata.ChangeAttaching(false, null);
        planeAndObjectSpawner_Nakata.ResetAir();
    }


}
