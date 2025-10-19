using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DeveloperDashboard : NetworkBehaviour
{
    public static DeveloperDashboard Instance { get; private set; }

    [SerializeField] private GameObject dashboardUI;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    private bool dashboardOpen = false;

    [Header("Player Character Section")]
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private TMP_InputField playerMovementSpeedInput;
    [SerializeField] private TMP_InputField playerChannelTimeInput;

    [Header("Projectile Section - Bullet")]
    [SerializeField] private ProjectileNormal bulletPrefab;
    [SerializeField] private TMP_InputField bulletDamageInput;
    [SerializeField] private TMP_InputField bulletProjectileSpeedInput;
    [SerializeField] private TMP_InputField bulletCooldownInput;
    [SerializeField] private TMP_InputField bulletMaxDistanceInput;

    [Header("Projectile Section - Boomerang")]
    [SerializeField] private ProjectileBoomerang boomerangPrefab;
    [SerializeField] private TMP_InputField boomerangDamageInput;
    [SerializeField] private TMP_InputField boomerangProjectileSpeedInput;
    [SerializeField] private TMP_InputField boomerangMaxCooldownInput;
    [SerializeField] private TMP_InputField boomerangMaxDistanceInput;
    [SerializeField] private TMP_InputField boomerangReturnMaxDistanceInput;
    [SerializeField] private TMP_InputField boomerangCooldownReductionInput;

    [Header("Ability Section - Blink")]
    [SerializeField] private AbilityBlink blinkPrefab;
    [SerializeField] private TMP_InputField blinkCooldownInput;
    [SerializeField] private TMP_InputField blinkRadiusInput;

    [Header("Ability Section - Fakeshot")]
    [SerializeField] private AbilityFakeShot fakeshotPrefab;
    [SerializeField] private TMP_InputField fakeshotCooldownInput;

    [Header("Ability Section - Parry")]
    [SerializeField] private AbilityParry parryPrefab;
    [SerializeField] private TMP_InputField parryCooldownInput;
    [SerializeField] private TMP_InputField parryDurationInput;

    private NetworkVariable<bool> isPaused = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Only enable for host
        if (!IsHost)
        {
            this.enabled = false;
            return;
        }

        if (dashboardUI != null)
            dashboardUI.SetActive(false);

        LoadDefaultValues();
    }

    private void LoadDefaultValues()
    {
        if (playerPrefab != null)
        {
            NavMeshAgent navMeshAgent = playerPrefab.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null && playerMovementSpeedInput != null)
                playerMovementSpeedInput.text = navMeshAgent.speed.ToString("F2");
        }

        if (bulletPrefab != null)
        {
            if (bulletDamageInput != null)
                bulletDamageInput.text = bulletPrefab.ProjectileDamage.ToString("F2");
            if (bulletProjectileSpeedInput != null)
                bulletProjectileSpeedInput.text = bulletPrefab.ProjectileSpeed.ToString("F2");
            if (bulletCooldownInput != null)
                bulletCooldownInput.text = bulletPrefab.MaxCooldown.ToString("F2");
            if (bulletMaxDistanceInput != null)
                bulletMaxDistanceInput.text = bulletPrefab.MaxDistance.ToString("F2");
        }

        if (boomerangPrefab != null)
        {
            if (boomerangDamageInput != null)
                boomerangDamageInput.text = boomerangPrefab.ProjectileDamage.ToString("F2");
            if (boomerangProjectileSpeedInput != null)
                boomerangProjectileSpeedInput.text = boomerangPrefab.ProjectileSpeed.ToString("F2");
            if (boomerangMaxCooldownInput != null)
                boomerangMaxCooldownInput.text = boomerangPrefab.MaxCooldown.ToString("F2");
            if (boomerangMaxDistanceInput != null)
                boomerangMaxDistanceInput.text = boomerangPrefab.MaxDistance.ToString("F2");
            if (boomerangReturnMaxDistanceInput != null)
                boomerangReturnMaxDistanceInput.text = boomerangPrefab.ReturnMaxDistance.ToString("F2");
            if (boomerangCooldownReductionInput != null)
                boomerangCooldownReductionInput.text = boomerangPrefab.CooldownReduction.ToString("F2");
        }

        if (blinkPrefab != null)
        {
            if (blinkCooldownInput != null)
                blinkCooldownInput.text = blinkPrefab.MaxCooldown.ToString("F2");
            if (blinkRadiusInput != null)
                blinkRadiusInput.text = blinkPrefab.BlinkRadius.ToString("F2");
        }

        if (fakeshotPrefab != null)
        {
            if (fakeshotCooldownInput != null)
                fakeshotCooldownInput.text = fakeshotPrefab.MaxCooldown.ToString("F2");
        }

        if (parryPrefab != null)
        {
            if (parryCooldownInput != null)
                parryCooldownInput.text = parryPrefab.MaxCooldown.ToString("F2");
            if (parryDurationInput != null)
                parryDurationInput.text = parryPrefab.ParryDuration.ToString("F2");
        }
    }

    private void Update()
    {
        if (!IsHost)
            return;

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDashboard();
        }
    }

    private void ToggleDashboard()
    {
        dashboardOpen = !dashboardOpen;

        if (dashboardUI != null)
            dashboardUI.SetActive(dashboardOpen);

        SetGamePausedServerRpc(dashboardOpen);
    }

    [ServerRpc]
    private void SetGamePausedServerRpc(bool paused)
    {
        isPaused.Value = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost)
            this.enabled = false;
    }
}