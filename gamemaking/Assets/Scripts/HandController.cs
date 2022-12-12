using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // Ȱ��ȭ ����
    public static bool isActivate = false;

    [SerializeField] private Hand currentHand; // ���� ������ Hand�� Ÿ�� ����
    private bool isAttack = false; // ������
    private bool isSwing = false;

    private RaycastHit hitInfo;
    
        
    void Update()
    {
        if(isActivate)
            TryAttack();
    }

    private void TryAttack()
    {
        // Fire1(���� ���콺) Ŭ���� �ڷ�ƾ ����
        if(Input.GetButton("Fire1"))
        {
            if(!isAttack)
            {
                //�ڷ�ƾ ����
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true; // isAttack Ȱ��ȭ
        currentHand.animatation.SetTrigger("Attack"); // Attack �ִϸ��̼� ����

        yield return new WaitForSeconds(currentHand.attackDelayA); // attackDelayA��ŭ ������ �� ���ݽ���
        isSwing = true; // ���� Ȱ��ȭ

        StartCoroutine(HitCoroutine()); // ���߿��� �Ҽ� �ִ� HitCoroutine�� �ݺ� ����

        yield return new WaitForSeconds(currentHand.attackDelayB); // attackDelayB��ŭ ������ �� ���ݳ� ����
        isSwing = false; // ���� ��Ȱ��ȭ => HitCoroutine ��Ȱ��ȭ

        // Delay A�� Delay B�� ����ŭ ���(�̹� ����Ѱ��� ��)
        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false; // isAttack�� ��Ȱ��ȭ ���Ѽ� �ٽ� ������ �� �ֵ��� ����
    }

    IEnumerator HitCoroutine()
    {
        while(isSwing) // isSwing�� true�϶� ��� �ݺ� ����
        {
            if(CheckObject())
            {
                //�浹����.
                isSwing = false; // ���������� ���� ����(�������� ��� ���� �ʵ���)
                Debug.Log(hitInfo.transform.name); // �浹�Ѱ� �������� ���
            }
            else
            {
                //�浹 X
            }
            yield return null;
        }
    }

    private bool CheckObject()
    {
        //���濡 ������Ʈ�� ������� true ��ȯ / ������ false ��ȯ 
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfo, currentHand.range))
        {
            return true;
        }
        return false;
    }

    public void HandChange(Hand _hand)
    {
        if (WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        currentHand = _hand;
        WeaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentHand.animatation;

        currentHand.transform.localPosition = Vector3.zero;
        currentHand.gameObject.SetActive(true);
        isActivate = true;
    }
}
