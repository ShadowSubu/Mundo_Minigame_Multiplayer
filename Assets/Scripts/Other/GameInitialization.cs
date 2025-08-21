using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitialization : MonoBehaviour
{
    [SerializeField] string lobbyScene;

    private void Start()
    {
        SceneManager.LoadSceneAsync(lobbyScene, LoadSceneMode.Single);
    }
}
