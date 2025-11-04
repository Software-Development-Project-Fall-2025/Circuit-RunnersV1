using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilCarController : MonoBehaviour
{
    public Rigidbody sphereRB;

    // Start is called before the first frame update
    void Start(){
        sphereRB.transform.parent = null;
    }

    // Update is called once per frame
    void Update(){
        
    }
}
