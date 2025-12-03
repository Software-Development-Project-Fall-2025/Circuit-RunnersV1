using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;


//This needs to be on the Manager to function
public class CarSelectionSpawner : MonoBehaviour
{

    [Header("Car Section")]
    public GameObject[] cars;
    public Transform[] spawnpoint;
    [Header("Force car selection and debug")]
    public int car;
 
    void Start()
    {
        if (DataContainer.Instance != null)
        {
            car = DataContainer.Instance.carChosen;
            Debug.Log("DataContainer found and car selected was " + car);
            Destroy(DataContainer.Instance.gameObject);
            Instantiate(cars[car], spawnpoint[car].position, spawnpoint[car].rotation);
            
        }
        else
        {
            Debug.Log("ERROR 404: DataContainer not found");
        }


        


    }
}
