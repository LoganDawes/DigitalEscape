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
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float zoom = 5f;
    [SerializeField] private float tuningFactor = 0.4f;
    [SerializeField] private float maxScale = 10f;

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

        Vector3 desiredPosition;
        float desiredZoom = zoom;

        // Check if target is a PlayerController with a clone
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null && player.hasClone && player.cloneInstance != null)
        {
            // Get positions of player and clone
            Vector3 playerPos = player.transform.position;
            Vector3 clonePos = player.cloneInstance.transform.position;

            // Center camera between player and clone
            Vector3 midpoint = (playerPos + clonePos) / 2f;
            desiredPosition = midpoint + offset;

            // Adjust zoom to fit both in view
            float distance = Vector3.Distance(playerPos, clonePos);
            desiredZoom = Mathf.Max(zoom, distance * tuningFactor);
        }
        else
        {
            // Get the desired position
            desiredPosition = target.position + offset;
        }

        // Clamp desiredZoom to maxScale
        desiredZoom = Mathf.Min(desiredZoom, maxScale);

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera position
        transform.position = smoothedPosition;

        // Set the camera zoom
        cam.orthographicSize = desiredZoom;
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
