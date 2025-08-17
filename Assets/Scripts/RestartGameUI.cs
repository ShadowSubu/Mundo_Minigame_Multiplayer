using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RestartGameUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;

    private void OnEnable()
    {
        restartButton.onClick.AddListener(RestartGame);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(RestartGame);
    }

    private void Start()
    {
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Show();
        }
    }

    public void Show()
    {
        // show all the child game objects
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        // hide all the child game objects
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void RestartGame()
    {
        GameManager.Instance.RestartGame();
        Hide();
    }
}
