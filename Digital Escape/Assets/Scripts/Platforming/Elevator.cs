using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
    Elevator : Platforming
    When interacted with locks the player in the box, moves up or down, then unlocks the player.
 
 */
 
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class Elevator : MonoBehaviour, IActivatable
{
    // Variables
    [Header("Elevator Settings")]
    [SerializeField] private List<Transform> trackPoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 2f;

    [Header("Collision Layers")]
    [SerializeField] private string groundLayerName = "Ground";
    [SerializeField] private string oneWayPlatformLayerName = "OneWayPlatform";

    private int groundLayer;
    private int oneWayPlatformLayer;

    [HideInInspector]
    public Vector2 elevatorVelocity;

    private Vector2 moveTarget;
    private List<Vector3> worldTrackPoints = new List<Vector3>();
    private bool isMoving = false;
    private int currentPointIndex = 0;
    private Vector2 previousPosition;
    private bool playerInRange = false;
    private bool cloneInRange = false;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D[] borderColliders = new BoxCollider2D[4]; // left, right, top, bottom
    private BoxCollider2D elevatorTrigger;
    private GameObject elevatorGroundDetector;
    private GameObject activatingPlayer;

    // Start
    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        var colliders = GetComponents<BoxCollider2D>();
        if (colliders.Length < 5)
        {
            Debug.LogWarning("Elevator object must have 5 BoxCollider2D components (4 borders, 1 trigger).");
        }
        for (int i = 0; i < 4; i++)
        {
            borderColliders[i] = colliders.Length > i ? colliders[i] : null;
            if (borderColliders[i] != null)
                borderColliders[i].enabled = false;
        }
        elevatorTrigger = colliders.Length > 4 ? colliders[4] : null;
        if (elevatorTrigger != null)
            elevatorTrigger.isTrigger = true;

        groundLayer = LayerMask.NameToLayer(groundLayerName);
        oneWayPlatformLayer = LayerMask.NameToLayer(oneWayPlatformLayerName);

        // Set initial state of moving
        isMoving = false;

        // Store world positions of track points
        worldTrackPoints.Clear();
        foreach (var t in trackPoints)
        {
            worldTrackPoints.Add(t.position);
        }

        if (worldTrackPoints.Count > 0)
            transform.position = worldTrackPoints[0];
        previousPosition = rb.position;

        // Find the child GameObject named "elevatorGroundDetector"
        Transform detectorTransform = transform.Find("elevatorGroundDetector");
        if (detectorTransform != null)
        {
            elevatorGroundDetector = detectorTransform.gameObject;
            elevatorGroundDetector.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Elevator: Child GameObject 'elevatorGroundDetector' not found.");
        }
    }

    // MoveRoutine
    IEnumerator MoveRoutine()
    {
        isMoving = true;
        foreach (var c in borderColliders)
            if (c != null) c.enabled = true;

        if (activatingPlayer != null)
        {
            activatingPlayer.transform.SetParent(transform);
            var playerColliders = activatingPlayer.GetComponents<BoxCollider2D>();
            var allColliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            foreach (var pc in playerColliders)
            {
                foreach (var col in allColliders)
                {
                    if (col.gameObject.layer == groundLayer || col.gameObject.layer == oneWayPlatformLayer)
                    {
                        Physics2D.IgnoreCollision(pc, col, true);
                    }
                }
            }
        }

        // Elevator movement loop
        while (true)
        {
            Vector2 target = worldTrackPoints[currentPointIndex];
            moveTarget = target;

            if (elevatorGroundDetector != null)
                elevatorGroundDetector.SetActive(true);

            // Move towards the target point
            while (Vector2.Distance(transform.position, target) > 0.05f)
            {
                isMoving = true;
                yield return null;
            }

            rb.MovePosition(target);
            transform.position = target;

            // Check if this track point is a midpoint
            Transform currentTrackTransform = trackPoints[currentPointIndex];
            bool isMidpoint = currentTrackTransform != null && currentTrackTransform.CompareTag("Midpoint");

            if (!isMidpoint)
            {
                // Stop at normal trackpoint
                isMoving = false;
                if (elevatorGroundDetector != null)
                    elevatorGroundDetector.SetActive(false);

                foreach (var c in borderColliders)
                    if (c != null) c.enabled = false;

                if (activatingPlayer != null)
                {
                    activatingPlayer.transform.SetParent(null);
                    var playerColliders = activatingPlayer.GetComponents<BoxCollider2D>();
                    var allColliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
                    foreach (var pc in playerColliders)
                    {
                        foreach (var col in allColliders)
                        {
                            if (col.gameObject.layer == groundLayer || col.gameObject.layer == oneWayPlatformLayer)
                            {
                                Physics2D.IgnoreCollision(pc, col, false);
                            }
                        }
                    }
                    activatingPlayer = null;
                }
                break;
            }
            else
            {
                // If midpoint, immediately go to next trackpoint
                currentPointIndex = (currentPointIndex + 1) % worldTrackPoints.Count;
            }
        }
    }

    // Update
    void Update()
    {
        // Player interaction with E
        if (playerInRange && !isMoving && Input.GetKeyDown(KeyCode.E))
        {
            onActivated();
        }
        // Clone interaction with F
        if (cloneInRange && !isMoving && Input.GetKeyDown(KeyCode.F))
        {
            onActivated();
        }
    }

    // FixedUpdate
    void FixedUpdate()
    {
        Vector2 newPosition = rb.position;
        if (isMoving)
        {
            newPosition = Vector2.MoveTowards(rb.position, moveTarget, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }

        // Calculate velocity based on position change
        elevatorVelocity = (newPosition - previousPosition) / Time.fixedDeltaTime;
        previousPosition = newPosition;
    }

    // OnActivated
    public void onActivated()
    {
        if (isMoving || worldTrackPoints.Count < 2)
            return;

        // Find the activating player (in range)
        GameObject playerToActivate = null;
        var players = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.CompareTag("Player"))
            {
                var pc = player.GetComponent<PlayerController>();
                if ((playerInRange && pc != null && !pc.isClone) ||
                    (cloneInRange && pc != null && pc.isClone))
                {
                    playerToActivate = player;
                    break;
                }
            }
        }
        activatingPlayer = playerToActivate;

        currentPointIndex = (currentPointIndex + 1) % worldTrackPoints.Count;
        StartCoroutine(MoveRoutine());
    }

    // OnCollisionEnter2D
    void OnCollisionEnter2D(Collision2D collision)
    {
        // If the elevator collides with something, check for blocking movement
        if (collision.collider.attachedRigidbody != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;
                Vector2 velocity = (moveTarget - rb.position).normalized;
                if (Vector2.Dot(velocity, -normal) > 0.9f)
                {
                    break;
                }
            }
        }
    }

    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.isClone)
            {
                cloneInRange = true;
            }
            else
            {
                playerInRange = true;
            }
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.isClone)
            {
                cloneInRange = false;
            }
            else
            {
                playerInRange = false;
            }
        }
    }

