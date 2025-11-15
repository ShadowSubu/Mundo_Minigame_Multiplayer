using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;

    [Header("Lobby options UI")]
    [SerializeField] private GameObject lobbyOptionsPanel;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;

    [Header("Create Lobby UI")]
    [SerializeField] private GameObject createLobbyUI;
    [SerializeField] private TMP_InputField createLobbyNameInputField;
    [SerializeField] private Toggle createLobbyPrivateToggle;
    [SerializeField] private Button createLobbyConfirmButton;
    [SerializeField] private Button leaveCreateLobbyButton;
    [SerializeField] private Button oneVoneButton;
    [SerializeField] private Button twoVtwoButton;
    private GameMode gameMode;

    [Header("Join Lobby UI")]
    [SerializeField] private GameObject joinLobbyUI;
    [SerializeField] private PublicLobbySingleUI publicLobbyPrefab;
    [SerializeField] private Button refreshPublicLobbiesButton;
    [SerializeField] private TMP_InputField privateLobbyCodeInputField;
    [SerializeField] private Button joinPrivateLobbyButton;
    [SerializeField] private Transform publicLobbiesContainer;
    [SerializeField] private Button leaveJoinLobbyUI;

    [Header("Lobby Details UI")]
    [SerializeField] private GameObject lobbyDetailsUI;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private LobbyPlayerSingleUI lobbyPlayerPrefab;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Transform lobbyPlayerContainer;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private Button copyLobbyCodeButton;
    [SerializeField] private Button startGameButton;

    [Header("Loadout UI")]
    [SerializeField] private List<ProjectileSelectionButton> projectileSelectionButtons;
    [SerializeField] private List<AbilitySelectionButton> abilitySelectionButtons;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color defaultColor;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Error UI")]
    [SerializeField] private GameObject errorUI;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button errorCloseButton;

    [Header("Other UI")]
    [SerializeField] private Button settingButton;

    private void Awake()
    {
        lobbyPanel.SetActive(false);
        lobbyOptionsPanel.SetActive(false);
        createLobbyUI.SetActive(false);
        joinLobbyUI.SetActive(false);
        lobbyDetailsUI.SetActive(false);
        loadingUI.SetActive(false);
        errorUI.SetActive(false);
        oneVoneButton.image.color = new Color(1, 1, 1, 1f);
        twoVtwoButton.image.color = new Color(1, 1, 1, 0.5f);
        gameMode = GameMode.oneVone;
    }

    private void OnEnable()
    {
        createLobbyButton.onClick.AddListener(OpenCreateLobbyUI);
        joinLobbyButton.onClick.AddListener(OpenJoinLobbyUI);
        createLobbyConfirmButton.onClick.AddListener(CreateLobby);
        leaveCreateLobbyButton.onClick.AddListener(LeaveCreateLobby);
        refreshPublicLobbiesButton.onClick.AddListener(RefreshPublicLobbies);
        joinPrivateLobbyButton.onClick.AddListener(JoinPrivateLobby);
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
        leaveJoinLobbyUI.onClick.AddListener(LeaveJoinLobbyUI);
        errorCloseButton.onClick.AddListener(CloseError);
        startGameButton.onClick.AddListener(StartGame);
        oneVoneButton.onClick.AddListener(SelectGameMode1v1);
        twoVtwoButton.onClick.AddListener(SelectGameMode2v2);
        settingButton.onClick.AddListener(OpenSettings);
    }

    private void OnDisable()
    {
        createLobbyButton.onClick.RemoveAllListeners();
        joinLobbyButton.onClick.RemoveAllListeners();
        createLobbyConfirmButton.onClick.RemoveAllListeners();
        leaveCreateLobbyButton.onClick.RemoveAllListeners();
        refreshPublicLobbiesButton.onClick.RemoveAllListeners();
        joinPrivateLobbyButton.onClick.RemoveAllListeners();
        leaveLobbyButton.onClick.RemoveAllListeners();
        leaveJoinLobbyUI.onClick.RemoveAllListeners();
        errorCloseButton.onClick.RemoveAllListeners();
        startGameButton.onClick.RemoveAllListeners();
        oneVoneButton.onClick.RemoveAllListeners();
        twoVtwoButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        lobbyPanel.SetActive(true);
        lobbyOptionsPanel.SetActive(true);

        LobbyManager.Instance.OnPublicLobbyListChanged += LobbyManager_OnPublicLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLobbyLeft;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
        LobbyManager.Instance.OnJoinedLobbyUpdate += LobbyManager_OnJoinedLobbyUpdate;
        LobbyManager.Instance.OnServiceError += Instance_OnServiceError;
        LobbyManager.Instance.OnPlayerLoadoutSelection += Instance_OnPlayerLoadoutSelection;

        InitializeProjectileSelectionButtons();
        InitializeAbilitySelectionButtons();
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnPublicLobbyListChanged -= LobbyManager_OnPublicLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnLobbyJoined;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLobbyLeft;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= LobbyManager_OnJoinedLobbyUpdate;
        LobbyManager.Instance.OnServiceError -= Instance_OnServiceError;
        LobbyManager.Instance.OnPlayerLoadoutSelection -= Instance_OnPlayerLoadoutSelection;
    }

    private void OpenJoinLobbyUI()
    {
        lobbyOptionsPanel.SetActive(false);
        joinLobbyUI.SetActive(true);
    }

    private void OpenCreateLobbyUI()
    {
        lobbyOptionsPanel.SetActive(false);
        createLobbyUI.SetActive(true);
    }

    private void SelectGameMode1v1()
    {
        oneVoneButton.image.color = new Color(1, 1, 1, 1f);
        twoVtwoButton.image.color = new Color(1, 1, 1, 0.5f);
        gameMode = GameMode.oneVone;
    }

    private void SelectGameMode2v2()
    {
        oneVoneButton.image.color = new Color(1, 1, 1, 0.5f);
        twoVtwoButton.image.color = new Color(1, 1, 1, 1f);
        gameMode = GameMode.twoVtwo;
    }

    private void CreateLobby()
    {
        if (!string.IsNullOrEmpty(createLobbyNameInputField.text))
        {
            LobbyManager.Instance.CreateLobby(createLobbyNameInputField.text, createLobbyPrivateToggle.isOn, gameMode);
            ShowLoading("Creating lobby...");
        }
        else
        {
            Debug.LogWarning("Lobby name cannot be empty.");
            ShowError("Lobby name cannot be empty.");
        }
    }

    private void LeaveCreateLobby()
    {
        createLobbyUI.SetActive(false);
        lobbyOptionsPanel.SetActive(true);
    }

    private void JoinPrivateLobby()
    {
        if (!string.IsNullOrEmpty(privateLobbyCodeInputField.text))
        {
            LobbyManager.Instance.JoinLobbyByCode(privateLobbyCodeInputField.text);
            ShowLoading("Joining private lobby...");
        }
        else
        {
            Debug.LogWarning("Private lobby code cannot be empty.");
            ShowError("Private lobby code cannot be empty.");
        }
    }

    private void LobbyManager_OnPublicLobbyListChanged(object sender, LobbyManager.PublicLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.Lobbies);
    }

    private void Instance_OnPlayerLoadoutSelection(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby(e.lobby);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in publicLobbiesContainer)
        {
            if (child == publicLobbyPrefab) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            PublicLobbySingleUI lobbySingleTransform = Instantiate(publicLobbyPrefab, publicLobbiesContainer);
            lobbySingleTransform.gameObject.SetActive(true);
            lobbySingleTransform.Initialize(lobby, () =>
            {
                LobbyManager.Instance.JoinLobby(lobby);
            });
        }
    }

    private void LobbyManager_OnJoinedLobbyUpdate(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby(e.lobby);
        HideLoading();
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        foreach (Player player in lobby.Players)
        {
            LobbyPlayerSingleUI playerSingleTransform = Instantiate(lobbyPlayerPrefab, lobbyPlayerContainer);
            playerSingleTransform.gameObject.SetActive(true);
            playerSingleTransform.Initialize(
                player.Data[LobbyManager.KEY_PLAYER_NAME].Value,
                LobbyManager.Instance.IsLobbyHost() && player.Id == lobby.HostId,
                () => LobbyManager.Instance.KickPlayer(player.Id)
            );
        }

        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
        copyLobbyCodeButton.onClick.RemoveAllListeners();
        copyLobbyCodeButton.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = lobby.LobbyCode;
            Debug.Log("Lobby code copied to clipboard: " + lobby.LobbyCode);
        });
        ChangeStartGameButtonState();
    }

    private void ClearLobby()
    {
        foreach (Transform child in lobbyPlayerContainer)
        {
            if (child == lobbyPlayerPrefab) continue;
            Destroy(child.gameObject);
        }
    }

    private void RefreshPublicLobbies()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void LeaveJoinLobbyUI()
    {
        joinLobbyUI.SetActive(false);
        lobbyOptionsPanel.SetActive(true);
    }

    private void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        joinLobbyUI.SetActive(true);
        lobbyDetailsUI.SetActive(false);
        ChangeStartGameButtonState();
    }

    private void LobbyManager_OnLobbyLeft(object sender, EventArgs e)
    {
        joinLobbyUI.SetActive(true);
        lobbyDetailsUI.SetActive(false);
        ChangeStartGameButtonState();
    }

    private void LobbyManager_OnLobbyJoined(object sender, LobbyManager.LobbyEventArgs e)
    {
        joinLobbyUI.SetActive(false);
        createLobbyUI.SetActive(false);
        lobbyDetailsUI.SetActive(true);

        startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        ChangeStartGameButtonState();
    }

    private void InitializeProjectileSelectionButtons()
    {
        for (int i = 0; i < projectileSelectionButtons.Count; i++)
        {
            projectileSelectionButtons[i].OnProjectileSelected += OnProjectileSelected;
            projectileSelectionButtons[i].GetComponent<Image>().color = defaultColor;
        }
    }

    private void OnProjectileSelected(object sender, GameObject selectedButton)
    {
        foreach (var button in projectileSelectionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
        }
        selectedButton.GetComponent<Image>().color = selectedColor;
    }

    private void InitializeAbilitySelectionButtons()
    {
        for (int i = 0; i < abilitySelectionButtons.Count; i++)
        {
            abilitySelectionButtons[i].OnAbilitySelected += OnAbilitySelected;
            abilitySelectionButtons[i].GetComponent<Image>().color = defaultColor;
        }
    }

    private void OnAbilitySelected(object sender, GameObject selectedButton)
    {
        foreach (var button in abilitySelectionButtons)
        {
            button.GetComponent<Image>().color = defaultColor;
        }
        selectedButton.GetComponent<Image>().color = selectedColor;
    }

    private void StartGame()
    {
        switch (gameMode)
        {
            case GameMode.oneVone:
                SceneLoadingManager.Instance.LoadSceneAsync("Game 1v1");
                break;
            case GameMode.twoVtwo:
                SceneLoadingManager.Instance.LoadSceneAsync("Game 2v2");
                break;
            default:
                break;
        }
    }

    private void ChangeStartGameButtonState()
    {
        startGameButton.interactable = LobbyManager.Instance.IsLobbyReady();
    }

    private void ShowLoading(string loadingMessage)
    {
        loadingMessage = string.IsNullOrEmpty(loadingMessage) ? "Loading..." : loadingMessage;
        loadingUI.SetActive(true);
    }

    private void HideLoading()
    {
        loadingUI.SetActive(false);
    }

    private void Instance_OnServiceError(object sender, string e)
    {
        ShowError(e);
    }

    private void ShowError(string errorMessage)
    {
        errorUI.SetActive(true);
        errorText.text = errorMessage;
        loadingUI.SetActive(false);
    }

    private void CloseError()
    {
        errorUI.SetActive(false);
    }

    private void OpenSettings()
    {
        SettingsManager.Instance.OpenSetting();
    }
}
