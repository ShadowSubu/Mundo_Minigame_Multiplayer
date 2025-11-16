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
    [SerializeField] private GameAreaBehaviour gameAreaBehaviour;
    [SerializeField] private ArenaHeal arenaHeal;
    [SerializeField] private TMP_InputField playerMovementSpeedInput;
    [SerializeField] private TMP_InputField playerChannelTimeInput;
    [SerializeField] private TMP_InputField arenaHealIntervalInput;
    [SerializeField] private TMP_InputField arenaHealAmountInput;

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

    [Header("Projectile Section - Mortar")]
    [SerializeField] private ProjectileMortar mortarPrefab;
    [SerializeField] private TMP_InputField mortarDamageInput;
    [SerializeField] private TMP_InputField mortarProjectileSpeedInput;
    [SerializeField] private TMP_InputField mortarMaxCooldownInput;
    [SerializeField] private TMP_InputField mortarMaxHeightInput;
    [SerializeField] private TMP_InputField mortarExplosionRadiusInput;

    [Header("Projectile Section - Homing")]
    [SerializeField] private ProjectileHoming homingPrefab;
    [SerializeField] private TMP_InputField homingDamageInput;
    [SerializeField] private TMP_InputField homingProjectileSpeedInput;
    [SerializeField] private TMP_InputField homingMaxCooldownInput;
    [SerializeField] private TMP_InputField homingMaxDistanceInput;
    [SerializeField] private TMP_InputField homingTurnSensitivityInput;

    [Header("Projectile Section - Curve")]
    [SerializeField] private ProjectileCurved curvePrefab;
    [SerializeField] private TMP_InputField curveDamageInput;
    [SerializeField] private TMP_InputField curveProjectileSpeedInput;
    [SerializeField] private TMP_InputField curveMaxCooldownInput;
    [SerializeField] private TMP_InputField curveStrengthInput;

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

    [Header("Ability Section - Invisibility")]
    [SerializeField] private AbilityInvisibility invisibilityPrefab;
    [SerializeField] private TMP_InputField invisibilityCooldownInput;
    [SerializeField] private TMP_InputField invisibilityDurationInput;

    [Header("Ability Section - Speed Boost")]
    [SerializeField] private AbilitySpeedBoost speedBoostPrefab;
    [SerializeField] private TMP_InputField speedBoostCooldownInput;
    [SerializeField] private TMP_InputField speedBoostDurationInput;

    private NetworkVariable<bool> isPaused = new NetworkVariable<bool>(false);

    // Network Variables for Player Character
    private NetworkVariable<float> netPlayerMovementSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> netPlayerChannelTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netArenaHealInterval = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<byte> netArenaHealAmount = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    // Network Variables for Mortar
    private NetworkVariable<byte> netMortarDamage = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netMortarProjectileSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netMortarMaxCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netMortarMaxHeight = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netMortarExplosionRadius = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Homing
    private NetworkVariable<byte> netHomingDamage = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netHomingProjectileSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netHomingMaxCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netHomingMaxDistance = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netHomingTurnSensitivity = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Curve
    private NetworkVariable<byte> netCurveDamage = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netCurveProjectileSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netCurveMaxCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netCurveStrength = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Blink
    private NetworkVariable<float> netBlinkCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netBlinkRadius = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Fakeshot
    private NetworkVariable<float> netFakeshotCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Parry
    private NetworkVariable<float> netParryCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netParryDuration = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Invisibility
    private NetworkVariable<float> netInvisibilityCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netInvisibilityDuration = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network Variables for Speed Boost
    private NetworkVariable<float> netSpeedBoostCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netSpeedBoostDuration = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        // Player Character
        if (playerMovementSpeedInput != null)
            playerMovementSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.PlayerMovementSpeed, value));
        if (playerChannelTimeInput != null)
            playerChannelTimeInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.PlayerChannelTime, value));
        if (arenaHealIntervalInput != null)
            arenaHealIntervalInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ArenaHealInterval, value));
        if (arenaHealAmountInput != null)
            arenaHealAmountInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ArenaHealAmount, value));

        // Bullet
        if (bulletDamageInput != null)
            bulletDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletDamage, value));
        if (bulletProjectileSpeedInput != null)
            bulletProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletProjectileSpeed, value));
        if (bulletCooldownInput != null)
            bulletCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletCooldown, value));
        if (bulletMaxDistanceInput != null)
            bulletMaxDistanceInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BulletMaxDistance, value));

        // Boomerang
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

        // Mortar
        if (mortarDamageInput != null)
            mortarDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.MortarDamage, value));
        if (mortarProjectileSpeedInput != null)
            mortarProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.MortarProjectileSpeed, value));
        if (mortarMaxCooldownInput != null)
            mortarMaxCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.MortarMaxCooldown, value));
        if (mortarMaxHeightInput != null)
            mortarMaxHeightInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.MortarMaxHeight, value));
        if (mortarExplosionRadiusInput != null)
            mortarExplosionRadiusInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.MortarExplosionRadius, value));

        // Homing
        if (homingDamageInput != null)
            homingDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.HomingDamage, value));
        if (homingProjectileSpeedInput != null)
            homingProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.HomingProjectileSpeed, value));
        if (homingMaxCooldownInput != null)
            homingMaxCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.HomingMaxCooldown, value));
        if (homingMaxDistanceInput != null)
            homingMaxDistanceInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.HomingMaxDistance, value));
        if (homingTurnSensitivityInput != null)
            homingTurnSensitivityInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.HomingTurnSensitivity, value));

        // Curve
        if (curveDamageInput != null)
            curveDamageInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.CurveDamage, value));
        if (curveProjectileSpeedInput != null)
            curveProjectileSpeedInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.CurveProjectileSpeed, value));
        if (curveMaxCooldownInput != null)
            curveMaxCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.CurveMaxCooldown, value));
        if (curveStrengthInput != null)
            curveStrengthInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.CurveStrength, value));

        // Blink
        if (blinkCooldownInput != null)
            blinkCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BlinkCooldown, value));
        if (blinkRadiusInput != null)
            blinkRadiusInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.BlinkRadius, value));

        // Fakeshot
        if (fakeshotCooldownInput != null)
            fakeshotCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.FakeshotCooldown, value));

        // Parry
        if (parryCooldownInput != null)
            parryCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ParryCooldown, value));
        if (parryDurationInput != null)
            parryDurationInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.ParryDuration, value));

        // Invisibility
        if (invisibilityCooldownInput != null)
            invisibilityCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.InvisibilityCooldown, value));
        if (invisibilityDurationInput != null)
            invisibilityDurationInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.InvisibilityDuration, value));

        // Speed Boost
        if (speedBoostCooldownInput != null)
            speedBoostCooldownInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.SpeedBoostCooldown, value));
        if (speedBoostDurationInput != null)
            speedBoostDurationInput.onValueChanged.AddListener((value) => OnInputFieldChanged(InputFieldType.SpeedBoostDuration, value));
    }

    private void RemoveInputFieldListeners()
    {
        // Player Character
        if (playerMovementSpeedInput != null)
            playerMovementSpeedInput.onValueChanged.RemoveAllListeners();
        if (playerChannelTimeInput != null)
            playerChannelTimeInput.onValueChanged.RemoveAllListeners();
        if (arenaHealIntervalInput != null)
            arenaHealIntervalInput.onValueChanged.RemoveAllListeners();
        if (arenaHealAmountInput != null)
            arenaHealAmountInput.onValueChanged.RemoveAllListeners();

        // Bullet
        if (bulletDamageInput != null)
            bulletDamageInput.onValueChanged.RemoveAllListeners();
        if (bulletProjectileSpeedInput != null)
            bulletProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (bulletCooldownInput != null)
            bulletCooldownInput.onValueChanged.RemoveAllListeners();
        if (bulletMaxDistanceInput != null)
            bulletMaxDistanceInput.onValueChanged.RemoveAllListeners();

        // Boomerang
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

        // Mortar
        if (mortarDamageInput != null)
            mortarDamageInput.onValueChanged.RemoveAllListeners();
        if (mortarProjectileSpeedInput != null)
            mortarProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (mortarMaxCooldownInput != null)
            mortarMaxCooldownInput.onValueChanged.RemoveAllListeners();
        if (mortarMaxHeightInput != null)
            mortarMaxHeightInput.onValueChanged.RemoveAllListeners();
        if (mortarExplosionRadiusInput != null)
            mortarExplosionRadiusInput.onValueChanged.RemoveAllListeners();

        // Homing
        if (homingDamageInput != null)
            homingDamageInput.onValueChanged.RemoveAllListeners();
        if (homingProjectileSpeedInput != null)
            homingProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (homingMaxCooldownInput != null)
            homingMaxCooldownInput.onValueChanged.RemoveAllListeners();
        if (homingMaxDistanceInput != null)
            homingMaxDistanceInput.onValueChanged.RemoveAllListeners();
        if (homingTurnSensitivityInput != null)
            homingTurnSensitivityInput.onValueChanged.RemoveAllListeners();

        // Curve
        if (curveDamageInput != null)
            curveDamageInput.onValueChanged.RemoveAllListeners();
        if (curveProjectileSpeedInput != null)
            curveProjectileSpeedInput.onValueChanged.RemoveAllListeners();
        if (curveMaxCooldownInput != null)
            curveMaxCooldownInput.onValueChanged.RemoveAllListeners();
        if (curveStrengthInput != null)
            curveStrengthInput.onValueChanged.RemoveAllListeners();

        // Blink
        if (blinkCooldownInput != null)
            blinkCooldownInput.onValueChanged.RemoveAllListeners();
        if (blinkRadiusInput != null)
            blinkRadiusInput.onValueChanged.RemoveAllListeners();

        // Fakeshot
        if (fakeshotCooldownInput != null)
            fakeshotCooldownInput.onValueChanged.RemoveAllListeners();

        // Parry
        if (parryCooldownInput != null)
            parryCooldownInput.onValueChanged.RemoveAllListeners();
        if (parryDurationInput != null)
            parryDurationInput.onValueChanged.RemoveAllListeners();

        // Invisibility
        if (invisibilityCooldownInput != null)
            invisibilityCooldownInput.onValueChanged.RemoveAllListeners();
        if (invisibilityDurationInput != null)
            invisibilityDurationInput.onValueChanged.RemoveAllListeners();

        // Speed Boost
        if (speedBoostCooldownInput != null)
            speedBoostCooldownInput.onValueChanged.RemoveAllListeners();
        if (speedBoostDurationInput != null)
            speedBoostDurationInput.onValueChanged.RemoveAllListeners();
    }

    private enum InputFieldType
    {
        PlayerMovementSpeed,
        PlayerChannelTime,
        ArenaHealInterval,
        ArenaHealAmount,
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
        MortarDamage,
        MortarProjectileSpeed,
        MortarMaxCooldown,
        MortarMaxHeight,
        MortarExplosionRadius,
        HomingDamage,
        HomingProjectileSpeed,
        HomingMaxCooldown,
        HomingMaxDistance,
        HomingTurnSensitivity,
        CurveDamage,
        CurveProjectileSpeed,
        CurveMaxCooldown,
        CurveStrength,
        BlinkCooldown,
        BlinkRadius,
        FakeshotCooldown,
        ParryCooldown,
        ParryDuration,
        InvisibilityCooldown,
        InvisibilityDuration,
        SpeedBoostCooldown,
        SpeedBoostDuration
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
            // Player Character
            case InputFieldType.PlayerMovementSpeed:
                if (float.TryParse(value, out float movSpeed))
                    netPlayerMovementSpeed.Value = movSpeed;
                break;
            case InputFieldType.PlayerChannelTime:
                if (int.TryParse(value, out int channelTime))
                    netPlayerChannelTime.Value = channelTime;
                break;
            case InputFieldType.ArenaHealInterval:
                if (float.TryParse(value, out float healInterval))
                    netArenaHealInterval.Value = healInterval;
                break;
            case InputFieldType.ArenaHealAmount:
                if (byte.TryParse(value, out byte healAmount))
                    netArenaHealAmount.Value = healAmount;
                break;

            // Bullet
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

            // Boomerang
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

            // Mortar
            case InputFieldType.MortarDamage:
                if (byte.TryParse(value, out byte mortarDmg))
                    netMortarDamage.Value = mortarDmg;
                break;
            case InputFieldType.MortarProjectileSpeed:
                if (float.TryParse(value, out float mortarSpeed))
                    netMortarProjectileSpeed.Value = mortarSpeed;
                break;
            case InputFieldType.MortarMaxCooldown:
                if (float.TryParse(value, out float mortarCd))
                    netMortarMaxCooldown.Value = mortarCd;
                break;
            case InputFieldType.MortarMaxHeight:
                if (float.TryParse(value, out float mortarHeight))
                    netMortarMaxHeight.Value = mortarHeight;
                break;
            case InputFieldType.MortarExplosionRadius:
                if (float.TryParse(value, out float mortarRadius))
                    netMortarExplosionRadius.Value = mortarRadius;
                break;

            // Homing
            case InputFieldType.HomingDamage:
                if (byte.TryParse(value, out byte homingDmg))
                    netHomingDamage.Value = homingDmg;
                break;
            case InputFieldType.HomingProjectileSpeed:
                if (float.TryParse(value, out float homingSpeed))
                    netHomingProjectileSpeed.Value = homingSpeed;
                break;
            case InputFieldType.HomingMaxCooldown:
                if (float.TryParse(value, out float homingCd))
                    netHomingMaxCooldown.Value = homingCd;
                break;
            case InputFieldType.HomingMaxDistance:
                if (float.TryParse(value, out float homingDist))
                    netHomingMaxDistance.Value = homingDist;
                break;
            case InputFieldType.HomingTurnSensitivity:
                if (float.TryParse(value, out float homingTurn))
                    netHomingTurnSensitivity.Value = homingTurn;
                break;

            // Curve
            case InputFieldType.CurveDamage:
                if (byte.TryParse(value, out byte curveDmg))
                    netCurveDamage.Value = curveDmg;
                break;
            case InputFieldType.CurveProjectileSpeed:
                if (float.TryParse(value, out float curveSpeed))
                    netCurveProjectileSpeed.Value = curveSpeed;
                break;
            case InputFieldType.CurveMaxCooldown:
                if (float.TryParse(value, out float curveCd))
                    netCurveMaxCooldown.Value = curveCd;
                break;
            case InputFieldType.CurveStrength:
                if (float.TryParse(value, out float curveStr))
                    netCurveStrength.Value = curveStr;
                break;

            // Blink
            case InputFieldType.BlinkCooldown:
                if (float.TryParse(value, out float blinkCd))
                    netBlinkCooldown.Value = blinkCd;
                break;
            case InputFieldType.BlinkRadius:
                if (float.TryParse(value, out float blinkRad))
                    netBlinkRadius.Value = blinkRad;
                break;

            // Fakeshot
            case InputFieldType.FakeshotCooldown:
                if (float.TryParse(value, out float fakeCd))
                    netFakeshotCooldown.Value = fakeCd;
                break;

            // Parry
            case InputFieldType.ParryCooldown:
                if (float.TryParse(value, out float parryCd))
                    netParryCooldown.Value = parryCd;
                break;
            case InputFieldType.ParryDuration:
                if (float.TryParse(value, out float parryDur))
                    netParryDuration.Value = parryDur;
                break;

            // Invisibility
            case InputFieldType.InvisibilityCooldown:
                if (float.TryParse(value, out float invisCd))
                    netInvisibilityCooldown.Value = invisCd;
                break;
            case InputFieldType.InvisibilityDuration:
                if (float.TryParse(value, out float invisDur))
                    netInvisibilityDuration.Value = invisDur;
                break;

            // Speed Boost
            case InputFieldType.SpeedBoostCooldown:
                if (float.TryParse(value, out float speedCd))
                    netSpeedBoostCooldown.Value = speedCd;
                break;
            case InputFieldType.SpeedBoostDuration:
                if (float.TryParse(value, out float speedDur))
                    netSpeedBoostDuration.Value = speedDur;
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost)
            this.enabled = false;

        // Subscribe to network variable changes on clients
        // Player Character
        netPlayerMovementSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(playerMovementSpeedInput, newVal.ToString("F2"));
        netPlayerChannelTime.OnValueChanged += (oldVal, newVal) => UpdateInputField(playerChannelTimeInput, newVal.ToString());
        netArenaHealInterval.OnValueChanged += (oldVal, newVal) => UpdateInputField(arenaHealIntervalInput, newVal.ToString("F2"));
        netArenaHealAmount.OnValueChanged += (oldVal, newVal) => UpdateInputField(arenaHealAmountInput, newVal.ToString());

        // Bullet
        netBulletDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletDamageInput, newVal.ToString());
        netBulletProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletProjectileSpeedInput, newVal.ToString("F2"));
        netBulletCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletCooldownInput, newVal.ToString("F2"));
        netBulletMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(bulletMaxDistanceInput, newVal.ToString("F2"));

        // Boomerang
        netBoomerangDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangDamageInput, newVal.ToString());
        netBoomerangProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangProjectileSpeedInput, newVal.ToString("F2"));
        netBoomerangMaxCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangMaxCooldownInput, newVal.ToString("F2"));
        netBoomerangMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangMaxDistanceInput, newVal.ToString("F2"));
        netBoomerangReturnMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangReturnMaxDistanceInput, newVal.ToString("F2"));
        netBoomerangCooldownReduction.OnValueChanged += (oldVal, newVal) => UpdateInputField(boomerangCooldownReductionInput, newVal.ToString("F2"));

        // Mortar
        netMortarDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(mortarDamageInput, newVal.ToString());
        netMortarProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(mortarProjectileSpeedInput, newVal.ToString("F2"));
        netMortarMaxCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(mortarMaxCooldownInput, newVal.ToString("F2"));
        netMortarMaxHeight.OnValueChanged += (oldVal, newVal) => UpdateInputField(mortarMaxHeightInput, newVal.ToString("F2"));
        netMortarExplosionRadius.OnValueChanged += (oldVal, newVal) => UpdateInputField(mortarExplosionRadiusInput, newVal.ToString("F2"));

        // Homing
        netHomingDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(homingDamageInput, newVal.ToString());
        netHomingProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(homingProjectileSpeedInput, newVal.ToString("F2"));
        netHomingMaxCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(homingMaxCooldownInput, newVal.ToString("F2"));
        netHomingMaxDistance.OnValueChanged += (oldVal, newVal) => UpdateInputField(homingMaxDistanceInput, newVal.ToString("F2"));
        netHomingTurnSensitivity.OnValueChanged += (oldVal, newVal) => UpdateInputField(homingTurnSensitivityInput, newVal.ToString("F2"));

        // Curve
        netCurveDamage.OnValueChanged += (oldVal, newVal) => UpdateInputField(curveDamageInput, newVal.ToString());
        netCurveProjectileSpeed.OnValueChanged += (oldVal, newVal) => UpdateInputField(curveProjectileSpeedInput, newVal.ToString("F2"));
        netCurveMaxCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(curveMaxCooldownInput, newVal.ToString("F2"));
        netCurveStrength.OnValueChanged += (oldVal, newVal) => UpdateInputField(curveStrengthInput, newVal.ToString("F2"));

        // Blink
        netBlinkCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(blinkCooldownInput, newVal.ToString("F2"));
        netBlinkRadius.OnValueChanged += (oldVal, newVal) => UpdateInputField(blinkRadiusInput, newVal.ToString("F2"));

        // Fakeshot
        netFakeshotCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(fakeshotCooldownInput, newVal.ToString("F2"));

        // Parry
        netParryCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(parryCooldownInput, newVal.ToString("F2"));
        netParryDuration.OnValueChanged += (oldVal, newVal) => UpdateInputField(parryDurationInput, newVal.ToString("F2"));

        // Invisibility
        netInvisibilityCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(invisibilityCooldownInput, newVal.ToString("F2"));
        netInvisibilityDuration.OnValueChanged += (oldVal, newVal) => UpdateInputField(invisibilityDurationInput, newVal.ToString("F2"));

        // Speed Boost
        netSpeedBoostCooldown.OnValueChanged += (oldVal, newVal) => UpdateInputField(speedBoostCooldownInput, newVal.ToString("F2"));
        netSpeedBoostDuration.OnValueChanged += (oldVal, newVal) => UpdateInputField(speedBoostDurationInput, newVal.ToString("F2"));
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
        gameAreaBehaviour.SetArenaHealCooldown(GetArenaHealInterval());
        arenaHeal.SetArenaHealAmount(GetArenaHealAmount());

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
        // Player Character
        if (playerPrefab != null)
        {
            NavMeshAgent navMeshAgent = playerPrefab.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null && playerMovementSpeedInput != null)
                playerMovementSpeedInput.text = navMeshAgent.speed.ToString("F2");
            if (playerChannelTimeInput != null)
                playerChannelTimeInput.text = playerPrefab.GetComponent<Shooter>().ChannelDuration.ToString("F2");
        }

        if (arenaHealAmountInput != null && arenaHeal != null)
            arenaHealAmountInput.text = arenaHeal.HealAmount.ToString();
        if (arenaHealIntervalInput != null && gameAreaBehaviour != null)
            arenaHealIntervalInput.text = gameAreaBehaviour.HealCooldown.ToString("F2");

        // Bullet
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

        // Boomerang
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

        // Mortar
        if (mortarPrefab != null)
        {
            if (mortarDamageInput != null)
                mortarDamageInput.text = mortarPrefab.ProjectileDamage.ToString();
            if (mortarProjectileSpeedInput != null)
                mortarProjectileSpeedInput.text = mortarPrefab.ProjectileSpeed.ToString("F2");
            if (mortarMaxCooldownInput != null)
                mortarMaxCooldownInput.text = mortarPrefab.MaxCooldown.ToString("F2");
            if (mortarMaxHeightInput != null)
                mortarMaxHeightInput.text = mortarPrefab.MaxHeight.ToString("F2");
            if (mortarExplosionRadiusInput != null)
                mortarExplosionRadiusInput.text = mortarPrefab.ExplosionRadius.ToString("F2");
        }

        // Homing
        if (homingPrefab != null)
        {
            if (homingDamageInput != null)
                homingDamageInput.text = homingPrefab.ProjectileDamage.ToString();
            if (homingProjectileSpeedInput != null)
                homingProjectileSpeedInput.text = homingPrefab.ProjectileSpeed.ToString("F2");
            if (homingMaxCooldownInput != null)
                homingMaxCooldownInput.text = homingPrefab.MaxCooldown.ToString("F2");
            if (homingMaxDistanceInput != null)
                homingMaxDistanceInput.text = homingPrefab.MaxDistance.ToString("F2");
            if (homingTurnSensitivityInput != null)
                homingTurnSensitivityInput.text = homingPrefab.TurnSensitivity.ToString();
        }

        // Curve
        if (curvePrefab != null)
        {
            if (curveDamageInput != null)
                curveDamageInput.text = curvePrefab.ProjectileDamage.ToString();
            if (curveProjectileSpeedInput != null)
                curveProjectileSpeedInput.text = curvePrefab.ProjectileSpeed.ToString("F2");
            if (curveMaxCooldownInput != null)
                curveMaxCooldownInput.text = curvePrefab.MaxCooldown.ToString("F2");
            if (curveStrengthInput != null)
                curveStrengthInput.text = curvePrefab.CurveStrength.ToString("F2");
        }

        // Blink
        if (blinkPrefab != null)
        {
            if (blinkCooldownInput != null)
                blinkCooldownInput.text = blinkPrefab.MaxCooldown.ToString("F2");
            if (blinkRadiusInput != null)
                blinkRadiusInput.text = blinkPrefab.BlinkRadius.ToString("F2");
        }

        // Fakeshot
        if (fakeshotPrefab != null)
        {
            if (fakeshotCooldownInput != null)
                fakeshotCooldownInput.text = fakeshotPrefab.MaxCooldown.ToString("F2");
        }

        // Parry
        if (parryPrefab != null)
        {
            if (parryCooldownInput != null)
                parryCooldownInput.text = parryPrefab.MaxCooldown.ToString("F2");
            if (parryDurationInput != null)
                parryDurationInput.text = parryPrefab.ParryDuration.ToString("F2");
        }

        // Invisibility
        if (invisibilityPrefab != null)
        {
            if (invisibilityCooldownInput != null)
                invisibilityCooldownInput.text = invisibilityPrefab.MaxCooldown.ToString("F2");
            if (invisibilityDurationInput != null)
                invisibilityDurationInput.text = invisibilityPrefab.InvisibilityDuration.ToString("F2");
        }

        // Speed Boost
        if (speedBoostPrefab != null)
        {
            if (speedBoostCooldownInput != null)
                speedBoostCooldownInput.text = speedBoostPrefab.MaxCooldown.ToString("F2");
            if (speedBoostDurationInput != null)
                speedBoostDurationInput.text = speedBoostPrefab.SpeedBoostDuration.ToString("F2");
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

    // ========== GETTER METHODS ==========

    // Player Character Getters
    public float GetPlayerMovementSpeed() => netPlayerMovementSpeed.Value;
    public int GetPlayerChannelTime() => netPlayerChannelTime.Value;
    public float GetArenaHealInterval() => netArenaHealInterval.Value;
    public byte GetArenaHealAmount() => netArenaHealAmount.Value;

    // Bullet Getters
    public byte GetBulletDamage() => netBulletDamage.Value;
    public float GetBulletProjectileSpeed() => netBulletProjectileSpeed.Value;
    public float GetBulletCooldown() => netBulletCooldown.Value;
    public float GetBulletMaxDistance() => netBulletMaxDistance.Value;

    // Boomerang Getters
    public byte GetBoomerangDamage() => netBoomerangDamage.Value;
    public float GetBoomerangProjectileSpeed() => netBoomerangProjectileSpeed.Value;
    public float GetBoomerangMaxCooldown() => netBoomerangMaxCooldown.Value;
    public float GetBoomerangMaxDistance() => netBoomerangMaxDistance.Value;
    public float GetBoomerangReturnMaxDistance() => netBoomerangReturnMaxDistance.Value;
    public float GetBoomerangCooldownReduction() => netBoomerangCooldownReduction.Value;

    // Mortar Getters
    public byte GetMortarDamage() => netMortarDamage.Value;
    public float GetMortarProjectileSpeed() => netMortarProjectileSpeed.Value;
    public float GetMortarMaxCooldown() => netMortarMaxCooldown.Value;
    public float GetMortarMaxHeight() => netMortarMaxHeight.Value;
    public float GetMortarExplosionRadius() => netMortarExplosionRadius.Value;

    // Homing Getters
    public byte GetHomingDamage() => netHomingDamage.Value;
    public float GetHomingProjectileSpeed() => netHomingProjectileSpeed.Value;
    public float GetHomingMaxCooldown() => netHomingMaxCooldown.Value;
    public float GetHomingMaxDistance() => netHomingMaxDistance.Value;
    public float GetHomingTurnSensitivity() => netHomingTurnSensitivity.Value;

    // Curve Getters
    public byte GetCurveDamage() => netCurveDamage.Value;
    public float GetCurveProjectileSpeed() => netCurveProjectileSpeed.Value;
    public float GetCurveMaxCooldown() => netCurveMaxCooldown.Value;
    public float GetCurveStrength() => netCurveStrength.Value;

    // Blink Getters
    public float GetBlinkCooldown() => netBlinkCooldown.Value;
    public float GetBlinkRadius() => netBlinkRadius.Value;

    // Fakeshot Getters
    public float GetFakeshotCooldown() => netFakeshotCooldown.Value;

    // Parry Getters
    public float GetParryCooldown() => netParryCooldown.Value;
    public float GetParryDuration() => netParryDuration.Value;

    // Invisibility Getters
    public float GetInvisibilityCooldown() => netInvisibilityCooldown.Value;
    public float GetInvisibilityDuration() => netInvisibilityDuration.Value;

    // Speed Boost Getters
    public float GetSpeedBoostCooldown() => netSpeedBoostCooldown.Value;
    public float GetSpeedBoostDuration() => netSpeedBoostDuration.Value;

    public bool OverrideValues => overrideValues;

    public TMP_InputField.OnChangeEvent OnVlaueChanged { get; private set; }
    public bool DashboardEnabled => dashboardOpen;
}