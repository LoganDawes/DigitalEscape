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

    // For smooth transition when clone disappears
    private Vector3 lastMidpoint;
    private float lastZoom;
    private bool wasTrackingClone;
    private float transitionTimer;
    private readonly float transitionDuration = 0.5f; // seconds

    // Awake
    private void Awake()
    {
        gameObject.SetActive(true); // Ensure object is enabled
        
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
            // Register with GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.RegisterCamera(this);
            }
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
        bool trackingClone = false;
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

            // Store last midpoint and zoom for smooth transition
            lastMidpoint = midpoint;
            lastZoom = desiredZoom;
            trackingClone = true;
            transitionTimer = 0f; // reset timer if clone exists
        }
        else
        {
            // If we were tracking a clone last frame, smoothly transition back to player
            if (wasTrackingClone)
            {
                transitionTimer += Time.deltaTime;
                float t = Mathf.Clamp01(transitionTimer / transitionDuration);

                Vector3 playerPos = target.position;
                Vector3 targetPosition = playerPos + offset;
                desiredPosition = Vector3.Lerp(lastMidpoint + offset, targetPosition, t);
                desiredZoom = Mathf.Lerp(lastZoom, zoom, t);

                // If transition finished, snap to player and stop tracking
                if (t >= 1f)
                {
                    desiredPosition = targetPosition;
                    desiredZoom = zoom;
                    wasTrackingClone = false;
                }
            }
            else
            {
                // Get the desired position
                desiredPosition = target.position + offset;
                desiredZoom = zoom;
            }
        }

        // Clamp desiredZoom to maxScale
        desiredZoom = Mathf.Min(desiredZoom, maxScale);

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera position
        transform.position = smoothedPosition;

        // Set the camera zoom
        cam.orthographicSize = desiredZoom;

        // Update tracking state
        wasTrackingClone = trackingClone || wasTrackingClone;
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
