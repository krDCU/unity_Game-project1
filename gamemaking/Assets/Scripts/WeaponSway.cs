using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    // ���� ��ġ.
    private Vector3 originPos;

    // ���� ��ġ
    private Vector3 currentPos;

    // sway �Ѱ�
    [SerializeField] private Vector3 limitPos;

    // ������ sway �Ѱ�
    [SerializeField] private Vector3 fineSightlimitPos;


    // �ε巯�� ������ ����
    [SerializeField] private Vector3 smoothSway;

    // �ʿ� ������Ʈ
    [SerializeField] private GunController theGunController;

    void Start()
    {
        originPos = this.transform.localPosition;
        theGunController = FindObjectOfType<GunController>();
    }

    
    void Update()
    {
        TrySway();
    }

    private void TrySway()
    {
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
        {
            Swaying();
        }
        else
            BackToriginPos();
    }

    private void Swaying()
    {
        float _moveX = Input.GetAxisRaw("Mouse X");
        float _moveY = Input.GetAxisRaw("Mouse Y");

        if(!theGunController.isFineSightMode) // �Ϲ� ���
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -limitPos.x, limitPos.x), // X ��
                       Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.x), -limitPos.y, limitPos.y), // Y ��
                                              originPos.z); // Z��
        }
        else // ������
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.y), -fineSightlimitPos.x, fineSightlimitPos.x), // X ��
                       Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -fineSightlimitPos.y, fineSightlimitPos.y), // Y ��
                                              originPos.z); // Z��
        }
        transform.localPosition = currentPos;

    }

    private void BackToriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
