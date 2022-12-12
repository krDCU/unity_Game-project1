using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{

    [SerializeField] private Animator chAnimator;

    // ũ�ν���� ���¿� ���� ���� ��Ȯ��
    private float gunAccuracy;

    // ũ�ν���� ��Ȱ��ȭ�� ���� �θ� ��ü
    [SerializeField] private GameObject go_CrossHairHUD;
    [SerializeField] private GunController theGunController;

    
    public void WalkingAnimation(bool _flag)
    {
        WeaponManager.currentWeaponAnim.SetBool("Walk", _flag);
        chAnimator.SetBool("Walking", _flag);
    }

    public void RunningAnimation(bool _flag)
    {
        WeaponManager.currentWeaponAnim.SetBool("Run", _flag);
        chAnimator.SetBool("Running", _flag);
    }

    public void JumpAnimation(bool _flag)
    {
        chAnimator.SetBool("Running", _flag);
    }

    public void CrouchingAnimation(bool _flag)
    {
        chAnimator.SetBool("Crouching", _flag);
    }

    public void FineSightAnmation(bool _flag)
    {
        chAnimator.SetBool("FineSight", _flag);
    }


    public void FireAnimaiton()
    {
        if (chAnimator.GetBool("Walking"))
            chAnimator.SetTrigger("Walk_Fire");
        else if (chAnimator.GetBool("Crouching"))
            chAnimator.SetTrigger("Crouch_Fire");
        else
            chAnimator.SetTrigger("Idle_Fire");

    }

    public float GetAccuracy()
    {
        if (chAnimator.GetBool("Walking"))
            gunAccuracy = 0.06f;
        else if (chAnimator.GetBool("Crouching"))
            gunAccuracy = 0.015f;
        else if (theGunController.GetFineSightMode())
            gunAccuracy = 0.001f;
        else
            gunAccuracy = 0.035f;

        return gunAccuracy;

     }

}
