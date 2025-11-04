using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public float bounceForce = 1000f;    
    public float upwardForce = 200f;     
    public string[] collisionTags = { "Wall", "Car" };  

    private Rigidbody rb;
    private CarController carController;  

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
    }

    void OnCollisionEnter(Collision collision) {
        //CarController carController = collision.gameObject.GetComponent<CarController>();

        Debug.Log("SHOULD BE ENTERING COLLISION");
        bool isValidCollision = false;
        foreach (string tag in collisionTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isValidCollision = true;
                break;
            }
            else {
                Debug.Log("WHY THIS NO WORK");
            }
        }

        if (!isValidCollision) return;
        if (carController != null){
            if(carController.health >= 10){
                carController.health -= 10;
            }
            else if(carController.health < 10) {
                carController.health -= carController.health;
            }
            else {
                return;
            }
        }
        Debug.Log("Health: " + carController.health);
        // Get the car's forward direction and reverse it
        Vector3 carForward = transform.forward;
        Vector3 violentlyBackwards = -carForward; 
        
        // Reset velocities
        rb.velocity = Vector3.zero;  
        rb.angularVelocity = Vector3.zero;  

        float crashForce = 2000f;  
        rb.AddForce(violentlyBackwards * crashForce, ForceMode.Impulse);  

        
    }
}
