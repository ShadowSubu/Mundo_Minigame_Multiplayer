using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image cooldownBar;
    [SerializeField] private Canvas playerCanvas;
    [SerializeField] private Transform healthbarTransform;

    private PlayerController playerController;
    private Shooter shooter;
    private Camera mainCamera;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        shooter = GetComponent<Shooter>();

        mainCamera = Camera.main;
    }

    private void Start()
    {
        playerController.OnHealthChanged += UpdateHealthBar;
        shooter.OnCooldownChanged += UpdateCooldownBar;

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

    private void UpdateCooldownBar(object sender, float e)
    {
        cooldownBar.fillAmount = e / shooter.GetMaxCooldown();
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
}
