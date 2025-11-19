using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
    MovingPlatform : Platforming
    Platform that either moves constantly in a set path or is moved by a button press.
 
 */

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class MovingPlatform : Platform, IActivatable
{
    // Variables
    [Header("Moving Platform Settings")]
    [SerializeField] private List<Transform> trackPoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;

    [SerializeField] private bool isActive = true;

    [HideInInspector]
    public Vector2 platformVelocity;

    private Vector2 moveTarget;
    private List<Vector3> worldTrackPoints = new List<Vector3>();
    private bool isMoving = true;
    private bool isBlocked = false;
    private int currentPointIndex = 0;
    private Vector2 previousPosition;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D platformCollider;

    // Start
    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<BoxCollider2D>();

        // Set initial state of moving
        isMoving = isActive;

        // Store world positions of track points
        worldTrackPoints.Clear();
        foreach (var t in trackPoints)
        {
            worldTrackPoints.Add(t.position);
        }

        if (worldTrackPoints.Count > 0)
            transform.position = worldTrackPoints[0];
        previousPosition = rb.position;
        StartCoroutine(MoveRoutine());
    }

    // MoveRoutine
    IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (!isActive || isBlocked || worldTrackPoints.Count < 2)
            {
                isMoving = false;
                yield return null;
                continue;
            }

            Vector2 target = worldTrackPoints[currentPointIndex];

            // Move towards the target point
            while (Vector2.Distance(transform.position, target) > 0.05f)
            {
                if (!isActive || isBlocked)
                {
                    isMoving = false;
                    yield return null;
                    continue;
                }

                moveTarget = target;
                isMoving = true;
                yield return null;
            }

            // Snap to the target point and stop
            isMoving = false;
            rb.MovePosition(target);
            transform.position = target;

            // Determine if this is a midpoint or a normal trackpoint
            Transform currentTrackTransform = trackPoints[currentPointIndex];
            bool isMidpoint = currentTrackTransform != null && currentTrackTransform.CompareTag("Midpoint");

            // Wait only if not a midpoint
            if (!isMidpoint)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // Move to the next point
            currentPointIndex = (currentPointIndex + 1) % worldTrackPoints.Count;
        }
    }

    // FixedUpdate
    void FixedUpdate()
    {
        Vector2 newPosition = rb.position;
        if (isMoving && !isBlocked)
        {
            newPosition = Vector2.MoveTowards(rb.position, moveTarget, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }

        // Calculate velocity based on position change
        platformVelocity = (newPosition - previousPosition) / Time.fixedDeltaTime;
        previousPosition = newPosition;
    }

    // OnActivated
    public void onActivated()
    {
        isActive = !isActive;
    }

    // OnCollisionEnter2D
    void OnCollisionEnter2D(Collision2D collision)
    {
        // If the platform collides with something, check for blocking movement
        if (collision.collider.attachedRigidbody != null)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;
                Vector2 velocity = (moveTarget - rb.position).normalized;
                if (Vector2.Dot(velocity, -normal) > 0.9f)
                {
                    isBlocked = true;
                    break;
                }
            }
        }
    }

    // OnCollisionExit2D
    void OnCollisionExit2D(Collision2D collision)
    {
        // If the platform stops colliding with something, allow movement again
        isBlocked = false;
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

        // Preview the platform at each track point
        var box = GetComponent<BoxCollider2D>();

        Vector3 size = new Vector3(box.size.x * Mathf.Abs(transform.localScale.x), box.size.y * Mathf.Abs(transform.localScale.y), 1f);
        Vector3 offset = (Vector3)box.offset;

        foreach (var t in trackPoints)
        {
            if (t != null)
            {
                // Save the current matrix
                Matrix4x4 oldMatrix = Gizmos.matrix;

                // Apply position, rotation, and scale
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


