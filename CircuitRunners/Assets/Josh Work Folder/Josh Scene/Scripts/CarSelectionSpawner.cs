using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This needs to be on the Manager to function
public class CarSelectionSpawner : MonoBehaviour
{

    [Header("Car Section")]
    public GameObject[] cars;
    public Transform[] spawnpoint;
    [Header("Force car selection and debug")]
    public int car;

    

    // Start is called before the first frame update
    void Start()
    {
        if (DataContainer.Instance != null)
        {
            car = DataContainer.Instance.carChosen;
            Debug.Log("DataContainer found and car selected was " + car);
            Destroy(DataContainer.Instance.gameObject);
            //Vector3 pos = spawnpoint[car];
            Instantiate(cars[car]);
        }
        else
        {
            Debug.Log("ERROR 404: DataContainer not found");
        }


        


    }
}
