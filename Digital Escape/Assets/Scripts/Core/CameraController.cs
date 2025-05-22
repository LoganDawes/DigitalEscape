using UnityEngine;

/*

    CameraController : Core
    Controls the camera position, smoothing, and zoom level.
    Can change the target to follow a different object.

 */

public class CameraController : MonoBehaviour
{
    // Variables
    [Header("Camera Settings")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float zoom = 5f;

    // Components
    private Camera cam;

    // Instance
    private static CameraController instance;

    // Awake
    private void Awake()
    {
        // Initialize components
        cam = GetComponent<Camera>();

        // Variable & Component checks
        if (cam == null)
        {
            Debug.LogError("Camera component not found on the camera object.");
        }

        // Creates one instance of the camera controller
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Late Update
    void LateUpdate()
    {
        // Check if the target is assigned
        if (target == null)
        {
            return;
        }

        // Get the desired position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera position
        transform.position = smoothedPosition;

        // Set the camera zoom
        cam.orthographicSize = zoom;
    }

    // Set Target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
