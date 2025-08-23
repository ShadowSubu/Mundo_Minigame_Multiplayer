using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class GameAuthentication : MonoBehaviour
{
    public event EventHandler<OnSignInCompletedEventArgs> OnSignInCompleted;
    public class OnSignInCompletedEventArgs : EventArgs
    {
        public bool success;
        public string playerId;
        public string playerName;
    }

    public event EventHandler<bool> OnPlayerNameChanged;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInAnonymouslyAsync();
        }
    }

    public async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            OnSignInCompleted?.Invoke(this, new OnSignInCompletedEventArgs
            {
                success = true,
                playerId = AuthenticationService.Instance.PlayerId,
                playerName = AuthenticationService.Instance.PlayerName
            });
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            OnSignInCompleted?.Invoke(this, new OnSignInCompletedEventArgs
            {
                success = false,
                playerId = null,
                playerName = null
            });
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            OnSignInCompleted?.Invoke(this, new OnSignInCompletedEventArgs
            {
                success = false,
                playerId = null,
                playerName = null
            });
        }
    }

    public async void CheckPlayerNameAndConfirm(string playerName)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            Debug.Log($"Player name updated to: {playerName}");
            OnPlayerNameChanged?.Invoke(this, true);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            OnPlayerNameChanged?.Invoke(this, false);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            OnPlayerNameChanged?.Invoke(this, false);
        }
    }
}
