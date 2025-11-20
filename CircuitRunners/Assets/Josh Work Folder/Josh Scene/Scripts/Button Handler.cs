using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    [Header("Button if Needed")]
    public List<Button> startButton;

    [Header("Later Use")]
    public List<GameObject> hiddenItem;
    
    public void GoToSceneName(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    public void ButtonColorGreen(Button MainButton)
    {
        MainButton.image.color = Color.green;
    }

    public void ButtonColorWhite(Button MainButton)
    {
        MainButton.image.color = Color.white;
    }


    public void GoToSceneNameGiven(string sceneName)
    {
        startButton[0].gameObject.SetActive(true);
        startButton[1].gameObject.SetActive(true);
        startButton[0].onClick.AddListener(() =>
        {
            SceneManager.LoadScene(sceneName);
        });

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

    //Test a button works
    public void LogClick(string button)
    {
        Debug.Log(button + " was clicked.");

    }

    //Exit is here for self build testing
    public void Quit(){
        Application.Quit();
        Debug.Log("System has Exited...");
    }

    
}
