using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;// car
    public Transform mapPlane;// plane (map)
    public Transform cameraTarget, cameraTargetPivot;
    public float baseDistance = 10f;
    public float zoomFactor = 0.5f;
    public float cameraStep = 5f;

    public int min, max;

    float clampX;

    private Vector3 mapCenter;
    private float mapSize;

    void Start()
    {
        if (mapPlane != null)
        {
            Renderer r = mapPlane.GetComponent<Renderer>();
            if (r != null)
            {
                mapCenter = r.bounds.center;
                Vector3 size = r.bounds.size;
                mapSize = Mathf.Max(size.x, size.z); // largest dimension
            }
        }

        if (target == null) return;
        if (mapPlane != null)
        {
            Camera.main.orthographicSize = mapPlane.localScale.y * 2f;
        }

        float distance = baseDistance + (mapSize * zoomFactor);

        // fixed angled direction (isometric-like)
        Vector3 direction = Quaternion.Euler(30f, 45f, 0f) * Vector3.back;

        // position relative to map center, not car
        Vector3 desiredPosition = mapCenter + direction * distance;

        clampX = Mathf.Clamp(desiredPosition.x, min, max); // Clamp the x position
        desiredPosition = new Vector3(clampX, desiredPosition.y, desiredPosition.z); // Apply the clamped x position

        transform.position = desiredPosition; // set camera position
        cameraTarget.position = transform.position;
    }

    void LateUpdate()
    {
        cameraTargetPivot.position = target.position;
        float clampX = Mathf.Clamp(cameraTarget.position.x, min, max);

        Vector3 desiredPosition = new Vector3(clampX, cameraTarget.position.y, cameraTarget.position.z);
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, cameraStep * Time.deltaTime);
    }

}
