using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image projectileCooldownBar;
    [SerializeField] private Image abilityCooldownBar;
    [SerializeField] private Canvas playerCanvas;
    [SerializeField] private Transform healthbarTransform;

    private TargetPlayer playerController;
    private Shooter shooter;
    private Caster caster;
    private Camera mainCamera;

    private void Awake()
    {
        playerController = GetComponent<TargetPlayer>();
        shooter = GetComponent<Shooter>();
        caster = GetComponent<Caster>();

        mainCamera = Camera.main;
    }

    private void Start()
    {
        playerController.OnHealthChanged += UpdateHealthBar;
        shooter.OnCooldownChanged += UpdateProjectileCooldownBar;
        caster.OnCooldownChanged += UpdateAbilityCooldownBar;

        if (!IsOwner)
        {
            playerCanvas.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        RotateHealthUIToFaceCamera();
    }

    private void RotateHealthUIToFaceCamera()
    {
        healthbarTransform.LookAt(healthbarTransform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }

    private void UpdateProjectileCooldownBar(object sender, float e)
    {
        projectileCooldownBar.fillAmount = e / shooter.GetMaxCooldown();
    }

    private void UpdateAbilityCooldownBar(object sender, float e)
    {
        abilityCooldownBar.fillAmount = e / caster.GetMaxCooldown();
    }

    private void UpdateHealthBar(object sender, byte e)
    {
        UpdateHealthBarRpc(e);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateHealthBarRpc(byte e)
    {
        healthBar.fillAmount = (float)e / (float)playerController.GetMaxHealth();
    }

    #region Testing

    public Image normalBulletButton;
    public Image boomerangBulletButton;
    public Color selectionColor;

    [Rpc(SendTo.ClientsAndHost)]
    public void SelectNormalProjectileRpc()
    {
        GetComponent<Shooter>().SelectProjectile(ProjectileType.Normal);
        normalBulletButton.color = selectionColor;
        boomerangBulletButton.color = Color.white;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SelectBoomerangProjectileRpc()
    {
        GetComponent<Shooter>().SelectProjectile(ProjectileType.Boomerang);
        boomerangBulletButton.color = selectionColor;
        normalBulletButton.color = Color.white;
    }

    #endregion
}
