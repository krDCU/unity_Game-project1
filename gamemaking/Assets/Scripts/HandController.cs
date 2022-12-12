using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // 활성화 여부
    public static bool isActivate = false;

    [SerializeField] private Hand currentHand; // 현재 장착된 Hand형 타입 무기
    private bool isAttack = false; // 공격중
    private bool isSwing = false;

    private RaycastHit hitInfo;
    
        
    void Update()
    {
        if(isActivate)
            TryAttack();
    }

    private void TryAttack()
    {
        // Fire1(왼쪽 마우스) 클릭시 코루틴 실행
        if(Input.GetButton("Fire1"))
        {
            if(!isAttack)
            {
                //코루틴 실행
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true; // isAttack 활성화
        currentHand.animatation.SetTrigger("Attack"); // Attack 애니메이션 실행

        yield return new WaitForSeconds(currentHand.attackDelayA); // attackDelayA만큼 딜레이 후 공격실행
        isSwing = true; // 스윙 활성화

        StartCoroutine(HitCoroutine()); // 적중여부 할수 있는 HitCoroutine을 반복 실행

        yield return new WaitForSeconds(currentHand.attackDelayB); // attackDelayB만큼 딜레이 후 공격끝 실행
        isSwing = false; // 스윙 비활성화 => HitCoroutine 비활성화

        // Delay A와 Delay B를 뺀만큼 대기(이미 대기한것을 뺌)
        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false; // isAttack을 비활성화 시켜서 다시 공격할 수 있도록 설정
    }

    IEnumerator HitCoroutine()
    {
        while(isSwing) // isSwing이 true일때 계속 반복 실행
        {
            if(CheckObject())
            {
                //충돌했음.
                isSwing = false; // 적중했으면 공격 멈춤(데미지가 계속 들어가지 않도록)
                Debug.Log(hitInfo.transform.name); // 충돌한게 무엇인지 출력
            }
            else
            {
                //충돌 X
            }
            yield return null;
        }
    }

    private bool CheckObject()
    {
        //전방에 오브젝트가 있을경우 true 반환 / 없으면 false 반환 
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
