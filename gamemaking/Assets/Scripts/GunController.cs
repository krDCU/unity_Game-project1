using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // Ȱ��ȭ ����
    public static bool isActivate = true;

    // ���� ���� �� ��
    [SerializeField] private Gun currentGun;

    // ����ӵ� ���
    private float currentFireRate; // Gun ��ũ��Ʈ�� �ִ� FireRate�� ���� 
                                   // => 1�ʿ� 1�� ���� , currentFireRate�� 0�� �Ǹ� �߻簡��

    // ȿ����
    private AudioSource audioSource;


    // ���� ����
    private bool isReload = false ; // isReload�� false �϶��� �߻�
    public bool isFineSightMode = false ; // isFineSightMode => �����ػ��� , false�� �⺻ ���� / true�� ������ ����

    [SerializeField] private Vector3 originPos; // ���� ������ ��

    // ������ �浹 ���� �޾ƿ�
    private RaycastHit hitInfo;

    // �ʿ��� ������Ʈ (ī�޶�����)
    [SerializeField] private Camera theCam;
    private Crosshair theCrosshair;
    private PlayerController thePlayerController;

    // Ÿ�� ����Ʈ
    [SerializeField] GameObject hit_effect_prefab;

    void Start()
    {
        thePlayerController = FindObjectOfType<PlayerController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        originPos = Vector3.zero; // originPos �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition;

        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.gunAnimation; ;

    }
    void Update()
    {
        if(isActivate)
        {
            GunFireRateCalc(); // ����ӵ� ���
            TryFire(); // �Ѿ� �߻�
            TryReload(); // ���� ������ 
            TryFineSight(); // ������
        }
        
    }
    
    // ����ӵ� ����
    private void GunFireRateCalc()
    {
        // 0�� �Ǿ��� �� �߻�
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime; // currentFireRate�� 0���� Ŭ��� 1�ʿ� 1�� ����
    }

    // �߻� �õ�
    private void TryFire()
    {
        //Fire1�� �Է°� currentFireRate�� 0���� �۰ų� ������� �߻�
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload && !thePlayerController.isRun)
        {
            GunFire();
        }
    }

    // �߻� �� ���
    private void GunFire() //�߻� ��
    {
        if(!isReload) // isReload�� false�� ���(�߻簡�ɻ���)���� �߻��غ� ����
        {
            if (currentGun.currentBulletCount > 0) // ź������ �����ִ� �Ѿ��� ������ 0�̻��϶� �߻�
                Shoot(); // �߻� �Լ�
            else // �ϳ��� ���� �� ������
            {
                CancleFineSight(); // �Ѿ��� �ϳ��� ��� �������Ҷ� ������ ����
                StartCoroutine(ReloadCoroutine());
            }
        }
        
    }

    // �߻� �� ���
    private void Shoot() //�߻� ��
    {
        theCrosshair.FireAnimaiton();
        currentGun.currentBulletCount--; // �߻� �� ź������ ����ִ� ź�� ����
        currentFireRate = currentGun.fireRate; // currentFireRate�� currentGun�� fireRate�� ����
        // �ٽ� 0.2�� ���� 0���� ���� �Ҷ� ���� �߻� �Ұ���
        // ����ӵ� ����
        PlaySE(currentGun.fire_Sound); // �Ѿ� �߻� ����
        currentGun.muzzleFlash.Play(); // �Ѿ� �߻� ����Ʈ

        Hit();

        // �ѱ� �ݵ� �ڷ�ƾ ����
        StopAllCoroutines(); // �ڷ�ƾ �ߺ� ���� ����
        StartCoroutine(RetroActionCoroutine()); // �ѱ� �ݵ� �ڷ�ƾ
        
        // Debug.Log("�Ѿ� �߻�");
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

    // ȿ���� ���
    private void PlaySE(AudioClip _clip) // ȿ���� ���
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    // ������ �õ�
    private void TryReload() // ������ �ڵ�
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancleFineSight(); // FineSight�� �ٽ� ������� !isFineSightMode�� ������� ������ ����
            //R �Է�Ű && !isReload(������ ���� false �϶�) && ź������ �����ִ� �Ѿ� ���� < ������ �Ѿ� ���� �� ����
            StartCoroutine(ReloadCoroutine());
        }
    }

    // ������ ���
    public void CancelReload()
    {
        if(isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }


    // ������
    IEnumerator ReloadCoroutine() // ������ ��⸦ ���� �ڷ�ƾ ���
    {
        if(currentGun.carryBulletCount > 0) // ���� �����ϰ� �ִ� �Ѿ��� ������ 0 �̻��� ��
        {
            isReload = true; // ������ �����ϸ� isReload�� true�� ��ȯ���� �߻縦 �� �ϵ���.

            currentGun.gunAnimation.SetTrigger("Reload"); // Reload �ִϸ����� ����

            currentGun.carryBulletCount += currentGun.currentBulletCount; // ���� ź������ �����ִ� �Ѿ��� ������ ���� ���� ���� �Ѿ��� ������ ������
            currentGun.currentBulletCount = 0; // ź������ �����ִ� �Ѿ��� ���� �������� �Ѿ˿� �Ѱ���� ������ 0����.
            //=> ���� ź������ 15���� ���� ���¿��� �������� �ϸ� 15���� ������� ���󰡹����� �ʵ��� ���� ������ �Ѿ˿� ���Խ�Ű�� ����.

            yield return new WaitForSeconds(currentGun.reloadTime); // ������ �ð���ŭ ���

            if(currentGun.carryBulletCount >= currentGun.reloadBulletCount) // ���� ������ �Ѿ��� ������ ������ �Ѿ��� ������ ũ�Ű� �������
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount; // ���� ź������ �����ִ� �Ѿ��� ������ �Ѿ� ������ ���� �־���. ���� ������
                currentGun.carryBulletCount -= currentGun.reloadBulletCount; // ���� ������ �Ѿ� �������� ������ �Ѿ� ������ŭ ����
            }
            else // ���� ������ �Ѿ��� ������ ������ �Ѿ��� ������ ���� ���
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount; // ���� ź������ �����ִ� �Ѿ˿� ���� ������ �Ѿ��� ������ ���� ������
                currentGun.carryBulletCount = 0; // ���� ������ ��� �Ѿ��� ������ �߱� ������ 0����
            }

            isReload = false; // �������� ������ �ٽ� false�� �ٲ� �߻簡 ������ ���·�.

        }

        else // �Ѿ��� ���� ���
        {
            Debug.Log("������ �Ѿ��� �����ϴ�.");
        }

    }

    // ������ �õ�
    private void TryFineSight() // ���콺 ��Ŭ�� �� ������ �Լ��� �̵�
    {
        if(Input.GetButtonDown("Fire2") && !isReload) // ���콺 ��Ŭ�� && �������� �ƴ� �� �������� �̷��������.
        {
            FineSight();
        }
    }

    // ������ ���
    public void CancleFineSight() // ������ ����
    {
        if (isFineSightMode) // FineSightMode�� True�� FineSight�� ����
            FineSight();
    }


    // ������ ���� ����
    private void FineSight() // ������ �Լ�
    {
        
        isFineSightMode = !isFineSightMode; // !isFineSightMode�� isFineSightMode = ture�� ���� �ǹ� / ����ɶ����� �ι��� ����
        currentGun.gunAnimation.SetBool("FineSightMode", isFineSightMode); // �ִϸ��̼� FineSightMode�� isFineSightMode(true)�� �ٲ���
        theCrosshair.FineSightAnmation(isFineSightMode);

        if (isFineSightMode) // ������ ����϶�
        {
            StopAllCoroutines(); // �ڷ�ƾ ���� ���� ������ �����ϰ� �ִ� �ڷ�ƾ���� �ߴܽ��� �����ذ� ����ġ�� ��ġ�� ��Ȯ�ϰ� �ٽ� �ٲ�� �ֵ��� ����
            StartCoroutine(FineSightActivateCoroutine());
        }
        else // ������ ��尡 �ƴ� ��
        {
            StopAllCoroutines();// �ڷ�ƾ ���� ���� ������ �����ϰ� �ִ� �ڷ�ƾ���� �ߴܽ��� �����ذ� ����ġ�� ��ġ�� ��Ȯ�ϰ� �ٽ� �ٲ�� �ֵ��� ����
            StartCoroutine(FineSightDeActivateCoroutine());
        }

    }

    // ������ Ȱ��ȭ
    IEnumerator FineSightActivateCoroutine() // ������ �ڷ�ƾ
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos) 
        {
            //���� ���� localPosition(���� ��ġ)�� fineSightOriginPos(��������ġ)�� �� ������ �ݺ�
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.3f); // Lerp�� mathf lerp�� ������ ���
            // currentGun.transform.localPosition(������ġ)���� currentGun.fineSightOriginPos(��������ġ)���� 0.2f�� ������ �ݺ�
            yield return null; // 1������ ���
        }
        
    }

    // ������ ��Ȱ��ȭ
    IEnumerator FineSightDeActivateCoroutine() // ������ -> ���� ���������� �ٲ�� �ڷ�ƾ 
    {
        while (currentGun.transform.localPosition != originPos)
        {
            //���� ���� localPosition(���� ��ġ)�� fineSightOriginPos(��������ġ)�� �� ������ �ݺ�
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.3f); // Lerp�� mathf lerp�� ������ ���
            // currentGun.transform.localPosition(������ġ)���� currentGun.fineSightOriginPos(��������ġ)���� 0.2f�� ������ �ݺ�
            yield return null; // 1������ ���
        }
    }

    // �ݵ� �ڷ�ƾ
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z); // ������ �� ������, �Ϲ� �߻� ���� �ִ� �ݵ�
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); // ������ ���� ���� �ִ� �ݵ�

        if(!isFineSightMode) // ������ ���°� �ƴҰ��
        {

            currentGun.transform.localPosition = originPos; // �ݵ��� ��ø�Ǿ� �ݵ��� ���¿��� �ݵ��� ��ġ�� ���� �ʵ��� ó�� ������ �� ���� ��ġ�� �� ��ġ�� �ʱ�ȭ

            // �ݵ� ����
            while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            // �� ��ġ
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else // ������ ������ ��� 
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;
            // �ݵ� ����
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // �� ��ġ
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
