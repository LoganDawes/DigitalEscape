using UnityEngine;

/*
 
    Box : Platforming
    Pushable box that can be moved by the player or other objects.
 
 */

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class Box : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private MovingPlatform currentPlatform;

    // Start
    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
    }

    // FixedUpdate
    void FixedUpdate()
    {
        // If on a moving platform, inherit its velocity
        if (currentPlatform != null)
        {
            rb.linearVelocity += currentPlatform.platformVelocity;
        }
    }

    // OnCollisionStay2D
    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if standing on a MovingPlatform
        var platform = collision.collider.GetComponent<MovingPlatform>();
        if (platform != null)
        {
            foreach (var contact in collision.contacts)
            {
                // Only consider contacts from below (standing on top)
                if (contact.normal.y > 0.5f)
                {
                    currentPlatform = platform;
                    return;
                }
            }
        }
        else
        {
            currentPlatform = null;
        }
    }

    // OnCollisionExit2D
    void OnCollisionExit2D(Collision2D collision)
    {
        // Reset platform reference when leaving platform
        if (collision.collider.GetComponent<MovingPlatform>() != null)
        {
            currentPlatform = null;
        }
    }
}
