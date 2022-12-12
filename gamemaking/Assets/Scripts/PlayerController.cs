using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //���ǵ� ���� ����
    [SerializeField] private float walkSpeed; //�÷��̾� ������ �ӵ�
    [SerializeField] private float runSpeed; // �÷��̾� �ٱ� �ӵ�
    // walkSpeed�� runSpeed 2���� ������ ������ �ִµ� applySpeed�� ������ ������ ������ applySpped�� �����ؼ� ����
    private float applySpeed;

    //�ɱ� ���ǵ�
    [SerializeField] private float crouchSpeed;

    //���� Speed�� �ƴ� ���������� ������ �󸶸�ŭ�� ������ �ۿ����� ����
    [SerializeField] private float jumpForce;


    // �ȱ� or �ٱ� ���º���
    public bool isRun = false;
    // �÷��̾ ���� ��ġ�ߴ��� �ƴ��� ���º���
    private bool isGround = true;
    //�ɾҴ��� �� �ɾҴ��� ���º���
    private bool isCrouch = false;
    private bool isWalk = false;

    // ������ üũ ����
    private Vector3 lastPos;

    //�ɾ����� �󸶳� ������ �����ϴ� ����
    [SerializeField] private float crouchPosY;
    private float originPosY; // ���� ���¿��� ���ƿ� ���� ��ġ
    private float applyCrouchPosY; // crouchPosY�� originPosY�� ���� �������� ���� / applySpeed���� ����

    //ĸ�� �ݶ��̴��� �̿��� ���鿡 ������ �ִ��� Ȯ��
    private CapsuleCollider capsuleCollider;

    // ī�޶� �ΰ���
    [SerializeField] private float lookSensitivity;

    // ī�޶� ȸ������ �Ѱ�
    [SerializeField] private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //ī�޶� ���� ����

    //�ʿ��� ������Ʈ
    [SerializeField] Camera theCamera;
    private Rigidbody myRigid; //�÷��̾� ������Ģ

    private GunController theGunController;
    private Crosshair theCrossHair;


    void Start()
    {
        // ĸ���ݶ��̴� , ������ٵ� ������Ʈ ȣ�� / applySpeed�� �ʱⰪ�� walkSpeed�� ����
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrossHair = FindObjectOfType <Crosshair>();

        // �ʱ�ȭ
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y; //�÷��̾� ī�޶��� ������������ originPosY�� ����
        applyCrouchPosY = originPosY; // �⺻ ���ִ� ����
    }

    void FixedUpdate()
    {
        MoveCheck();
    }

    void Update()
    {
        IsGround(); // �÷��̾��� ���� Ȯ�� �Լ�
        TryJump(); // ���� ������ ����
        TryRun(); // �ٱ� ������ ����
        TryCrouch(); // �ɱ� ������ ����
        Move(); // ������ ����
        CameraRotation(); // ī�޶� ���� ������ ����
        CharacterRotation(); // ī�޶�(ĳ����) �¿� ������ ����
        
    }


    private void TryCrouch()
    {
        //LeftContorl�� ������ ���� �ɱ�
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Crouch();
        }
        //LeftContorl�� �Է��� ������ �ɱ� ���
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            CrouchCancel();
        }

    }

    private void Crouch()
    {
        //LeftControl�� ������� isCrouch�� true��ȯ
        //applySpeed�� crouchSpeed�� ��ȯ
        //applyCrouchPosY�� crouchPosY(���� ������ ��ġ)�� ��ȯ
        //ī�޶��� ������������ Vector3�� ���Ӱ� ������ x�� z�� ��ġ�� �״�� ���ΰ� crouchPosY�� ���� �� �ִ� applyCrouchPosY�� ȣ��
        isCrouch = true;
        theCrossHair.CrouchingAnimation(isCrouch);
        applySpeed = crouchSpeed;
        applyCrouchPosY = crouchPosY;
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }

    private void CrouchCancel()
    {
        //LeftControl�� �Է��� ���� ��� isCrouch�� false ��ȯ
        //applySpeed�� walkSpeed�� ��ȯ
        //applyCrouchPosY�� originPosY(���ִ� ������ ��ġ)�� ��ȯ
        //ī�޶��� ������������ Vector3�� ���Ӱ� ������ x�� z�� ��ġ�� �״�� ���ΰ� originPosY�� ���� �� �ִ� applyCrouchPosY�� ȣ��
        isCrouch = false;
        theCrossHair.CrouchingAnimation(isCrouch);
        applySpeed = walkSpeed;
        applyCrouchPosY = originPosY;
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }

    
    IEnumerator CrouchCoroutine()
    {
        //�ɾ����� �ε巯�� ī�޶� �������� ����
        //count�� �̿��� playerCamera�� ��������� 0.9999�� y���� ������� 0�� ����� ������ ���ѿ� ������ ����Ǳ� ������
        //��Ȯ�ϰ� 1�� 0���� �����ϱ� ���� count�� �̿��� 15�� �̻� �ݺ��Ǹ� break�� �̿��ϵ���
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

    //private void Crouch()
    //{
    //    //isCoruch�� false�� ��� True / True�� ��� False��
    //    isCrouch = !isCrouch;
    //    if (isCrouch)
    //    {
    //        applySpeed = crouchSpeed;
    //        applyCrouchPosY = crouchPosY;
    //    }
    //    else
    //    {
    //        applySpeed = walkSpeed;
    //        applyCrouchPosY = originPosY;
    //    }
    //    //ī�޶��� ���������� ��ġ�� Vector3�� ���Ӱ� �����ؼ� x�� z���� �״�� ���ΰ� y�� ���� applyCrouchPosY�� ȣ��
    //    theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    //}

    private void IsGround()
    {
        //IsGorund �Լ��� Player�� ���鿡 ��ġ�� �ִ��� Ȯ�� =>Raycast�� �̿��� isGorund�� true / false ���� Ȯ��
        //transform�� up / right�� ���� => down / left�� ��� -up , -right�� �ݴ� ���� ����
        //Vector3.down���� -transform.up�ε� ���� => ������ -transform.up���� ����� ��� player�� �������� ���������� �ϴ��� ���� Raycast�� �߻�
        //�̸� �����ϱ� ���� ������ ��ǥ Vector3�� �̿�
        //capsuleCollider.bounds.extents.y�� capsuleCollider.bounds(ĸ���ݶ��̴��� ����), extents(�ش� ������ 1/2������), y���� 1/2 ���� 0.1f��ŭ�� �������� ���
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrossHair.JumpAnimation(!isGround);
    }
    

    private void TryJump()
    {
        //Jump Ű �Է� ���� �� �Լ� ȣ��
        //GetKeyDown�� �ѹ� ������ �� / GetKeyUp�� Ű �Է��� ������ �� ��������(����������)
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            Jump();
        }
    }

    private void Jump()
    {
        //���� ���� �Լ�
        //���������� velocity�� transform.up�� jumpForce ��ŭ ���� �����־� ��ġ�� ����(����)
        myRigid.velocity = transform.up * jumpForce;

    }

    private void TryRun()
    {
        // ���� ����Ʈ�� Ű�Է��� ������ Running �Լ�
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        // ���� ����Ʈ�� Ű�Է��� ���������� RunningCancel �Լ�
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }


    private void Running()
    {
        if(isCrouch)
            Crouch();

        theGunController.CancleFineSight();


        //�ٴ� �����̱� ������ isRun�� True
        isRun = true;
        //applySpeed�� walkSpeed�� ���� runSpeed�� �ٲپ� Move �Լ����� �ٴ� �ӵ��� ����
        applySpeed = runSpeed;
        theCrossHair.RunningAnimation(isRun);
    }

    private void RunningCancel()
    {
        //�ٴ� ���¸� ��� �߱� ������ isRun�� �ٽ� False
        isRun=false;
        //applySpped�� ���� runSpped���� �ٽ� walkSpeed�� ����
        applySpeed = walkSpeed;
        theCrossHair.RunningAnimation(isRun);
    }

    private void Move()
    {
        //Ű���� �¿�/AD �Է½� 1 �Ǵ� -1�� ���ϵǸ鼭 _moveDirX�� ���� ��(�¿�)
        float _moveDirX = Input.GetAxisRaw("Horizontal");

        //Ű���� ���Ʒ�/WS �Է½� 1 �Ǵ� -1�� ���ϵǸ鼭 _moveDirZ�� ���� ��(�յ�)
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        //Vector3.right �⺻�� (1,0,0) �� _moveDirX�� ��(0 �Ǵ� -1)�� �����־� �¿� �̵��� ����
        Vector3 _moveHorizontal = transform.right * _moveDirX;

        //Vector3.forward �⺻�� (0,0,1) �� _moveDirZ�� ��(0 �Ǵ� -1)�� �����־� �յ� �̵��� ����
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        //_moveHorizontal + _moveVertical�� �����Ұ�� (1,0,0)�� (0,0,1)�� ������ (1,0,1) = 2�� ����
        // ���⼭ normalized�� ����� 2�� �ƴ� (0.5, 0, 0.5)�� ��Ÿ�� 1�� �ǵ��� ����
        // �ﰢ�Լ� (1,0,1)�� (0.5, 0, 0.5)�� ���ư��� ������ ������ ����Ƽ �������Ļ� (0.5, 0, 0.5)�� �� ����
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        //myRigid�� MovePosition �Լ��� �̿��� _velocity��ŭ �̵��� ����
        //Time.deltaTime�� �̿��� �����̵� �ϴµ��� �̵��� �ƴ� �ڿ������� �̵����� ����
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void MoveCheck()
    {
        
        if (!isRun && !isCrouch && isGround) // isRun�� false�� ���� üũ / �ٰ� ���� ���� üũ X
        {
            // lastPos(�������� ��ġ)�� transform.position(���� ��ġ) ������ �Ÿ��� ��ȯ�ϰ� 0.01f���� ũ�� �Ȱ� ������ ���� ����
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f)
                isWalk = true; 
            else
                isWalk = false;

            theCrossHair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }

    private void CameraRotation()
    {
        //���� ī�޶� ȸ�� ����

        //���콺�� ���Ʒ� ���� ��ȭ / ����Ƽ���� X�� ����
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        
        //���콺 �ΰ����� ���� õõ�� �����̵��� ����
        float _cameraRotaitonX = _xRotation * lookSensitivity;
       
        //currentCameraRotationX�� _cameraRotaitonX;�� ���ؼ� ȸ���� ����
        currentCameraRotationX -= _cameraRotaitonX;
        
        //Clamp�� �̿��� currentCameraRotationX�� ���� cameraRotationLimit�� ����� ������ �Ѿ�� �ʵ��� ����
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        
    }

    private void CharacterRotation()
    {
        //�¿� ĳ���� ȸ�� / ����Ƽ���� Y�� �¿�

        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        //myRigid.rotation�� ���ʹϾ����� ��ȯ�� _characterRotationY�� ���� �¿� ȸ������
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

    }
}
