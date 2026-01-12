using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SceneColdStartup : MonoBehaviour
{
    public static SceneColdStartup Instance;
    [SerializeField] private bool startSceneCold = true;
    public bool StartSceneCold => startSceneCold;

    [SerializeField] private NetworkObject playerController;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Dropdown projectileDropdown;
    [SerializeField] private TMP_Dropdown abilityDropdown;
    [SerializeField] private GameObject loadoutSelectionWindow;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(SpawnPlayer);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(SpawnPlayer);
    }

    private void Start()
    {
        //SpawnPlayer();
        loadoutSelectionWindow.SetActive(true);
    }


    private async void SpawnPlayer()
    {
        // Start the host
        NetworkManager.Singleton.StartHost();

        // Wait until the network is ready
        while (!NetworkManager.Singleton.IsListening)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        // Wait one more frame to ensure everything is initialized
        await System.Threading.Tasks.Task.Yield();

        // Change ownership
        playerController.ChangeOwnership(NetworkManager.Singleton.LocalClientId);

        // Wait one frame for ownership change to process
        await System.Threading.Tasks.Task.Yield();

        // Now call the RPC
        playerController.GetComponent<PlayerController>().InitializeRpc(GameManager.Team.A, projectileDropdown.options[projectileDropdown.value].text, abilityDropdown.options[abilityDropdown.value].text);
        loadoutSelectionWindow.SetActive(false);
        playerController.GetComponent<PlayerUI>().TogglePlayerUICanvas(true);
    }
}
