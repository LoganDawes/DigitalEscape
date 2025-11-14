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
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float dropTime = 0.5f;
    [SerializeField] private float waterExitBoost = 10f;

    [Header("Health Settings")]
    public float maxHealth = 3;
    public float currentHealth;
    
    // Player state
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSneaking;
    private bool wasSneaking;
    private bool isInWater = false;
    private float lastWaterY;

    // Original stats
    private Vector3 originalScale;
    private float originalJumpForce;
    private float originalGravityScale;
    private float previousVerticalVelocity;

    // Platform velocity interaction
    private MovingPlatform currentPlatform;
    private Elevator currentElevator;

    [Header("Powerup")]
    [SerializeField] private PowerupType currentPowerup = PowerupType.None;
    [SerializeField] private float heavyJumpForceMultiplier = 0.8f;
    [SerializeField] private float heavySneakGravityScale = 40f;
    [SerializeField] private float shrinkScale = 0.5f;
    [SerializeField] private float shrinkColliderScale = 0.95f;
    [SerializeField] private float shrinkJumpForceMultiplier = 0.7f;
    [SerializeField] private GameObject clonePrefab;
    private bool heavySneakActive = false;
    private bool isShrunk = false;
    public bool isClone = false;
    public bool hasClone = false;
    [HideInInspector] public GameObject cloneInstance;
    [HideInInspector] public PlayerController cloneOwnerInstance;
    public PowerupType cloneCollectedPowerup = PowerupType.None;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayPlatformLayer;
    private LayerMask combinedGroundLayer;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float knockbackDuration = 0.2f;
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

    [Header("Shrink Phantom Effect")]
    [SerializeField] private int shrinkPhantomCount = 3;
    [SerializeField] private float shrinkPhantomDuration = 0.3f;
    [SerializeField] private float shrinkPhantomInterval = 0.05f;
    [SerializeField] private Color shrinkPhantomColor = new Color(1f, 1f, 1f, 0.5f);

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
        DontDestroyOnLoad(gameObject);
        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterPlayer(this);
        }
    }

    private void Initialize()
    {
        // Initialize variables
        originalScale = transform.localScale;
        originalJumpForce = jumpForce;
        originalGravityScale = GetComponent<Rigidbody2D>().gravityScale;
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
            float moveInput = isClone ? Input.GetAxisRaw("HorizontalClone") : Input.GetAxis("Horizontal");
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
                WaterMovement(-1f, 0f);
            }
            else
            {
                // Normal movement
                float moveInput = isClone ? Input.GetAxisRaw("HorizontalClone") : Input.GetAxis("Horizontal");
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            }
        }
        
        // Jumping
        UpdateJumpForce();
        bool jumpPressed = isClone ? Input.GetKeyDown(KeyCode.UpArrow) : (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W));
        if (isGrounded && jumpPressed)
        {
            audioSource.PlayOneShot(jumpSound);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        // Sneaking logic
        SneakingDetection();

        // Store previous vertical velocity for landing detection
        previousVerticalVelocity = rb.linearVelocity.y;

        // Powerup activation on Z press
        bool powerupPressed = isClone ? Input.GetKeyDown(KeyCode.X) : Input.GetKeyDown(KeyCode.Z);
        if (powerupPressed)
        {
            PowerupActivate();
        }
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

    // t: interpolation factor (0-1), targetXVel: target horizontal velocity
    private void WaterMovement(float t, float targetXVel)
    {
        // Water movement: dampen horizontal, slow descent, allow upward movement
        float moveInput = isClone ? Input.GetAxisRaw("HorizontalClone") : Input.GetAxis("Horizontal");
        float verticalInput = isClone ? Input.GetAxisRaw("VerticalClone") : Input.GetAxis("Vertical");

        // Swim powerup: full directional movement in water
        if (currentPowerup == PowerupType.Swim)
        {
            float swimSpeed = moveSpeed;
            float swimXVel = moveInput * swimSpeed;
            float swimYVel = verticalInput * swimSpeed;
            rb.linearVelocity = new Vector2(swimXVel, swimYVel);
            return;
        }

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
        if (currentPowerup == PowerupType.Heavy)
        {
            // Prevent upward movement in water when Heavy powerup is active
            if (yVel < 0)
            {
                yVel = Mathf.Lerp(yVel, -moveSpeed * 0.2f, 0.1f); // slow descent
            }
        }
        else
        {
            if (verticalInput > 0f)
            {
                yVel = Mathf.Lerp(yVel, moveSpeed * 0.5f, 0.1f); // swim up
            }
            else if (yVel < 0)
            {
                yVel = Mathf.Lerp(yVel, -moveSpeed * 0.2f, 0.1f); // slow descent
            }
        }

        rb.linearVelocity = new Vector2(xVel, yVel);
    }

    private void UpdateJumpForce()
    {
        if (currentPowerup == PowerupType.Heavy)
        {
            jumpForce = originalJumpForce * heavyJumpForceMultiplier;
        }
        else
        {
            float scale = 1f;
            if (isShrunk)
            {
                scale *= shrinkJumpForceMultiplier;
            }
            if (isSneaking)
            {
                scale *= 0.5f;
            }
            jumpForce = originalJumpForce * scale;
        }
    }
    
    private void SneakingDetection()
    {
        // Prevent sneaking in water with Swim powerup unless already sneaking
        bool sneakInput = isClone ? Input.GetKey(KeyCode.DownArrow) : (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift));
        if (currentPowerup == PowerupType.Swim && isInWater)
        {
            // If already sneaking, allow to remain sneaking until S released
            if (wasSneaking && sneakInput)
            {
                isSneaking = true;
            }
            else
            {
                isSneaking = false;
            }
        }
        else
        {
            isSneaking = sneakInput;
        }

        if (currentPowerup == PowerupType.Heavy)
        {
            // Heavy: Sneaking only affects speed and gravity, not collider/sprite
            if (isSneaking && !heavySneakActive)
            {
                // Increase gravity while sneaking
                rb.gravityScale = heavySneakGravityScale;
                audioSource.PlayOneShot(sneakSound);
                heavySneakActive = true;

                // Drop through platform
                StartCoroutine(DropThroughPlatform());
            }
            else if (!isSneaking && heavySneakActive)
            {
                // Restore gravity
                rb.gravityScale = originalGravityScale;
                audioSource.PlayOneShot(unsneakSound);
                heavySneakActive = false;
            }
            wasSneaking = isSneaking;
            return;
        }
        else
        {
            // Restore gravity if not Heavy
            if (heavySneakActive)
            {
                rb.gravityScale = originalGravityScale;
                heavySneakActive = false;
            }
        }

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

    private void PowerupActivate()
    {
        if (currentPowerup == PowerupType.Shrink)
        {
            if (!isShrunk)
            {
                // Shrink phantom effect
                StartCoroutine(ShrinkPhantomEffect());

                // Shrink player from bottom
                float oldHeight = defaultCollider.size.y * transform.localScale.y;
                transform.localScale = originalScale * shrinkScale;
                defaultCollider.size *= shrinkColliderScale;
                sneakingCollider.size *= shrinkColliderScale;
                moveSpeed *= shrinkScale;
                // Adjust position so feet stay in place
                float newHeight = defaultCollider.size.y * transform.localScale.y;
                float deltaY = oldHeight - newHeight;
                transform.position += new Vector3(0f, -deltaY / 2f, 0f);
                isShrunk = true;
            }
            else
            {
                // Check if there is enough room to grow above
                float growHeight = (defaultCollider.size.y / shrinkColliderScale) * originalScale.y;
                float currentHeight = defaultCollider.size.y * transform.localScale.y;
                float deltaY = growHeight - currentHeight;
                Vector2 checkPosition = (Vector2)transform.position + new Vector2(0f, deltaY / 2f);
                Vector2 checkSize = new Vector2(defaultCollider.size.x / shrinkColliderScale, deltaY);
                Collider2D hit = Physics2D.OverlapBox(
                    checkPosition + defaultCollider.offset,
                    checkSize,
                    0f,
                    combinedGroundLayer
                );
                if (hit != null)
                {
                    Debug.Log("[PlayerController] Not enough room to grow!");
                    return;
                }

                // Restore player from bottom
                transform.localScale = originalScale;
                defaultCollider.size /= shrinkColliderScale;
                sneakingCollider.size /= shrinkColliderScale;
                moveSpeed /= shrinkScale;
                // Adjust position so feet stay in place
                transform.position += new Vector3(0f, deltaY / 2f, 0f);
                isShrunk = false;
            }
        }
        else if (currentPowerup == PowerupType.Clone && !isClone)
        {
            if (!hasClone)
            {
                // Instantiate clone at current position
                if (clonePrefab != null)
                {
                    cloneInstance = Instantiate(clonePrefab, transform.position, transform.rotation);
                    PlayerController cloneController = cloneInstance.GetComponent<PlayerController>();
                    if (cloneController != null)
                    {
                        cloneController.isClone = true;
                        cloneController.currentPowerup = PowerupType.None; // Clone starts with no powerup
                        cloneController.cloneOwnerInstance = this; // Set owner reference on clone
                        // Apply stored powerup to clone if any
                        if (cloneCollectedPowerup != PowerupType.None)
                        {
                            cloneController.SetPowerup(cloneCollectedPowerup);
                        }
                    }
                    // Set clone layer to "PlayerClone"
                    int cloneLayer = LayerMask.NameToLayer("PlayerClone");
                    if (cloneLayer >= 0)
                    {
                        cloneInstance.layer = cloneLayer;
                        foreach (Transform child in cloneInstance.transform)
                            child.gameObject.layer = cloneLayer;
                    }
                    hasClone = true;
                    cloneOwnerInstance = this; // Set owner reference on owner
                }
            }
            else
            {
                // Destroy the clone if it exists
                if (cloneInstance != null)
                {
                    Destroy(cloneInstance);
                    cloneInstance = null;
                }
                hasClone = false;
                cloneOwnerInstance = null; // Clear owner reference
            }
        }
    }

    private IEnumerator ShrinkPhantomEffect()
    {
        for (int i = 0; i < shrinkPhantomCount; i++)
        {
            SpawnShrinkPhantom();
            yield return new WaitForSeconds(shrinkPhantomInterval);
        }
    }

    private void SpawnShrinkPhantom()
    {
        GameObject phantom = new GameObject("ShrinkPhantom");
        phantom.transform.position = transform.position;
        phantom.transform.localScale = transform.localScale;
        var sr = phantom.AddComponent<SpriteRenderer>();
        sr.sprite = spriteRenderer.sprite;
        sr.sortingLayerID = spriteRenderer.sortingLayerID;
        sr.sortingOrder = spriteRenderer.sortingOrder - 1;
        sr.color = shrinkPhantomColor;

        StartCoroutine(FadeAndDestroyPhantom(sr, shrinkPhantomDuration));
    }

    private IEnumerator FadeAndDestroyPhantom(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color startColor = sr.color;
        while (timer < duration)
        {
            float t = timer / duration;
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(sr.gameObject);
    }

    // FixedUpdate
    void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            MovingPlatformVelocityAdjustment();
        }
        if (currentElevator != null)
        {
            ElevatorVelocityAdjustment();
        }
    }

    private void MovingPlatformVelocityAdjustment()
    {
        // Moving platform velocity adjustment
        if (currentPlatform != null)
        {
            // Only apply horizontal platform velocity always
            Vector2 platformVel = currentPlatform.platformVelocity;
            Debug.Log($"[PlayerController] Platform velocity: {platformVel}");
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
    
    private void ElevatorVelocityAdjustment()
    {
        // Elevator velocity adjustment
        if (currentElevator != null)
        {
            Vector2 elevatorVel = currentElevator.elevatorVelocity;
            Debug.Log($"[PlayerController] Elevator velocity: {elevatorVel}, Elevator: {currentElevator.name}");
            rb.linearVelocity += new Vector2(elevatorVel.x, 0f);
            if (isGrounded && rb.linearVelocity.y <= 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + elevatorVel.y);
            }
        }

        // Reset for next frame
        currentElevator = null;
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
        if (isClone && cloneOwnerInstance != null)
        {
            cloneOwnerInstance.currentHealth = Mathf.Max(0, cloneOwnerInstance.currentHealth - 1);
        }
        else
        {
            currentHealth = Mathf.Max(0, currentHealth - 1);
        }

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
        else if (collision.collider.CompareTag("ElevatorFloor"))
        {
            ElevatorCollision(collision);
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

    private void ElevatorCollision(Collision2D collision)
    {
        var elevator = collision.collider.GetComponentInParent<Elevator>();
        // Find the parent Elevator component (ElevatorFloor is a child object)
        var parentElevator = collision.collider.transform.parent;
        if (parentElevator != null)
        {
            Elevator parentElevatorComponent = parentElevator.GetComponent<Elevator>();
            if (parentElevatorComponent != null)
            {
                Debug.Log($"[PlayerController] Setting currentElevator to {parentElevatorComponent.name} (velocity: {parentElevatorComponent.elevatorVelocity})");
                currentElevator = parentElevatorComponent;
            }
            else
            {
                Debug.LogWarning($"[PlayerController] Parent object {parentElevator.name} does not have Elevator component.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerController] ElevatorFloor has no parent transform.");
        }
    }

    public void SetPowerup(PowerupType type)
    {
        // Prevent clone from acquiring Clone powerup
        if (isClone && type == PowerupType.Clone)
        {
            return;
        }

        // If overriding Clone powerup and a clone exists, destroy the clone
        if (currentPowerup == PowerupType.Clone && type != PowerupType.Clone && cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            cloneCollectedPowerup = PowerupType.None;
            hasClone = false;
        }

        // Set powerup type
        currentPowerup = type;

        // Apply powerup effects
        if (type == PowerupType.Heavy)
        {
            spriteRenderer.sprite = defaultSprite;
            defaultCollider.enabled = true;
            sneakingCollider.enabled = false;
        }
        if (type == PowerupType.None)
        {
            rb.gravityScale = originalGravityScale;
            heavySneakActive = false;
        }
        
        // If this is a clone, also overwrite owner's cloneCollectedPowerup
        if (isClone && cloneOwnerInstance != null)
        {
            cloneOwnerInstance.cloneCollectedPowerup = type;
        }
    }

    public PowerupType GetPowerup()
    {
        return currentPowerup;
    }
}

