using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Shooter shooter;
    [SerializeField] private Caster caster;

    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int shootingHash = Animator.StringToHash("Shoot");

    private void Start()
    {
        playerController.OnPlayerMove += HandleMovement;
        shooter.OnShoot += HandleShooting;
    }

    private void OnDestroy()
    {
        playerController.OnPlayerMove -= HandleMovement;
    }

    private void HandleMovement(object sender, float sqrMagnitude)
    {
        if (sqrMagnitude > 0)
        {
            animator.SetBool(isMovingHash, true);
        }
        else
        {
            animator.SetBool(isMovingHash, false);
        }
    }

    private void HandleShooting(object sender, EventArgs e)
    {
        animator.SetTrigger(shootingHash);
    }
}