# if UNITY_EDITOR
    // OnDrawGizmos
    void OnDrawGizmos()
    {
        if (trackPoints == null || trackPoints.Count < 2)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < trackPoints.Count - 1; i++)
        {
            if (trackPoints[i] != null && trackPoints[i + 1] != null)
            {
                Gizmos.DrawLine(trackPoints[i].position, trackPoints[i + 1].position);
            }
        }

        // Draw line from last to first to close the loop
        if (trackPoints[0] != null && trackPoints[trackPoints.Count - 1] != null)
        {
            Gizmos.DrawLine(trackPoints[trackPoints.Count - 1].position, trackPoints[0].position);
        }

        // Preview the elevator at each track point using SpriteRenderer size
        var spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 size = Vector3.one;
        Vector3 offset = Vector3.zero;
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            size = spriteRenderer.sprite.bounds.size;
            offset = spriteRenderer.sprite.bounds.center;
        }
        foreach (var t in trackPoints)
        {
            if (t != null)
            {
                // Save the current matrix
                Matrix4x4 oldMatrix = Gizmos.matrix;

                // Apply position, rotation, and object's scale
                Gizmos.matrix = Matrix4x4.TRS(t.position, transform.rotation, transform.lossyScale);

                // Set color based on tag
                if (t.CompareTag("Midpoint"))
                    Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // orange
                else
                    Gizmos.color = Color.cyan;

                Gizmos.DrawWireCube(offset, size);

                // Restore the matrix
                Gizmos.matrix = oldMatrix;
            }
        }
    }
#endif
}
