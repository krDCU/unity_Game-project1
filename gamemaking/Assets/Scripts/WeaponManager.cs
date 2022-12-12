using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(GunController))]
public class WeaponManager : MonoBehaviour
{

    // 공유 자원 / 클래스 변수(정적 변수) / 무기 중복 교체 실행 방지
    public static bool isChangeWeapon = false;
    // 현재 무기와 현재 무기의 애니메이션
    public static Transform currentWeapon;
    public static Animator currentWeaponAnim;
    // 현재 무기 타입
    [SerializeField] private string currentWeaponType;

    // 무기 교체 딜레이
    [SerializeField] float changeWeaponDelayTime;
    // 무기 교체가 완전히 끝나는 시점
    [SerializeField] float changeWeaponEndDelayTime;

    // 무기 종류 관리
    [SerializeField] private Gun[] guns;
    [SerializeField] private Hand[] hands;

    // 쉽게 배열 관리를 할 수 있도로 무기 접근이 가능하도록
    private Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();
    private Dictionary<string, Hand> handDictionary = new Dictionary<string, Hand>();

    // 필요한 컴포넌트
    [SerializeField] private GunController theGunController;
    [SerializeField] private HandController theHandController;

    void Start()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            gunDictionary.Add(guns[i].gunName, guns[i]);
        }
        for (int i = 0; i < hands.Length; i++)
        {
            handDictionary.Add(hands[i].handName, hands[i]);
        }
    }

    void Update()
    {
        // 퀵슬롯 구현
        if (!isChangeWeapon)
        {
            // 숫자패드 1 누를시
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartCoroutine(ChangeWeaponCoroutine("HAND", "맨손"));
            // 숫자패드 2 누를시
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun"));
        }
    }

    // 무기교체 코루틴
    public IEnumerator ChangeWeaponCoroutine(string _type, string _name)
    {
        isChangeWeapon = true;
        currentWeaponAnim.SetTrigger("Weapon_Out");

        yield return new WaitForSeconds(changeWeaponDelayTime); // 무기 교체 딜레이만큼 대기

        CancelPreWeaponAction(); // 무기교체시 무기동작 취소
        WeaponChange(_type, _name);

        yield return new WaitForSeconds(changeWeaponEndDelayTime); // 무기 교체 종료 시점만큼 대기

        currentWeaponType = _type;
        isChangeWeapon = false;
    }

    private void CancelPreWeaponAction()
    {
        switch (currentWeaponType)
        {
            case "GUN":
                theGunController.CancleFineSight(); // 무기 교체 시 정조준 취소
                theGunController.CancelReload(); // 무기 교체 시 재장전 취소
                GunController.isActivate = false;
                break;
            case "HAND":
                HandController.isActivate = false;
                break;
        }
    }

    private void WeaponChange(string _type, string _name)
    {
        if(_type == "GUN")
        {
            theGunController.GunChange(gunDictionary[_name]);
        }
        if(_type == "HAND")
        {
            theHandController.HandChange(handDictionary[_name]);
        }
    }

}
