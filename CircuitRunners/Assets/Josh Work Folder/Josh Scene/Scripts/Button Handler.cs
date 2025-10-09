using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    public void GoToSceneName(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    public void GoToSceneID(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }
    public void ToggleObjectFalse(GameObject f) 
    {
        f.SetActive(false);
    }

    public void ToggleObjectTrue(GameObject t)
    {
        t.SetActive(true);
    }
    //Exit is here for self build testing
    public void Quit(){
        Application.Quit();
        Debug.Log("System has Exited...");
    }

    
}
