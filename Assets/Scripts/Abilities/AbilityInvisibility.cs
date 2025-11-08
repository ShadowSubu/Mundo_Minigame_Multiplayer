using System;
using Unity.Netcode;
using UnityEngine;

public class AbilityInvisibility : AbilityBase
{
    [Header("Invisibility Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float allyAlpha = 0.3f;
    [SerializeField] private float enemyAlpha = 0f;

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private CanvasGroup canvasToHide;
    private MaterialPropertyBlock propertyBlock;
    private Color[] originalColors;

    private void Awake()
    {
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            maxCooldown = DeveloperDashboard.Instance.GetInvisibilityCooldown();
            duration = DeveloperDashboard.Instance.GetInvisibilityDuration();
        }
    }

    internal override void OnAbilityUse(Ray ray, GameManager.Team team)
    {
        RequestToInvisibleRpc(team);
    }

    [Rpc(SendTo.Server)]
    private void RequestToInvisibleRpc(GameManager.Team team)
    {
        ActivateInvisibilityRpc(team);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivateInvisibilityRpc(GameManager.Team team)
    {
        GameManager.Team playerTeam = GameManager.Instance.GetLocalPlayerTeam();
        if (playerTeam == team)
        {
            // Make it partially Invisible
            ApplyInvisibility(allyAlpha);
        }
        else
        {
            // Make it Fully Invisible
            ApplyInvisibility(enemyAlpha);
        }
        Invoke(nameof(RestoreInvisibility), duration);
    }

    private void ApplyInvisibility(float alphaValue)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material mat = renderers[i].material;

            // Enable transparency
            //SetMaterialTransparent(mat);

            // Apply alpha
            Color newColor = originalColors[i];
            newColor.a = alphaValue;
            mat.color = newColor;

            canvasToHide.alpha = alphaValue;
        }
    }

    private void RestoreInvisibility()
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material mat = renderers[i].material;
            //SetMaterialOpaque(mat);
            mat.color = originalColors[i];
            canvasToHide.alpha = 1;
        }
    }

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private void SetMaterialOpaque(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }

    public float InvisibilityDuration => duration;
}
