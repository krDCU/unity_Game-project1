using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // 활성화 여부
    public static bool isActivate = true;

    // 현재 장착 된 총
    [SerializeField] private Gun currentGun;

    // 연사속도 계산
    private float currentFireRate; // Gun 스크립트에 있는 FireRate와 같음 
                                   // => 1초에 1씩 감소 , currentFireRate가 0이 되면 발사가능

    // 효과음
    private AudioSource audioSource;


    // 상태 변수
    private bool isReload = false ; // isReload가 false 일때만 발사
    public bool isFineSightMode = false ; // isFineSightMode => 정조준상태 , false면 기본 상태 / true면 정조준 상태

    [SerializeField] private Vector3 originPos; // 본래 포지션 값

    // 레이저 충돌 정보 받아옴
    private RaycastHit hitInfo;

    // 필요한 컴포넌트 (카메라정보)
    [SerializeField] private Camera theCam;
    private Crosshair theCrosshair;
    private PlayerController thePlayerController;

    // 타격 이펙트
    [SerializeField] GameObject hit_effect_prefab;

    void Start()
    {
        thePlayerController = FindObjectOfType<PlayerController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        originPos = Vector3.zero; // originPos 초기화
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition;

        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.gunAnimation; ;

    }
    void Update()
    {
        if(isActivate)
        {
            GunFireRateCalc(); // 연사속도 계산
            TryFire(); // 총알 발사
            TryReload(); // 수동 재장전 
            TryFineSight(); // 정조준
        }
        
    }
    
    // 연사속도 재계산
    private void GunFireRateCalc()
    {
        // 0이 되었을 때 발사
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime; // currentFireRate가 0보다 클경우 1초에 1씩 감소
    }

    // 발사 시도
    private void TryFire()
    {
        //Fire1의 입력과 currentFireRate가 0보다 작거나 같을경우 발사
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload && !thePlayerController.isRun)
        {
            GunFire();
        }
    }

    // 발사 전 계산
    private void GunFire() //발사 전
    {
        if(!isReload) // isReload가 false일 경우(발사가능상태)에만 발사준비 실행
        {
            if (currentGun.currentBulletCount > 0) // 탄알집에 남아있는 총알의 개수가 0이상일때 발사
                Shoot(); // 발사 함수
            else // 하나도 없을 때 재장전
            {
                CancleFineSight(); // 총알이 하나도 없어서 재장전할때 정조준 해제
                StartCoroutine(ReloadCoroutine());
            }
        }
        
    }

    // 발사 후 계산
    private void Shoot() //발사 후
    {
        theCrosshair.FireAnimaiton();
        currentGun.currentBulletCount--; // 발사 후 탄알집에 들어있는 탄알 감소
        currentFireRate = currentGun.fireRate; // currentFireRate에 currentGun의 fireRate를 넣음
        // 다시 0.2가 들어가서 0으로 감소 할때 까지 발사 불가능
        // 연사속도 재계산
        PlaySE(currentGun.fire_Sound); // 총알 발사 사운드
        currentGun.muzzleFlash.Play(); // 총알 발사 이펙트

        Hit();

        // 총기 반동 코루틴 실행
        StopAllCoroutines(); // 코루틴 중복 실행 방지
        StartCoroutine(RetroActionCoroutine()); // 총기 반동 코루틴
        
        // Debug.Log("총알 발사");
    }

    private void Hit()
    {
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward + 
            new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                        Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy), 0)
                        , out hitInfo, currentGun.range))
        {
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);        
        }
    }

    // 효과음 재생
    private void PlaySE(AudioClip _clip) // 효과음 재생
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    // 재장전 시도
    private void TryReload() // 재장전 코드
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancleFineSight(); // FineSight를 다시 실행시켜 !isFineSightMode을 실행시켜 정조준 해제
            //R 입력키 && !isReload(재장전 상태 false 일때) && 탄알집에 남아있는 총알 개수 < 재장전 총알 개수 때 실행
            StartCoroutine(ReloadCoroutine());
        }
    }

    // 재장전 취소
    public void CancelReload()
    {
        if(isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }


    // 재장전
    IEnumerator ReloadCoroutine() // 재장전 대기를 위해 코루틴 사용
    {
        if(currentGun.carryBulletCount > 0) // 현재 소유하고 있는 총알의 개수가 0 이상일 때
        {
            isReload = true; // 재장전 시작하면 isReload를 true로 변환시켜 발사를 못 하도록.

            currentGun.gunAnimation.SetTrigger("Reload"); // Reload 애니메이터 실행

            currentGun.carryBulletCount += currentGun.currentBulletCount; // 현재 탄알집에 남아있는 총알의 개수를 현재 소유 중인 총알의 개수에 더해줌
            currentGun.currentBulletCount = 0; // 탄알집에 남아있는 총알을 현재 소유중인 총알에 넘겨줬기 때문에 0으로.
            //=> 만약 탄알집에 15발이 남은 상태에서 재장전을 하면 15발이 허공으로 날라가버리지 않도록 현재 소유한 총알에 포함시키는 것임.

            yield return new WaitForSeconds(currentGun.reloadTime); // 재장전 시간만큼 대기

            if(currentGun.carryBulletCount >= currentGun.reloadBulletCount) // 현재 소유한 총알의 개수가 재장전 총알의 수보다 크거가 같을경우
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount; // 현재 탄알집에 남아있는 총알의 개수에 총알 재장전 수를 넣어줌. 전부 재장전
                currentGun.carryBulletCount -= currentGun.reloadBulletCount; // 현재 소유한 총알 개수에서 재장전 총알 개수만큼 감소
            }
            else // 현재 소유한 총알의 개수가 재장전 총알의 수보다 적을 경우
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount; // 현재 탄알집에 남아있는 총알에 현재 소유한 총알의 개수를 전부 재장전
                currentGun.carryBulletCount = 0; // 현재 소유한 모든 총알을 재장전 했기 때문에 0으로
            }

            isReload = false; // 재장전이 끝나면 다시 false로 바꿔 발사가 가능한 상태로.

        }

        else // 총알이 없을 경우
        {
            Debug.Log("소유한 총알이 없습니다.");
        }

    }

    // 정조준 시도
    private void TryFineSight() // 마우스 우클릭 시 정조준 함수로 이동
    {
        if(Input.GetButtonDown("Fire2") && !isReload) // 마우스 우클릭 && 재장전이 아닐 때 정조준이 이루어지도록.
        {
            FineSight();
        }
    }

    // 정조준 취소
    public void CancleFineSight() // 정조준 해제
    {
        if (isFineSightMode) // FineSightMode가 True면 FineSight를 실행
            FineSight();
    }


    // 정조준 로직 가동
    private void FineSight() // 정조준 함수
    {
        
        isFineSightMode = !isFineSightMode; // !isFineSightMode는 isFineSightMode = ture와 같은 의미 / 실행될때마다 인버터 역할
        currentGun.gunAnimation.SetBool("FineSightMode", isFineSightMode); // 애니메이션 FineSightMode을 isFineSightMode(true)로 바꿔줌
        theCrosshair.FineSightAnmation(isFineSightMode);

        if (isFineSightMode) // 정조준 모드일때
        {
            StopAllCoroutines(); // 코루틴 시작 전에 기존에 실행하고 있던 코루틴들을 중단시켜 정조준과 원위치의 수치가 정확하게 다시 바뀔수 있도록 구현
            StartCoroutine(FineSightActivateCoroutine());
        }
        else // 정조준 모드가 아닐 때
        {
            StopAllCoroutines();// 코루틴 시작 전에 기존에 실행하고 있던 코루틴들을 중단시켜 정조준과 원위치의 수치가 정확하게 다시 바뀔수 있도록 구현
            StartCoroutine(FineSightDeActivateCoroutine());
        }

    }

    // 정조준 활성화
    IEnumerator FineSightActivateCoroutine() // 정조준 코루틴
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos) 
        {
            //현재 총의 localPosition(현재 위치)가 fineSightOriginPos(정조준위치)가 될 때까지 반복
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.3f); // Lerp는 mathf lerp와 동일한 기능
            // currentGun.transform.localPosition(현재위치)에서 currentGun.fineSightOriginPos(정조준위치)까지 0.2f의 힘으로 반복
            yield return null; // 1프레임 대기
        }
        
    }

    // 정조준 비활성화
    IEnumerator FineSightDeActivateCoroutine() // 정조준 -> 원래 포지션으로 바뀌는 코루틴 
    {
        while (currentGun.transform.localPosition != originPos)
        {
            //현재 총의 localPosition(현재 위치)가 fineSightOriginPos(정조준위치)가 될 때까지 반복
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.3f); // Lerp는 mathf lerp와 동일한 기능
            // currentGun.transform.localPosition(현재위치)에서 currentGun.fineSightOriginPos(정조준위치)까지 0.2f의 힘으로 반복
            yield return null; // 1프레임 대기
        }
    }

    // 반동 코루틴
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z); // 정조준 안 했을때, 일반 발사 때의 최대 반동
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); // 정조준 했을 떄의 최대 반동

        if(!isFineSightMode) // 정조준 상태가 아닐경우
        {

            currentGun.transform.localPosition = originPos; // 반동이 중첩되어 반동된 상태에서 반동의 위치로 가지 않도록 처음 시작할 때 총의 위치를 원 위치로 초기화

            // 반동 시작
            while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            // 원 위치
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else // 정조준 상태일 경우 
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;
            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // 원 위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }


        }

    }

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }

    public void GunChange(Gun _gun)
    {
        if(WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        currentGun = _gun;
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.gunAnimation;

        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
