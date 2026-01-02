using System;
using UnityEngine;

public class BotAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Bot botController;
    [SerializeField] private Shooter shooter;
    [SerializeField] private Caster caster;

    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int shootingHash = Animator.StringToHash("Shoot");

    private void Start()
    {
        botController.OnBotMove += HandleMovement;
        shooter.OnShoot += HandleShooting;
    }

    private void OnDestroy()
    {
        botController.OnBotMove -= HandleMovement;
        shooter.OnShoot -= HandleShooting;
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
