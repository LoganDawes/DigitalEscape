using UnityEngine;

/*
 
    HeavyBox : Platforming
    Box that is too heavy to be moved by the player without heavy powerup.
 
 */

public class HeavyBox : Box
{
    // OnCollisionStay2D
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        // Check if colliding with player
        var player = collision.collider.GetComponent<PlayerController>();
        if (player != null)
        {
            // Only allow push if player has Heavy powerup
            if (player.GetPowerup() == PowerupType.Heavy)
            {
                // Let base Box handle normal push logic
                base.OnCollisionStay2D(collision);
            }
            else
            {
                // Zero out horizontal velocity if player tries to push
                var rb = GetComponent<Rigidbody2D>();
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            // Not a player, use base logic (e.g., moving platform)
            base.OnCollisionStay2D(collision);
        }
    }
}
