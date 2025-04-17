using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        switch(other.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                LoadNextLevel();
                break;
            default:
                ReloadLevel();
                break;
        }

        void LoadNextLevel()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;

            if(nextScene == SceneManager.sceneCountInBuildSettings)
            {
                nextScene = 0;
            }

            SceneManager.LoadScene(nextScene);
        }

        void ReloadLevel()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene);
        }
    }
}
