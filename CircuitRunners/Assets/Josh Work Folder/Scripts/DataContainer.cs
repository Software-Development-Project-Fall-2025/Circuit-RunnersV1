using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    // The Singleton Instance: allows access from anywhere.
    public static DataContainer Instance;

    // The data variable to be carried between scenes.
    // We make it public so other scripts can access and change it.
    public int carChosen = 0;

    private void Awake()
    {
        // --- Singleton Setup ---
        if (Instance == null)
        {
            //Single instance creator
            Instance = this;

            //DO NOT REMOVE OR IT BREAKS ~Josh
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //Dup Object Killer
            Destroy(this.gameObject);
        }
    }
}
