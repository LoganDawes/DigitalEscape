using UnityEngine;
using System.Collections;

/*
 
    PlayerController : Player
    Controls the player's movement, jumping, and interactions with the environment.
    Handles input and applies physics to the player character.
 
 */

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]

public class PlayerController : MonoBehaviour
{
    // Variables
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float dropTime = 0.5f;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isSneaking;
    private bool wasSneaking;
    private Vector2 originalColliderSize;
    private float originalJumpForce;
    private float previousVerticalVelocity;

    private MovingPlatform currentPlatform;
    private Vector2 lastPlatformVelocity;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    public LayerMask oneWayPlatformLayer;
    private LayerMask combinedGroundLayer;

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite sneakingSprite;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip sneakSound;
    public AudioClip unsneakSound;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D defaultCollider;
    private BoxCollider2D sneakingCollider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Start
    void Start()
    {
        // Initialize variables
        originalJumpForce = jumpForce;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        var colliders = GetComponents<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Variable & Component checks
        if (groundCheck == null)
        {
            Debug.LogError("Ground check transform not assigned to player object.");
        }
        if (groundLayer == 0)
        {
            Debug.LogError("Ground layer not assigned to player object.");
        }
        else if (oneWayPlatformLayer == 0)
        {
            Debug.LogError("One way platform layer not assigned to player object.");
        }
        else
        {
            // Combine ground and one-way platform layers
            combinedGroundLayer = groundLayer | oneWayPlatformLayer;
        }

        if (defaultSprite == null)
        {
            Debug.LogError("Default sprite not assigned to player object.");
        }
        if (sneakingSprite == null)
        {
            Debug.LogError("Sneaking sprite not assigned to player object.");
        }

        if (jumpSound == null)
        {
            Debug.LogWarning("Jump sound not assigned to player object.");
        }
        if (landSound == null)
        {
            Debug.LogWarning("Land sound not assigned to player object.");
        }
        if (sneakSound == null)
        {
            Debug.LogWarning("Sneak sound not assigned to player object.");
        }
        if (unsneakSound == null)
        {
            Debug.LogWarning("Unsneak sound not assigned to player object.");
        }
        if (colliders.Length < 2)
        {
            Debug.LogError("Player object must have two BoxCollider2D components (normal and sneaking).");
        }
        else
        {
            defaultCollider = colliders[0];
            sneakingCollider = colliders[1];
            defaultCollider.enabled = true;
            sneakingCollider.enabled = false;
        }
    }

    // Update
    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, combinedGroundLayer);

        // Landing detection
        if (!wasGrounded && isGrounded && previousVerticalVelocity < -5f)
        {
            audioSource.PlayOneShot(landSound);
        }
        wasGrounded = isGrounded;

        // Horizontal movement
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jumping
        if (isGrounded && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W)))
        {
            // Play jump sound
            audioSource.PlayOneShot(jumpSound);

            // Jump force
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        // Sneaking
        isSneaking = Input.GetKey(KeyCode.S);

        if (isSneaking != wasSneaking)
        {
            if (isSneaking)
            {
                // Set sprite for sneaking
                spriteRenderer.sprite = sneakingSprite;

                // Play sneak sound
                audioSource.PlayOneShot(sneakSound);

                // Set collider for sneaking
                defaultCollider.enabled = false;
                sneakingCollider.enabled = true;

                // Set jump force for hopping
                jumpForce = originalJumpForce / 2f;

                // Drop through platform
                StartCoroutine(DropThroughPlatform());

                // Set Audio Pitch higher
                audioSource.pitch = 1.3f;
            }
            else
            {
                // Check if there is enough room to stand up
                Vector2 checkPosition = (Vector2)transform.position + defaultCollider.offset;
                Vector2 checkSize = defaultCollider.size;

                // Slightly raise the check to avoid ground collision
                checkPosition.y += (defaultCollider.size.y - sneakingCollider.size.y) / 2f;

                // Check for ground collision in the sneaking position
                Collider2D hit = Physics2D.OverlapBox(
                    checkPosition,
                    checkSize,
                    0f,
                    combinedGroundLayer
                );

                // If there is no ground collision, allow unsneaking
                if (hit == null)
                {
                    // Reset sprite
                    spriteRenderer.sprite = defaultSprite;

                    // Reset Audio Pitch
                    audioSource.pitch = 1.0f;

                    // Play unsneak sound
                    audioSource.PlayOneShot(unsneakSound);

                    // Reset collider
                    defaultCollider.enabled = true;
                    sneakingCollider.enabled = false;

                    // Reset jump force
                    jumpForce = originalJumpForce;
                }
                else
                {
                    isSneaking = true;
                    return;
                }
            }

            // Update sneaking state
            wasSneaking = isSneaking;
        }

        // Store current vertical velocity
        previousVerticalVelocity = rb.linearVelocity.y;

    }

    // FixedUpdate
    void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            // Only apply horizontal platform velocity always
            Vector2 platformVel = currentPlatform.platformVelocity;
            rb.linearVelocity += new Vector2(platformVel.x, 0f);

            // Only apply vertical platform velocity if the player is grounded and not moving upwards (not jumping)
            if (isGrounded && rb.linearVelocity.y <= 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + platformVel.y);
            }

            lastPlatformVelocity = currentPlatform.platformVelocity;
        }
        else
        {
            lastPlatformVelocity = Vector2.zero;
        }

        // Reset for next frame
        currentPlatform = null;
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
                    break;
                }
            }
        }
    }

    // DropThroughPlatform
    IEnumerator DropThroughPlatform()
    {
        // Find all colliders on the platform layer
        Collider2D[] platforms = Physics2D.OverlapCircleAll(transform.position, 1.0f, oneWayPlatformLayer);

        foreach (var platform in platforms)
        {
            // Disable collision between player and platform
            Physics2D.IgnoreCollision(defaultCollider, platform, true);
            Physics2D.IgnoreCollision(sneakingCollider, platform, true);
        }

        // Wait for a short time before re-enabling collision
        yield return new WaitForSeconds(dropTime);

        foreach (var platform in platforms)
        {
            // Re-enable collision after delay
            Physics2D.IgnoreCollision(defaultCollider, platform, false);
            Physics2D.IgnoreCollision(sneakingCollider, platform, false);
        }
    }
}

