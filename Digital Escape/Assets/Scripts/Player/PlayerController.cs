using UnityEngine;
using System.Collections;

/*
 
    PlayerController : Player
    Controls the player's movement, jumping, and interactions with the environment.
    Handles input and applies physics to the player character.
 
 */

public class PlayerController : MonoBehaviour
{
    // Variables
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public float dropTime = 0.5f;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isJumping;
    private bool isSneaking;
    private bool wasSneaking;
    private Vector2 originalColliderSize;
    private float originalJumpForce;

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
        isJumping = false;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        var colliders = GetComponents<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Variable & Component checks
        if (groundCheck == null)
        {
            Debug.LogError("Ground check transform not assigned in the inspector.");
        }
        if (groundLayer == 0)
        {
            Debug.LogError("Ground layer not assigned in the inspector.");
        }
        else if (oneWayPlatformLayer == 0){
            Debug.LogError("One way platform layer not assigned in the inspector.");
        }
        else
        {
            // Combine ground and one-way platform layers
            combinedGroundLayer = groundLayer | oneWayPlatformLayer;
        }

        if (defaultSprite == null)
        {
            Debug.LogError("Default sprite not assigned in the inspector.");
        }
        if (sneakingSprite == null)
        {
            Debug.LogError("Sneaking sprite not assigned in the inspector.");
        }

        if (jumpSound == null)
        {
            Debug.LogWarning("Jump sound not assigned in the inspector.");
        }
        if (landSound == null)
        {
            Debug.LogWarning("Land sound not assigned in the inspector.");
        }
        if (sneakSound == null)
        {
            Debug.LogWarning("Sneak sound not assigned in the inspector.");
        }
        if (unsneakSound == null)
        {
            Debug.LogWarning("Unsneak sound not assigned in the inspector.");
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the player object.");
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
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the player object.");
        }
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on the player object.");
        }
    }

    // Update
    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, combinedGroundLayer);

        // Landing detection
        if (!wasGrounded && isGrounded && rb.linearVelocity.y < 0.1f)
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
            isJumping = true;

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

            // Update sneaking state
            wasSneaking = isSneaking;
        }
    }

    IEnumerator DropThroughPlatform()
    {
        // Find all colliders on the platform layer
        Collider2D[] platforms = Physics2D.OverlapCircleAll(transform.position, 0.5f, oneWayPlatformLayer);

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
