using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField] private float walkSpeed; //플레이어 움직임 속도
    [SerializeField] private float runSpeed; // 플레이어 뛰기 속도
    // walkSpeed와 runSpeed 2개의 움직임 변수가 있는데 applySpeed를 구현해 각각의 변수를 applySpped에 대입해서 구현
    private float applySpeed;

    //앉기 스피드
    [SerializeField] private float crouchSpeed;

    //점프 Speed가 아닌 순간적으로 점프를 얼마만큼의 힘으로 작용할지 적용
    [SerializeField] private float jumpForce;


    // 걷기 or 뛰기 상태변수
    public bool isRun = false;
    // 플레이어가 땅에 위치했는지 아닌지 상태변수
    private bool isGround = true;
    //앉았는지 안 앉았는지 상태변수
    private bool isCrouch = false;
    private bool isWalk = false;

    // 움직임 체크 변수
    private Vector3 lastPos;

    //앉았을때 얼마나 앉을지 결정하는 변수
    [SerializeField] private float crouchPosY;
    private float originPosY; // 앉은 상태에서 돌아올 원래 위치
    private float applyCrouchPosY; // crouchPosY와 originPosY의 값을 대입해줄 변수 / applySpeed같은 변수

    //캡슐 콜라이더를 이용해 지면에 접촉해 있는지 확인
    private CapsuleCollider capsuleCollider;

    // 카메라 민감도
    [SerializeField] private float lookSensitivity;

    // 카메라 회전각도 한계
    [SerializeField] private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //카메라가 보는 각도

    //필요한 컴포넌트
    [SerializeField] Camera theCamera;
    private Rigidbody myRigid; //플레이어 물리법칙

    private GunController theGunController;
    private Crosshair theCrossHair;


    void Start()
    {
        // 캡슐콜라이더 , 리지드바디 컴포넌트 호출 / applySpeed의 초기값을 walkSpeed로 설정
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrossHair = FindObjectOfType <Crosshair>();

        // 초기화
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y; //플레이어 카메라의 로컬포지션을 originPosY로 설정
        applyCrouchPosY = originPosY; // 기본 서있는 상태
    }

    void FixedUpdate()
    {
        MoveCheck();
    }

    void Update()
    {
        IsGround(); // 플레이어의 지면 확인 함수
        TryJump(); // 점프 움직임 구현
        TryRun(); // 뛰기 움직임 구현
        TryCrouch(); // 앉기 움직임 구현
        Move(); // 움직임 구현
        CameraRotation(); // 카메라 상하 움직임 구현
        CharacterRotation(); // 카메라(캐릭터) 좌우 움직임 구현
        
    }


    private void TryCrouch()
    {
        //LeftContorl을 누르는 동안 앉기
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Crouch();
        }
        //LeftContorl을 입력을 끝내면 앉기 취소
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            CrouchCancel();
        }

    }

    private void Crouch()
    {
        //LeftControl을 누를경우 isCrouch을 true반환
        //applySpeed를 crouchSpeed로 변환
        //applyCrouchPosY을 crouchPosY(앉은 상태의 위치)로 변환
        //카메라의 로컬포지션을 Vector3을 새롭게 생성해 x와 z의 위치는 그대로 나두고 crouchPosY의 값이 들어가 있는 applyCrouchPosY을 호출
        isCrouch = true;
        theCrossHair.CrouchingAnimation(isCrouch);
        applySpeed = crouchSpeed;
        applyCrouchPosY = crouchPosY;
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }

    private void CrouchCancel()
    {
        //LeftControl의 입력을 끝낼 경우 isCrouch을 false 반환
        //applySpeed를 walkSpeed로 변환
        //applyCrouchPosY을 originPosY(서있는 상태의 위치)로 변환
        //카메라의 로컬포지션을 Vector3을 새롭게 생성해 x와 z의 위치는 그대로 나두고 originPosY의 값이 들어가 있는 applyCrouchPosY을 호출
        isCrouch = false;
        theCrossHair.CrouchingAnimation(isCrouch);
        applySpeed = walkSpeed;
        applyCrouchPosY = originPosY;
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }

    
    IEnumerator CrouchCoroutine()
    {
        //앉았을때 부드러운 카메라 움직임을 구현
        //count를 이용해 playerCamera가 서있을경우 0.9999의 y값과 앉을경우 0의 가까운 값으로 무한에 가깝게 실행되기 때문에
        //정확하게 1과 0으로 구현하기 위해 count를 이용해 15번 이상 반복되면 break를 이용하도록
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
    //    //isCoruch가 false일 경우 True / True일 경우 False로
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
    //    //카메라의 로컬포지션 위치를 Vector3을 새롭게 생성해서 x와 z값은 그대로 나두고 y의 값만 applyCrouchPosY만 호출
    //    theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    //}

    private void IsGround()
    {
        //IsGorund 함수는 Player가 지면에 위치해 있는지 확인 =>Raycast를 이용해 isGorund의 true / false 여부 확인
        //transform은 up / right만 존재 => down / left가 없어서 -up , -right로 반대 방향 적용
        //Vector3.down말고 -transform.up로도 가능 => 하지만 -transform.up으로 사용할 경우 player를 기준으로 뒤집어지면 하늘을 향해 Raycast를 발사
        //이를 방지하기 위해 고정된 좌표 Vector3를 이용
        //capsuleCollider.bounds.extents.y은 capsuleCollider.bounds(캡슐콜라이더의 영역), extents(해당 영역의 1/2사이즈), y값의 1/2 값에 0.1f만큼의 오차범위 계산
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrossHair.JumpAnimation(!isGround);
    }
    

    private void TryJump()
    {
        //Jump 키 입력 했을 때 함수 호출
        //GetKeyDown은 한번 눌렀을 때 / GetKeyUp은 키 입력을 끝내고 안 눌렀을때(떨어졌을때)
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            Jump();
        }
    }

    private void Jump()
    {
        //점프 구현 함수
        //순간적으로 velocity를 transform.up에 jumpForce 만큼 힘을 가해주어 위치를 변경(점프)
        myRigid.velocity = transform.up * jumpForce;

    }

    private void TryRun()
    {
        // 왼쪽 쉬프트의 키입력이 있을시 Running 함수
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        // 왼쪽 쉬프트의 키입력이 떨어졌을때 RunningCancel 함수
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


        //뛰는 상태이기 때문에 isRun을 True
        isRun = true;
        //applySpeed에 walkSpeed의 값을 runSpeed로 바꾸어 Move 함수에서 뛰는 속도로 변경
        applySpeed = runSpeed;
        theCrossHair.RunningAnimation(isRun);
    }

    private void RunningCancel()
    {
        //뛰는 상태를 취소 했기 때문에 isRun을 다시 False
        isRun=false;
        //applySpped의 값을 runSpped에서 다시 walkSpeed로 변경
        applySpeed = walkSpeed;
        theCrossHair.RunningAnimation(isRun);
    }

    private void Move()
    {
        //키보드 좌우/AD 입력시 1 또는 -1이 리턴되면서 _moveDirX에 들어가게 됨(좌우)
        float _moveDirX = Input.GetAxisRaw("Horizontal");

        //키보드 위아래/WS 입력시 1 또는 -1이 리턴되면서 _moveDirZ에 들어가게 됨(앞뒤)
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        //Vector3.right 기본값 (1,0,0) 에 _moveDirX의 값(0 또는 -1)을 곱해주어 좌우 이동을 구현
        Vector3 _moveHorizontal = transform.right * _moveDirX;

        //Vector3.forward 기본값 (0,0,1) 에 _moveDirZ의 값(0 또는 -1)을 곱해주어 앞뒤 이동을 구현
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        //_moveHorizontal + _moveVertical을 실행할경우 (1,0,0)과 (0,0,1)이 더해져 (1,0,1) = 2가 나옴
        // 여기서 normalized를 사용해 2가 아닌 (0.5, 0, 0.5)로 나타내 1이 되도록 구현
        // 삼각함수 (1,0,1)과 (0.5, 0, 0.5)는 나아가는 방향이 같지만 유니티 구동형식상 (0.5, 0, 0.5)를 더 권장
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        //myRigid에 MovePosition 함수를 이용해 _velocity만큼 이동을 구현
        //Time.deltaTime을 이용해 순간이동 하는듯한 이동이 아닌 자연스러운 이동으로 구현
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void MoveCheck()
    {
        
        if (!isRun && !isCrouch && isGround) // isRun이 false일 때만 체크 / 뛰고 있을 때는 체크 X
        {
            // lastPos(전프레임 위치)와 transform.position(현재 위치) 사이의 거리를 반환하고 0.01f보다 크면 걷고 작으면 걷지 않음
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
        //상하 카메라 회전 구현

        //마우스로 위아래 시점 변화 / 유니티에서 X가 상하
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        
        //마우스 민감도를 곱해 천천히 움직이도록 구현
        float _cameraRotaitonX = _xRotation * lookSensitivity;
       
        //currentCameraRotationX에 _cameraRotaitonX;을 더해서 회전을 구현
        currentCameraRotationX -= _cameraRotaitonX;
        
        //Clamp를 이용해 currentCameraRotationX의 값을 cameraRotationLimit의 양수와 음수를 넘어서지 않도록 구현
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        
    }

    private void CharacterRotation()
    {
        //좌우 캐릭터 회전 / 유니티에서 Y가 좌우

        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        //myRigid.rotation과 쿼터니언으로 변환한 _characterRotationY을 곱해 좌우 회전구현
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

    }
}
