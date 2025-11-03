using UnityEngine;

/*

    WaterMine.cs : Hazards
    Floating mine that explodes on contact with the player, dealing damage in a radius.

*/

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class WaterMine : HazardBase
{
    // Variables
    [Header("Bobbing Settings")]
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 1.2f;

    [Header("Sinking Settings")]
    [SerializeField] private float sinkDuration = 0.5f;
    [SerializeField] private float maxSinkDepth = 2f;

    [Header("Physics")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isInWater = false;
    private float bobStartY;
    private float bobTimer = 0f;
    private float sinkDepth = 0f;
    private bool isSinking = false;
    private float sinkTimer = 0f;

    // Start
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("WaterMine requires a Rigidbody2D.");
        }
        // Ensure it collides with ground
        gameObject.layer = LayerMask.NameToLayer("Default");
        // Store initial Y for bobbing
        bobStartY = transform.position.y;
    }

    // Update
    void Update()
    {
        if (isInWater)
        {
            if (isSinking)
            {
                // Smoothly sink to sinkDepth over sinkDuration
                sinkTimer += Time.deltaTime;
                float t = Mathf.Clamp01(sinkTimer / sinkDuration);
                float targetY = bobStartY - sinkDepth;
                float newY = Mathf.Lerp(bobStartY, targetY, t);
                rb.MovePosition(new Vector2(transform.position.x, newY));
                rb.linearVelocity = Vector2.zero;

                if (t >= 1f)
                {
                    isSinking = false;
                    // Set bobStartY to the new resting position for bobbing
                    bobStartY = targetY;
                    bobTimer = Random.Range(0f, Mathf.PI * 2f);
                }
            }
            else
            {
                // Bobbing logic at the sunken depth
                bobTimer += Time.deltaTime * bobFrequency;
                float newY = bobStartY + Mathf.Sin(bobTimer) * bobAmplitude;
                rb.MovePosition(new Vector2(transform.position.x, newY));
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isInWater)
        {
            // Normal gravity applies
            rb.gravityScale = 1f;
        }
        else
        {
            // Disable gravity in water
            rb.gravityScale = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Entering water
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            isInWater = true;
            // Calculate sink depth based on downward velocity
            float entryVelocity = Mathf.Abs(rb.linearVelocity.y);
            sinkDepth = Mathf.Clamp(entryVelocity * 0.3f, 0.1f, maxSinkDepth);
            isSinking = true;
            sinkTimer = 0f;
            bobStartY = transform.position.y;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Exiting water
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            isInWater = false;
            isSinking = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy if colliding with Player
        if (collision.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}