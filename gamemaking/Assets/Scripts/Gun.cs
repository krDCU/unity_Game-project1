using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    //총 정보
    public string gunName; // 총 이름
    public float range; // 총의 사정거리
    public float accuracy; // 총의 정확도 
    public float fireRate; // 연사속도(수치가 높을수록 연사속도가 느림)
    public float reloadTime; // 재장전 속도
    public int damage; // 총의 데미지
    public float retroActionForce; // 총 반동 세기
    public float retroActionFineSightForce; // 정조준시 반동 세기

    public Vector3 fineSightOriginPos; // 정조준 시 위치

    public Animator gunAnimation; // 총 애니메이션

    public ParticleSystem muzzleFlash; // 총 발사 이펙트

    public AudioClip fire_Sound; // 총 발사 소리;

    //총알 정보
    public int reloadBulletCount; // 총알 재장전 개수
    public int currentBulletCount; // 현재 탄알집에 남아있는 총알의 개수
    public int maxBulletCount; // 최대 소유 가능 총알 개수
    public int carryBulletCount; // 현재 소유하고 있는 총알 개수

}
