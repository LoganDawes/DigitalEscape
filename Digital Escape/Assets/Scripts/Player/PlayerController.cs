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
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float dropTime = 0.5f;
    [SerializeField] private float waterExitBoost = 12f;
    [SerializeField] private float maxHealth = 3;
    public float currentHealth;
    private float lastWaterY;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSneaking;
    private bool wasSneaking;
    private float originalJumpForce;
    private bool isInWater = false;
    private float previousVerticalVelocity;

    private MovingPlatform currentPlatform;

        [Header("Powerup")]
    [SerializeField] private PowerupType currentPowerup = PowerupType.None;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayPlatformLayer;
    private LayerMask combinedGroundLayer;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;
    private bool isKnockbacked = false;
    private float knockbackTimer = 0f;

    [Header("Knockback Recovery")]
    [SerializeField] private float knockbackRecoveryDuration = 1.0f;
    private float knockbackRecoveryTimer = 0f;

    [Header("Sprites")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite sneakingSprite;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip sneakSound;
    [SerializeField] private AudioClip unsneakSound;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D defaultCollider;
    private BoxCollider2D sneakingCollider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Start
    void Start()
    {
        Initialize();
        ComponentChecks();
    }

    private void Initialize()
    {
        // Initialize variables
        originalJumpForce = jumpForce;
        currentHealth = maxHealth;

        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        var colliders = GetComponents<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

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

    private void ComponentChecks()
    {
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
    }

    // Update
    void Update()
    {
        // Landing detection logic
        LandingDetection();

        // Detect knockback state
        if (Knockback())
        {
            previousVerticalVelocity = rb.linearVelocity.y;
            return;
        }

        // Smooth recovery after knockback
        if (knockbackRecoveryTimer > 0f)
        {
            float moveInput = Input.GetAxis("Horizontal");
            float targetXVel = moveInput * moveSpeed;
            float t = 1f - (knockbackRecoveryTimer / knockbackRecoveryDuration);
            if (isInWater)
            {
                WaterMovement(t, targetXVel);
            }
            else
            {
                // Interpolate x velocity, keep y
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetXVel, t), rb.linearVelocity.y);
            }
            knockbackRecoveryTimer -= Time.deltaTime;
        }
        else
        {
            // Movement logic
            if (isInWater)
            {
                WaterMovement();
            }
            else
            {
                // Normal movement
                float moveInput = Input.GetAxis("Horizontal");
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            }
        }

        // Jumping
        if (isGrounded && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W)))
        {
            audioSource.PlayOneShot(jumpSound);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        // Sneaking logic
        SneakingDetection();

        // Store previous vertical velocity for landing detection
        previousVerticalVelocity = rb.linearVelocity.y;
    }

    private void LandingDetection()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, combinedGroundLayer);

        // Landing detection
        if (!wasGrounded && isGrounded && previousVerticalVelocity < -5f)
        {
            audioSource.PlayOneShot(landSound);
        }
        wasGrounded = isGrounded;
    }

    private bool Knockback()
    {
        // Knockback timer
        if (isKnockbacked)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockbacked = false;
                knockbackRecoveryTimer = knockbackRecoveryDuration;
            }
            // Skip movement input while knockbacked
            // Return true to indicate knockback is active
            return true;
        }
        return false;
    }

    private void WaterMovement()
    {
        WaterMovement(-1f, 0f);
    }

    // t: interpolation factor (0-1), targetXVel: target horizontal velocity
    private void WaterMovement(float t, float targetXVel)
    {
        // Water movement: dampen horizontal, slow descent, allow upward movement
        float moveInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Horizontal movement dampened
        float waterMoveSpeed = moveSpeed * 0.5f;
        float xVel = moveInput * waterMoveSpeed;

        // Interpolate x velocity if t >= 0 (used during knockback recovery)
        if (t >= 0f && t <= 1f)
        {
            xVel = Mathf.Lerp(rb.linearVelocity.x, targetXVel * 0.5f, t);
        }

        // Vertical movement: holding up moves player upwards
        float yVel = rb.linearVelocity.y;
        if (verticalInput > 0f)
        {
            yVel = Mathf.Lerp(yVel, moveSpeed * 0.5f, 0.1f); // swim up
        }
        else if (yVel < 0)
        {
            yVel = Mathf.Lerp(yVel, -moveSpeed * 0.2f, 0.1f); // slow descent
        }

        rb.linearVelocity = new Vector2(xVel, yVel);
    }
    
    private void SneakingDetection()
    {
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
    }

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

    // FixedUpdate
    void FixedUpdate()
    {
        MovingPlatformVelocityAdjustment();
    }

    private void MovingPlatformVelocityAdjustment()
    {
        // Moving platform velocity adjustment
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
        }

        // Reset for next frame
        currentPlatform = null;
    }

    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            lastWaterY = transform.position.y;
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            // Only boost if exiting upwards
            float currentY = transform.position.y;
            if (currentY > lastWaterY)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, waterExitBoost);
            }
            isInWater = false;
        }
    }

    // OnCollisionEnter2D
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard"))
        {
            HazardCollision(collision, 1.0f);
        }
        else if (collision.collider.CompareTag("ExplosiveHazard"))
        {
            HazardCollision(collision, 2.0f);
        }
    }

    private void HazardCollision(Collision2D hazard, float magnitude)
    {
        // Lose one health
        currentHealth = Mathf.Max(0, currentHealth - 1);

        // Determine knockback direction based on contact normal
        Vector2 knockbackDir = Vector2.zero;
        if (hazard.contactCount > 0)
        {
            // Use the first contact normal
            Vector2 normal = hazard.GetContact(0).normal;
            float absX = Mathf.Abs(normal.x);
            float absY = Mathf.Abs(normal.y);
            if (absY > absX)
            {
                // Vertical knockback (up or down)
                knockbackDir = new Vector2(0f, Mathf.Sign(normal.y));
            }
            else
            {
                // Horizontal knockback (left or right)
                knockbackDir = new Vector2(Mathf.Sign(normal.x), 0f);
            }
        }
        else
        {
            // Fallback to center-to-center
            Vector2 collisionPos = hazard.transform.position;
            Vector2 playerPos = transform.position;
            Vector2 diff = (playerPos - collisionPos).normalized;
            float absX = Mathf.Abs(diff.x);
            float absY = Mathf.Abs(diff.y);
            if (absY > absX)
            {
                knockbackDir = new Vector2(0f, Mathf.Sign(diff.y));
            }
            else
            {
                knockbackDir = new Vector2(Mathf.Sign(diff.x), 0f);
            }
        }

        rb.AddForce(knockbackDir * knockbackForce * magnitude, ForceMode2D.Impulse);

        // Disable movement for knockback duration
        isKnockbacked = true;
        knockbackTimer = knockbackDuration;
    }

    // OnCollisionStay2D
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("MovingPlatform"))
        {
            MovingPlatformCollision(collision);
        }
    }

    private void MovingPlatformCollision(Collision2D collision)
    {
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

    public bool SetPowerup(PowerupType type)
    {
        if (currentPowerup == PowerupType.None && type != PowerupType.None)
        {
            currentPowerup = type;
            return true;
        }
        return false;
    }

    public PowerupType GetPowerup()
    {
        return currentPowerup;
    }
}

