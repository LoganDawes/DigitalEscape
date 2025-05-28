using UnityEngine;

/*
 
    ToggleablePlatform : Platforming
    Platform that can be toggled on and off by a button.
 
 */

public class ToggleablePlatform : Platform, IActivatable
{
    // Variables
    [Header("Sprites")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    public bool isActive = true;

    // Components
    private BoxCollider2D platformCollider;
    private SpriteRenderer spriteRenderer;

    // Start
    void Start()
    {
        // Initialize components
        platformCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Variable & Component checks
        if (platformCollider == null)
        {
            Debug.LogError("BoxCollider2D component not found on the toggleable platform object");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the toggleable platform object");
        }

        // Set initial state based on isActive
        if (isActive)
        {
            spriteRenderer.sprite = activeSprite;
            platformCollider.enabled = true; // Enable collider if active
        }
        else
        {
            spriteRenderer.sprite = inactiveSprite;
            platformCollider.enabled = false; // Disable collider if inactive
        }
    }

    // OnActivated
    public void onActivated()
    {
        isActive = !isActive;

        // Update sprite when activated
        spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;

        // Toggle the platform's collider to enable or disable it
        platformCollider.enabled = !platformCollider.enabled;
    }
}
