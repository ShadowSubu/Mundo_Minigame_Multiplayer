using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PublicPlayerHealthIndicator : MonoBehaviour
{
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    private TargetPlayer targetPlayer;
    private byte maxHealth;

    private void Start()
    {
        playerHealthBar.fillAmount = 1f;
    }

    public void Initialize(TargetPlayer targetPlayer, byte maxHealth, string playerName)
    {
        this.maxHealth = maxHealth;
        playerHealthText.text = maxHealth.ToString();
        playerNameText.text = GetTextBeforeHash(playerName);
        this.targetPlayer = targetPlayer;
        this.targetPlayer.OnHealthChanged += TargetPLayer_OnHealthChanged;
    }

    private void TargetPLayer_OnHealthChanged(object sender, byte currentHealth)
    {
        playerHealthBar.fillAmount = (float)currentHealth/(float)maxHealth;
        playerHealthText.text = currentHealth.ToString();
    }

    private void OnDestroy()
    {
        targetPlayer.OnHealthChanged -= TargetPLayer_OnHealthChanged;
    }

    private string GetTextBeforeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        int index = input.IndexOf('#');
        if (index >= 0)
            return input.Substring(0, index);

        return input;
    }
}
