using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image projectileCooldownBar;
    [SerializeField] private Image abilityCooldownBar;
    [SerializeField] private TextMeshProUGUI projectileCooldownText;
    [SerializeField] private TextMeshProUGUI abilityCooldownText;
    [SerializeField] private Image projectileIconImage;
    [SerializeField] private Image abilityIconImage;
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
        GameManager.Instance.OnAllPlayersSpawned += GameManager_OnAllPlayersSpawned;

        if (!IsOwner)
        {
            TogglePlayerUICanvas(false);
        }

        projectileCooldownBar.fillAmount = 0f;
        projectileCooldownText.text = "";
        abilityCooldownBar.fillAmount = 0f;
        abilityCooldownText.text = "";
    }

    private void GameManager_OnAllPlayersSpawned(object sender, System.Collections.Generic.List<PlayerController> e)
    {
        abilityIconImage.sprite = caster.SelectedAbility.AbilityIcon;
        projectileIconImage.sprite = shooter.SelectedProjectile.ProjectileIcon;
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
        //Debug.Log($"Updating Projectile Cooldown Bar: Current Cooldown = {e}, Max Cooldown = {shooter.GetMaxCooldown()}");
        projectileCooldownBar.fillAmount = e / shooter.GetMaxCooldown();
        if (e == shooter.GetMaxCooldown())
        {
            projectileCooldownText.text = "";
        }
        else
        {
            projectileCooldownText.text = Mathf.FloorToInt(e).ToString();
        }
    }

    private void UpdateAbilityCooldownBar(object sender, float e)
    {
        abilityCooldownBar.fillAmount = e / caster.GetMaxCooldown();
        if(e == caster.GetMaxCooldown())
        {
            abilityCooldownText.text = "";
        }
        else
        {
            abilityCooldownText.text = Mathf.FloorToInt(e).ToString();
        }
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

    public void TogglePlayerUICanvas(bool value)
    {
        if (IsOwner)
        {
            playerCanvas.gameObject.SetActive(value);
        }
    }

    #region Testing

    //public Image normalBulletButton;
    //public Image boomerangBulletButton;
    //public Color selectionColor;

    //[Rpc(SendTo.ClientsAndHost)]
    //public void SelectNormalProjectileRpc()
    //{
    //    GetComponent<Shooter>().SelectProjectile(ProjectileType.Normal);
    //    normalBulletButton.color = selectionColor;
    //    boomerangBulletButton.color = Color.white;
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //public void SelectBoomerangProjectileRpc()
    //{
    //    GetComponent<Shooter>().SelectProjectile(ProjectileType.Boomerang);
    //    boomerangBulletButton.color = selectionColor;
    //    normalBulletButton.color = Color.white;
    //}

    #endregion
}
