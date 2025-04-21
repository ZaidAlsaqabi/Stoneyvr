using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    private FadeScreen fadeScreen;
    
    void Start()
    {
        fadeScreen = FindObjectOfType<FadeScreen>();
        
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (PlayerPrefs.GetInt("CurrentScene", -1) != currentScene)
        {
            PlayerPrefs.SetInt("PreviousScene", PlayerPrefs.GetInt("CurrentScene", 0));
            PlayerPrefs.SetInt("CurrentScene", currentScene);
            PlayerPrefs.Save();
        }
        
        Debug.Log("Current scene: " + currentScene + ", Previous scene: " + PlayerPrefs.GetInt("PreviousScene", -1));
    }

    
    public void GoBack()
    {
        Debug.Log("Back button pressed");
        
        int previousScene = PlayerPrefs.GetInt("PreviousScene", -1);
        if (previousScene >= 0)
        {
            StartCoroutine(GoToSceneWithFade(previousScene));
        }
        else
        {
            Debug.Log("No previous scene recorded. Cannot go back.");
        }
    }
    
    private IEnumerator GoToSceneWithFade(int sceneIndex)
    {
        Debug.Log("Starting fade transition to scene: " + sceneIndex);
        
        if (fadeScreen != null && fadeScreen.gameObject.activeInHierarchy)
        {
            Debug.Log("Using FadeScreen transition with duration: " + fadeScreen.fadeDuration);
            
            fadeScreen.gameObject.SetActive(true);
            
            fadeScreen.FadeOut();
            
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
            
            Debug.Log("Fade complete, loading scene");
        }
        else
        {
            Debug.Log("No FadeScreen found for transition");
        }
        
        SceneManager.LoadScene(sceneIndex);
    }
} 