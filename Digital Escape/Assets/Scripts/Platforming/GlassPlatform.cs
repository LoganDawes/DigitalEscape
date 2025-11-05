using UnityEngine;

/*
 
    GlassPlatform : Platforming
    Can be broken by the player using the heavy slam.
 
 */

public class GlassPlatform : Platform
{
    // Variables
    [SerializeField] private float breakVelocityThreshold = -30f; // Lowered threshold for better tuning

    // Start
    void Start()
    {

    }

    // Update
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.collider.attachedRigidbody;
            if (playerRb != null)
            {
                // Use the minimum of player's y velocity and collision relative y velocity
                float impactVelocity = Mathf.Min(playerRb.linearVelocity.y, collision.relativeVelocity.y);

                // Check if the player is moving downwards fast enough
                if (impactVelocity <= breakVelocityThreshold)
                {
                    // Disable collider immediately to prevent stutter
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                    // Destroy at end of frame to ensure player passes through
                    Destroy(gameObject, 0f);
                }
            }
        }
    }
}
