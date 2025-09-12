using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;// car
    public Transform mapPlane;// plane (map)
    public float baseDistance = 10f;
    public float zoomFactor = 0.5f;

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
    }

    void LateUpdate()
    {
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

        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }
}
