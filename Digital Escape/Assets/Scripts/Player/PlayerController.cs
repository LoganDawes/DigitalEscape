using UnityEngine;

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

    private bool isGrounded;
    private bool isSneaking;
    private bool isHopping;
    private Vector2 originalColliderSize;
    private float originalJumpForce;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite sneakingSprite;
    public Sprite hoppingSprite;
    public Sprite sneakingHoppingSprite;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D defaultCollider;
    private BoxCollider2D sneakingCollider;
    private SpriteRenderer spriteRenderer;

    // Start
    void Start()
    {
        // Initialize variables
        originalJumpForce = jumpForce;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        var colliders = GetComponents<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Variable & Component checks
        if (groundCheck == null)
        {
            Debug.LogError("Ground check transform not assigned in the inspector.");
        }
        if (groundLayer == 0)
        {
            Debug.LogError("Ground layer not assigned in the inspector.");
        }
        if (defaultSprite == null)
        {
            Debug.LogError("Default sprite not assigned in the inspector.");
        }
        if (sneakingSprite == null)
        {
            Debug.LogError("Sneaking sprite not assigned in the inspector.");
        }
        if (hoppingSprite == null)
        {
            Debug.LogError("Hopping sprite not assigned in the inspector.");
        }
        if (sneakingHoppingSprite == null)
        {
            Debug.LogError("Sneaking & hopping sprite not assigned in the inspector.");
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
    }

    // Update
    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Horizontal movement
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jumping
        if (isGrounded && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W)))
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        // Sneaking
        if (Input.GetKey(KeyCode.S))
        {
            isSneaking = true;

            // Set collider for sneaking
            defaultCollider.enabled = false;
            sneakingCollider.enabled = true;
        }
        else
        {
            isSneaking = false;

            // Reset collider
            defaultCollider.enabled = true;
            sneakingCollider.enabled = false;
        }

        // Hopping
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Toggle hopping state
            isHopping = !isHopping;

            if (isHopping)
            {
                // Set jump force for hopping
                jumpForce = originalJumpForce / 2f;
            }
            else
            {
                // Reset jump force
                jumpForce = originalJumpForce;
            }
        }

        // Sprite Control
        if (isHopping && isSneaking)
        {
            spriteRenderer.sprite = sneakingHoppingSprite;
        }
        else if (isHopping)
        {
            spriteRenderer.sprite = hoppingSprite;
        }
        else if (isSneaking)
        {
            spriteRenderer.sprite = sneakingSprite;
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }
}
