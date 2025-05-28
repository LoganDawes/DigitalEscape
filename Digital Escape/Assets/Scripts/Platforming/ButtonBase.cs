using UnityEngine;

/*
 
    ButtonBase : Platforming
    Base class for all button types.
 
 */

public class ButtonBase : MonoBehaviour
{
    // Variables
    [Header("Sprites")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    public bool isActive = false;

    // Components
    private IActivatable activatable;
    public GameObject connectedObject;

    private BoxCollider2D buttonCollider;
    private SpriteRenderer spriteRenderer;

    // Start
    void Start()
    {
        // Initialize components
        buttonCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Variable & Component checks
        if (connectedObject == null)
        {
            Debug.LogWarning("Connected object is not assigned to button object");
        }
        else
        {
            activatable = connectedObject.GetComponent<IActivatable>();
            if (activatable == null)
            {
                Debug.LogError("Connected object does not implement IActivatable interface");
            }
        }
        if (buttonCollider == null)
        {
            Debug.LogError("BoxCollider2D component not found on the button object");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the button object");
        }

        // Set initial sprite based on active state
        spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
    }

    // OnPressed
    public void OnPressed()
    {
        isActive = !isActive;

        // Update sprite when pressed
        spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;

        // Execute the onActivated method on the connected object if it exists
        activatable?.onActivated();
    }
}
