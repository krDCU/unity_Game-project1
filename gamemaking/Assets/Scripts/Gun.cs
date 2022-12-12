using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    //�� ����
    public string gunName; // �� �̸�
    public float range; // ���� �����Ÿ�
    public float accuracy; // ���� ��Ȯ�� 
    public float fireRate; // ����ӵ�(��ġ�� �������� ����ӵ��� ����)
    public float reloadTime; // ������ �ӵ�
    public int damage; // ���� ������
    public float retroActionForce; // �� �ݵ� ����
    public float retroActionFineSightForce; // �����ؽ� �ݵ� ����

    public Vector3 fineSightOriginPos; // ������ �� ��ġ

    public Animator gunAnimation; // �� �ִϸ��̼�

    public ParticleSystem muzzleFlash; // �� �߻� ����Ʈ

    public AudioClip fire_Sound; // �� �߻� �Ҹ�;

    //�Ѿ� ����
    public int reloadBulletCount; // �Ѿ� ������ ����
    public int currentBulletCount; // ���� ź������ �����ִ� �Ѿ��� ����
    public int maxBulletCount; // �ִ� ���� ���� �Ѿ� ����
    public int carryBulletCount; // ���� �����ϰ� �ִ� �Ѿ� ����

}
