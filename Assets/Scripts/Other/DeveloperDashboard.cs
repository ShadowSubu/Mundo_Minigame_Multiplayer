using System;
using System.Collections.Generic;
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
    }

    private void OnDisable()
    {
        confirmButton.onClick.AddListener(ConfirmValues);
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

    // Player Character Getters
    public float GetPlayerMovementSpeed() => float.TryParse(playerMovementSpeedInput.text, out float result) ? result : 0f;
    public int GetPlayerChannelTime() => int.TryParse(playerChannelTimeInput.text, out int result) ? result : 0;

    // Bullet Getters
    public byte GetBulletDamage() => byte.TryParse(bulletDamageInput.text, out byte result) ? result : (byte)0;
    public float GetBulletProjectileSpeed() => float.TryParse(bulletProjectileSpeedInput.text, out float result) ? result : 0f;
    public float GetBulletCooldown() => float.TryParse(bulletCooldownInput.text, out float result) ? result : 0f;
    public float GetBulletMaxDistance() => float.TryParse(bulletMaxDistanceInput.text, out float result) ? result : 0f;

    // Boomerang Getters
    public byte GetBoomerangDamage() => byte.TryParse(boomerangDamageInput.text, out byte result) ? result : (byte)0;
    public float GetBoomerangProjectileSpeed() => float.TryParse(boomerangProjectileSpeedInput.text, out float result) ? result : 0f;
    public float GetBoomerangMaxCooldown() => float.TryParse(boomerangMaxCooldownInput.text, out float result) ? result : 0f;
    public float GetBoomerangMaxDistance() => float.TryParse(boomerangMaxDistanceInput.text, out float result) ? result : 0f;
    public float GetBoomerangReturnMaxDistance() => float.TryParse(boomerangReturnMaxDistanceInput.text, out float result) ? result : 0f;
    public float GetBoomerangCooldownReduction() => float.TryParse(boomerangCooldownReductionInput.text, out float result) ? result : 0f;

    // Blink Getters
    public float GetBlinkCooldown() => float.TryParse(blinkCooldownInput.text, out float result) ? result : 0f;
    public float GetBlinkRadius() => float.TryParse(blinkRadiusInput.text, out float result) ? result : 0f;

    // Fakeshot Getters
    public float GetFakeshotCooldown() => float.TryParse(fakeshotCooldownInput.text, out float result) ? result : 0f;

    // Parry Getters
    public float GetParryCooldown() => float.TryParse(parryCooldownInput.text, out float result) ? result : 0f;
    public float GetParryDuration() => float.TryParse(parryDurationInput.text, out float result) ? result : 0f;

    public bool OverrideValues => overrideValues;
}