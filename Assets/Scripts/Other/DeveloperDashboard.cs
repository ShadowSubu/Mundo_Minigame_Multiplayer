using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DeveloperDashboard : NetworkBehaviour
{
    public static DeveloperDashboard Instance { get; private set; }

    [SerializeField] private bool overrideValues = true;
    [SerializeField] private GameObject dashboardUI;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private Button confirmButton;
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

    // Network Variables for Player Character
    private NetworkVariable<float> netPlayerMovementSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> netPlayerChannelTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Bullet
    private NetworkVariable<byte> netBulletDamage = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBulletProjectileSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBulletCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBulletMaxDistance = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Boomerang
    private NetworkVariable<byte> netBoomerangDamage = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBoomerangProjectileSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBoomerangMaxCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBoomerangMaxDistance = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBoomerangReturnMaxDistance = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBoomerangCooldownReduction = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Blink
    private NetworkVariable<float> netBlinkCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBlinkRadius = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Fakeshot
    private NetworkVariable<float> netFakeshotCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Parry
    private NetworkVariable<float> netParryCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netParryDuration = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    private void OnEnable()
    {
        confirmButton.onClick.AddListener(ConfirmValues);
        SetupInputFieldListeners();
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(ConfirmValues);
        RemoveInputFieldListeners();
    }

    private void SetupInputFieldListeners()
    {
        if (playerMovementSpeedInput != null)
            playerMovementSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.PlayerMovementSpeed, value));
        if (playerChannelTimeInput != null)
            playerChannelTimeInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.PlayerChannelTime, value));

        if (bulletDamageInput != null)
            bulletDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletDamage, value));
        if (bulletProjectileSpeedInput != null)
            bulletProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletProjectileSpeed, value));
        if (bulletCooldownInput != null)
            bulletCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletCooldown, value));
        if (bulletMaxDistanceInput != null)
            bulletMaxDistanceInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletMaxDistance, value));

        if (boomerangDamageInput != null)
            boomerangDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangDamage, value));
        if (boomerangProjectileSpeedInput != null)
            boomerangProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangProjectileSpeed, value));
        if (boomerangMaxCooldownInput != null)
            boomerangMaxCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangMaxCooldown, value));
        if (boomerangMaxDistanceInput != null)
            boomerangMaxDistanceInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangMaxDistance, value));
        if (boomerangReturnMaxDistanceInput != null)
            boomerangReturnMaxDistanceInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangReturnMaxDistance, value));
        if (boomerangCooldownReductionInput != null)
            boomerangCooldownReductionInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BoomerangCooldownReduction, value));

        if (blinkCooldownInput != null)
            blinkCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BlinkCooldown, value));
        if (blinkRadiusInput != null)
            blinkRadiusInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BlinkRadius, value));

        if (fakeshotCooldownInput != null)
            fakeshotCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.FakeshotCooldown, value));

        if (parryCooldownInput != null)
            parryCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ParryCooldown, value));
        if (parryDurationInput != null)
            parryDurationInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ParryDuration, value));
    }

    private void RemoveInputFieldListeners()
    {
        if (playerMovementSpeedInput != null)
            playerMovementSpeedInput.onValueChanged.RemoveAllListeners();
        if (playerChannelTimeInput != null)
            playerChannelTimeInput.onValueChanged.RemoveAllListeners();

        if (bulletDamageInput != null)
            bulletDamageInput.onValueChanged.RemoveAllListeners();
        if (bulletProjectileSpeedInput != null)
            bulletProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (bulletCooldownInput != null)
            bulletCooldownInput.onValueChanged.RemoveAllListeners();
        if (bulletMaxDistanceInput != null)
            bulletMaxDistanceInput.onValueChanged.RemoveAllListeners();

        if (boomerangDamageInput != null)
            boomerangDamageInput.onValueChanged.RemoveAllListeners();
        if (boomerangProjectileSpeedInput != null)
            boomerangProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (boomerangMaxCooldownInput != null)
            boomerangMaxCooldownInput.onValueChanged.RemoveAllListeners();
        if (boomerangMaxDistanceInput != null)
            boomerangMaxDistanceInput.onValueChanged.RemoveAllListeners();
        if (boomerangReturnMaxDistanceInput != null)
            boomerangReturnMaxDistanceInput.onValueChanged.RemoveAllListeners();
        if (boomerangCooldownReductionInput != null)
            boomerangCooldownReductionInput.onValueChanged.RemoveAllListeners();

        if (blinkCooldownInput != null)
            blinkCooldownInput.onValueChanged.RemoveAllListeners();
        if (blinkRadiusInput != null)
            blinkRadiusInput.onValueChanged.RemoveAllListeners();

        if (fakeshotCooldownInput != null)
            fakeshotCooldownInput.onValueChanged.RemoveAllListeners();

        if (parryCooldownInput != null)
            parryCooldownInput.onValueChanged.RemoveAllListeners();
        if (parryDurationInput != null)
            parryDurationInput.onValueChanged.RemoveAllListeners();
    }

    private enum InputFieldType
    {
        PlayerMovementSpeed,
        PlayerChannelTime,
        BulletDamage,
        BulletProjectileSpeed,
        BulletCooldown,
        BulletMaxDistance,
        BoomerangDamage,
        BoomerangProjectileSpeed,
        BoomerangMaxCooldown,
        BoomerangMaxDistance,
        BoomerangReturnMaxDistance,
        BoomerangCooldownReduction,
        BlinkCooldown,
        BlinkRadius,
        FakeshotCooldown,
        ParryCooldown,
        ParryDuration
    }

    private void OnInputFieldChanged(InputFieldType fieldType, string value)
    {
        if (!IsHost)
            return;

        UpdateNetworkVariableServerRpc(fieldType, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNetworkVariableServerRpc(InputFieldType fieldType, string value)
    {
        switch (fieldType)
        {
            case InputFieldType.PlayerMovementSpeed:
                if (float.TryParse(value, out float movSpeed))
                    netPlayerMovementSpeed.Value = movSpeed;
                break;
            case InputFieldType.PlayerChannelTime:
                if (int.TryParse(value, out int channelTime))
                    netPlayerChannelTime.Value = channelTime;
                break;
            case InputFieldType.BulletDamage:
                if (byte.TryParse(value, out byte bulletDmg))
                    netBulletDamage.Value = bulletDmg;
                break;
            case InputFieldType.BulletProjectileSpeed:
                if (float.TryParse(value, out float bulletSpeed))
                    netBulletProjectileSpeed.Value = bulletSpeed;
                break;
            case InputFieldType.BulletCooldown:
                if (float.TryParse(value, out float bulletCd))
                    netBulletCooldown.Value = bulletCd;
                break;
            case InputFieldType.BulletMaxDistance:
                if (float.TryParse(value, out float bulletDist))
                    netBulletMaxDistance.Value = bulletDist;
                break;
            case InputFieldType.BoomerangDamage:
                if (byte.TryParse(value, out byte boomDmg))
                    netBoomerangDamage.Value = boomDmg;
                break;
            case InputFieldType.BoomerangProjectileSpeed:
                if (float.TryParse(value, out float boomSpeed))
                    netBoomerangProjectileSpeed.Value = boomSpeed;
                break;
            case InputFieldType.BoomerangMaxCooldown:
                if (float.TryParse(value, out float boomCd))
                    netBoomerangMaxCooldown.Value = boomCd;
                break;
            case InputFieldType.BoomerangMaxDistance:
                if (float.TryParse(value, out float boomDist))
                    netBoomerangMaxDistance.Value = boomDist;
                break;
            case InputFieldType.BoomerangReturnMaxDistance:
                if (float.TryParse(value, out float boomRetDist))
                    netBoomerangReturnMaxDistance.Value = boomRetDist;
                break;
            case InputFieldType.BoomerangCooldownReduction:
                if (float.TryParse(value, out float boomCdRed))
                    netBoomerangCooldownReduction.Value = boomCdRed;
                break;
            case InputFieldType.BlinkCooldown:
                if (float.TryParse(value, out float blinkCd))
                    netBlinkCooldown.Value = blinkCd;
                break;
            case InputFieldType.BlinkRadius:
                if (float.TryParse(value, out float blinkRad))
                    netBlinkRadius.Value = blinkRad;
                break;
            case InputFieldType.FakeshotCooldown:
                if (float.TryParse(value, out float fakeCd))
                    netFakeshotCooldown.Value = fakeCd;
                break;
            case InputFieldType.ParryCooldown:
                if (float.TryParse(value, out float parryCd))
                    netParryCooldown.Value = parryCd;
                break;
            case InputFieldType.ParryDuration:
                if (float.TryParse(value, out float parryDur))
                    netParryDuration.Value = parryDur;
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost)
            this.enabled = false;

        // Subscribe to network variable changes on clients
        netPlayerMovementSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(playerMovementSpeedInput, newVal.ToString("F2"));
        netPlayerChannelTime.OnValueChanged += (oldVal, newVal) => UpdateInputField(playerChannelTimeInput, newVal.ToString());

        netBulletDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletDamageInput, newVal.ToString());
        netBulletProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletProjectileSpeedInput, newVal.ToString("F2"));
        netBulletCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletCooldownInput, newVal.ToString("F2"));
        netBulletMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletMaxDistanceInput, newVal.ToString("F2"));

        netBoomerangDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangDamageInput, newVal.ToString());
        netBoomerangProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangProjectileSpeedInput, newVal.ToString("F2"));
        netBoomerangMaxCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangMaxCooldownInput, newVal.ToString("F2"));
        netBoomerangMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangMaxDistanceInput, newVal.ToString("F2"));
        netBoomerangReturnMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangReturnMaxDistanceInput, newVal.ToString("F2"));
        netBoomerangCooldownReduction.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangCooldownReductionInput, newVal.ToString("F2"));

        netBlinkCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(blinkCooldownInput, newVal.ToString("F2"));
        netBlinkRadius.OnValueChanged += (oldVal, newVal) => UpdateInputField(blinkRadiusInput, newVal.ToString("F2"));

        netFakeshotCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(fakeshotCooldownInput, newVal.ToString("F2"));

        netParryCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(parryCooldownInput, newVal.ToString("F2"));
        netParryDuration.OnValueChanged += (oldVal, newVal) => UpdateInputField(parryDurationInput, newVal.ToString("F2"));
    }

    private void UpdateInputField(TMP_InputField inputField, string value)
    {
        if (inputField != null && !IsHost)
        {
            inputField.text = value;
        }
    }

    private void ConfirmValues()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetPlayerMovementSpeed(GetPlayerMovementSpeed());
            players[i].SetPlayerChannelDuration(GetPlayerChannelTime());
        }

        ToggleDashboard();
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
                bulletDamageInput.text = bulletPrefab.ProjectileDamage.ToString();
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
                boomerangDamageInput.text = boomerangPrefab.ProjectileDamage.ToString();
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

    // Player Character Getters - now use network variables
    public float GetPlayerMovementSpeed() => netPlayerMovementSpeed.Value;
    public int GetPlayerChannelTime() => netPlayerChannelTime.Value;

    // Bullet Getters - now use network variables
    public byte GetBulletDamage() => netBulletDamage.Value;
    public float GetBulletProjectileSpeed() => netBulletProjectileSpeed.Value;
    public float GetBulletCooldown() => netBulletCooldown.Value;
    public float GetBulletMaxDistance() => netBulletMaxDistance.Value;

    // Boomerang Getters - now use network variables
    public byte GetBoomerangDamage() => netBoomerangDamage.Value;
    public float GetBoomerangProjectileSpeed() => netBoomerangProjectileSpeed.Value;
    public float GetBoomerangMaxCooldown() => netBoomerangMaxCooldown.Value;
    public float GetBoomerangMaxDistance() => netBoomerangMaxDistance.Value;
    public float GetBoomerangReturnMaxDistance() => netBoomerangReturnMaxDistance.Value;
    public float GetBoomerangCooldownReduction() => netBoomerangCooldownReduction.Value;

    // Blink Getters - now use network variables
    public float GetBlinkCooldown() => netBlinkCooldown.Value;
    public float GetBlinkRadius() => netBlinkRadius.Value;

    // Fakeshot Getters - now use network variables
    public float GetFakeshotCooldown() => netFakeshotCooldown.Value;

    // Parry Getters - now use network variables
    public float GetParryCooldown() => netParryCooldown.Value;
    public float GetParryDuration() => netParryDuration.Value;

    public bool OverrideValues => overrideValues;

    public TMP_InputField.OnChangeEvent OnVlaueChanged { get; private set; }
}