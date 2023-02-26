using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string attackAnimation;
    [SerializeField] string parryAnimation;

    private void OnEnable()
    {
        PlayerController.onAttack += Attack;
        PlayerController.onParry += Parry;
    }
    private void OnDisable()
    {
        PlayerController.onAttack -= Attack;
        PlayerController.onParry -= Parry;
    }

    void Attack()
    {
        animator.Play(attackAnimation);
    }
    void Parry()
    {
        animator.Play(parryAnimation);
    }
}
