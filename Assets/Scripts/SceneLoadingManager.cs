using System;
using Unity.Netcode;
using UnityEngine;

public class SceneLoadingManager : NetworkBehaviour
{
    public static SceneLoadingManager Instance;

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

    public event EventHandler<string> OnSceneLoadStarted;
    public event EventHandler OnSceneLoadCompleted;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public void LoadSceneAsync(string sceneName)
    {
        if (!IsServer || string.IsNullOrEmpty(sceneName)) return;

        NotifySceneLoadStartRpc(sceneName);
        var sceneLoadOperation = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogWarning($"Some clients timedout during scene load : {clientsTimedOut.Count}");
        }

        NotifySceneLoadCompleteRpc(sceneName);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifySceneLoadStartRpc(string sceneName)
    {
        OnSceneLoadStarted?.Invoke(this, $"Loading {sceneName}..");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifySceneLoadCompleteRpc(string sceneName)
    {
        OnSceneLoadCompleted?.Invoke(this, EventArgs.Empty);
    }
}
