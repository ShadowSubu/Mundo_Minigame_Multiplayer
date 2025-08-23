using System;
using TMPro;
using UnityEngine;

public class SceneLoaderUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        loadingPanel.SetActive(false);
    }

    private void Start()
    {
        SceneLoadingManager.Instance.OnSceneLoadStarted += SceneLoadingManager_OnSceneLoadStarted;
        SceneLoadingManager.Instance.OnSceneLoadCompleted += SceneLoadingManager_OnSceneLoadCompleted;
    }

    private void OnDestroy()
    {
        SceneLoadingManager.Instance.OnSceneLoadStarted -= SceneLoadingManager_OnSceneLoadStarted;
        SceneLoadingManager.Instance.OnSceneLoadCompleted -= SceneLoadingManager_OnSceneLoadCompleted;
    }

    private void SceneLoadingManager_OnSceneLoadStarted(object sender, string e)
    {
        ShowLoading(e);
    }

    private void SceneLoadingManager_OnSceneLoadCompleted(object sender, EventArgs e)
    {
        HideLoading();
    }

    private void ShowLoading(string message)
    {
        loadingPanel.SetActive(true);
        loadingText.text = message;
    }

    private void HideLoading()
    {
        loadingPanel.SetActive(false);
    }
}
