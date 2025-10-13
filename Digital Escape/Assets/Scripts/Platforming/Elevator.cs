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
    public List<Transform> trackPoints = new List<Transform>();
    public float moveSpeed = 2f;

    [Header("Collision Layers")]
    public string groundLayerName = "Ground";
    public string oneWayPlatformLayerName = "OneWayPlatform";

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

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D[] borderColliders = new BoxCollider2D[4]; // left, right, top, bottom
    private BoxCollider2D elevatorTrigger;

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
    }

    // MoveRoutine
    IEnumerator MoveRoutine()
    {
        // Move to next track point only when activated
        isMoving = true;
        // Enable border colliders
        foreach (var c in borderColliders)
            if (c != null) c.enabled = true;
        
        // Ignore collisions with Ground and OneWayPlatform layers for all players inside elevator
        var players = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.CompareTag("Player"))
            {
                player.transform.SetParent(transform);
                var playerColliders = player.GetComponents<BoxCollider2D>();
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
        }
        Vector2 target = worldTrackPoints[currentPointIndex];
        moveTarget = target;
        while (Vector2.Distance(transform.position, target) > 0.05f)
        {
            isMoving = true;
            yield return null;
        }
        // Snap to the target point and stop
        rb.MovePosition(target);
        transform.position = target;
        isMoving = false;
        // Disable border colliders
        foreach (var c in borderColliders)
            if (c != null) c.enabled = false;
        
        // Restore collisions with Ground and OneWayPlatform layers for all players
        foreach (var player in players)
        {
            if (player.CompareTag("Player"))
            {
                player.transform.SetParent(null);
                var playerColliders = player.GetComponents<BoxCollider2D>();
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
        // Move to next track point
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
            playerInRange = true;
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

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
                Gizmos.DrawWireCube(offset, size);

                // Restore the matrix
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}
