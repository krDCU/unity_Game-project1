using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    // 기존 위치.
    private Vector3 originPos;

    // 현재 위치
    private Vector3 currentPos;

    // sway 한계
    [SerializeField] private Vector3 limitPos;

    // 정조준 sway 한계
    [SerializeField] private Vector3 fineSightlimitPos;


    // 부드러운 움직임 정도
    [SerializeField] private Vector3 smoothSway;

    // 필요 컴포넌트
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

        if(!theGunController.isFineSightMode) // 일반 사격
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -limitPos.x, limitPos.x), // X 값
                       Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.x), -limitPos.y, limitPos.y), // Y 값
                                              originPos.z); // Z값
        }
        else // 정조준
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.y), -fineSightlimitPos.x, fineSightlimitPos.x), // X 값
                       Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -fineSightlimitPos.y, fineSightlimitPos.y), // Y 값
                                              originPos.z); // Z값
        }
        transform.localPosition = currentPos;

    }

    private void BackToriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
