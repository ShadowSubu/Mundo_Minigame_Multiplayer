using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameAuthenticationUI : MonoBehaviour
{
    [SerializeField] GameAuthentication gameAuthentication;

    [Header("Lobby UI")]
    [SerializeField] GameObject lobbyUI;


    [Header("Player Name Change UI")]
    [SerializeField] GameObject playerNameChangeUI;
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] Button confirmPlayerNameButton;

    [Header("Sign In Failure UI")]
    [SerializeField] GameObject signInFailureUI;
    [SerializeField] Button retrySigninButton;

    private void Awake()
    {
        gameAuthentication.OnSignInCompleted += HandleSignInCompleted;
        gameAuthentication.OnPlayerNameChanged += HandlePlayerNameChanged;
    }

    private void OnEnable()
    {
        retrySigninButton.onClick.AddListener(RetrySignin);
        confirmPlayerNameButton.onClick.AddListener(CheckAndConfirmPlayerName);
    }

    private void OnDisable()
    {
        retrySigninButton.onClick.RemoveAllListeners();
        confirmPlayerNameButton.onClick.RemoveAllListeners();
    }

    private void HandleSignInCompleted(object sender, GameAuthentication.OnSignInCompletedEventArgs e)
    {
        if (e.success)
        {
            Debug.Log($"Player ID: {e.playerId}");
            Debug.Log($"Player Name: {e.playerName}");

            if (!string.IsNullOrEmpty(e.playerName))
            {
                // Open Lobby UI
            }
            else
            {
                // Open Player Name Change UI
                playerNameChangeUI.SetActive(true);
            }
        }
        else
        {
            // Handle sign-in failure
            signInFailureUI.SetActive(true);
        }
    }

    private void HandlePlayerNameChanged(object sender, bool success)
    {
        if (success)
        {
            playerNameChangeUI.SetActive(false);
            lobbyUI.SetActive(true);
        }
        else
        {
            // Handle Failure to change player name
        }
    }

    private void CheckAndConfirmPlayerName()
    {
        // Add constraints for the player name
        // Check if the string is valid
        gameAuthentication.CheckPlayerNameAndConfirm(playerNameInputField.text);
    }

    private async void RetrySignin()
    {
        signInFailureUI.SetActive(false);
        await gameAuthentication.SignInAnonymouslyAsync();
    }
}
