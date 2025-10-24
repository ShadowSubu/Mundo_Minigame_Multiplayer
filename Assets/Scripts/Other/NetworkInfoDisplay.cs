using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private float updateFrequency = 1f;

    private float timeSinceLastUpdate = 0f;
    private int currentPing = 0;

    // Color thresholds
    [SerializeField] private Color excellentColor = Color.green;
    [SerializeField] private Color goodColor = Color.yellow;
    [SerializeField] private Color fairColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color poorColor = Color.red;

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateFrequency)
        {
            UpdatePing();
            timeSinceLastUpdate = 0f;
        }
    }

    private void UpdatePing()
    {
        currentPing = GetNetworkPing();

        UpdatePingDisplay();
    }

    private void UpdatePingDisplay()
    {
        if (pingText != null)
        {
            pingText.text = $"PING: {currentPing}ms";
            pingText.color = GetPingColor(currentPing);
        }
    }

    private Color GetPingColor(int ping)
    {
        if (ping < 50)
            return excellentColor;
        else if (ping < 100)
            return goodColor;
        else if (ping < 150)
            return fairColor;
        else
            return poorColor;
    }

    private int GetNetworkPing()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong rtt = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);
            return (int)(rtt);
        }

        return 0;
    }
}
