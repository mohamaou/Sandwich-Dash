using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    private void Awake()
    {
        var level = PlayerPrefs.GetInt("Level", 1);
        if (level >= SceneManager.sceneCountInBuildSettings)
        {
            level = Random.Range(0, SceneManager.sceneCountInBuildSettings);
        }
        SceneManager.LoadScene(level);
    }
}
