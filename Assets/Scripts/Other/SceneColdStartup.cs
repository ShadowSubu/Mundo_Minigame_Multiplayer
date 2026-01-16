using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class SceneColdStartup : MonoBehaviour
{
    public static SceneColdStartup Instance;
    [SerializeField] private bool startSceneCold = true;
    public bool StartSceneCold => startSceneCold;

    [SerializeField] private PlayerController playerControllerPrefab;
    [SerializeField] private Transform playerSpawnPosition;
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
            await Task.Yield();
        }

        PlayerController playerController = Instantiate(playerControllerPrefab, playerSpawnPosition.position, playerSpawnPosition.rotation);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        //playerController.GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);

        playerController.GetComponent<PlayerController>().InitializeRpc(GameManager.Team.A, projectileDropdown.options[projectileDropdown.value].text, abilityDropdown.options[abilityDropdown.value].text);
        
        await Task.Yield();

        loadoutSelectionWindow.SetActive(false);
        playerController.GetComponent<PlayerUI>().TogglePlayerUICanvas(true);
    }
}
