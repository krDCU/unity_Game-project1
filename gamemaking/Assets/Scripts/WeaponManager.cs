using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(GunController))]
public class WeaponManager : MonoBehaviour
{

    // ���� �ڿ� / Ŭ���� ����(���� ����) / ���� �ߺ� ��ü ���� ����
    public static bool isChangeWeapon = false;
    // ���� ����� ���� ������ �ִϸ��̼�
    public static Transform currentWeapon;
    public static Animator currentWeaponAnim;
    // ���� ���� Ÿ��
    [SerializeField] private string currentWeaponType;

    // ���� ��ü ������
    [SerializeField] float changeWeaponDelayTime;
    // ���� ��ü�� ������ ������ ����
    [SerializeField] float changeWeaponEndDelayTime;

    // ���� ���� ����
    [SerializeField] private Gun[] guns;
    [SerializeField] private Hand[] hands;

    // ���� �迭 ������ �� �� �ֵ��� ���� ������ �����ϵ���
    private Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();
    private Dictionary<string, Hand> handDictionary = new Dictionary<string, Hand>();

    // �ʿ��� ������Ʈ
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
        // ������ ����
        if (!isChangeWeapon)
        {
            // �����е� 1 ������
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartCoroutine(ChangeWeaponCoroutine("HAND", "�Ǽ�"));
            // �����е� 2 ������
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun"));
        }
    }

    // ���ⱳü �ڷ�ƾ
    public IEnumerator ChangeWeaponCoroutine(string _type, string _name)
    {
        isChangeWeapon = true;
        currentWeaponAnim.SetTrigger("Weapon_Out");

        yield return new WaitForSeconds(changeWeaponDelayTime); // ���� ��ü �����̸�ŭ ���

        CancelPreWeaponAction(); // ���ⱳü�� ���⵿�� ���
        WeaponChange(_type, _name);

        yield return new WaitForSeconds(changeWeaponEndDelayTime); // ���� ��ü ���� ������ŭ ���

        currentWeaponType = _type;
        isChangeWeapon = false;
    }

    private void CancelPreWeaponAction()
    {
        switch (currentWeaponType)
        {
            case "GUN":
                theGunController.CancleFineSight(); // ���� ��ü �� ������ ���
                theGunController.CancelReload(); // ���� ��ü �� ������ ���
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
