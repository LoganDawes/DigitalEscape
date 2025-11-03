using UnityEngine;

/*
 
    ButtonBase : Platforming
    Base class for all button types.
 
 */

public class ButtonBase : MonoBehaviour
{
    // Variables
    [Header("Sprites")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    public bool isActive = false;

    // Components
    private IActivatable activatable;
    [SerializeField] private GameObject connectedObject;

    private BoxCollider2D buttonCollider;
    private SpriteRenderer spriteRenderer;

    // Awake
    void Awake()
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
    }

    // Start
    void Start()
    {
        // Ensure the button starts with the correct sprite
        spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
    }

    // OnPressed
    public virtual void OnPressed()
    {
        isActive = !isActive;

        // Update sprite when pressed
        spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;

        // Execute the onActivated method on the connected object if it exists
        activatable?.onActivated();
    }

# if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (connectedObject != null)
        {
            Vector3 start = transform.position;
            Vector3 end = connectedObject.transform.position;
            float dashLength = 0.2f;
            float gapLength = 0.1f;
            float distance = Vector3.Distance(start, end);
            Vector3 direction = (end - start).normalized;

            float drawn = 0f;
            bool draw = true;
            while (drawn < distance)
            {
                float segment = draw ? dashLength : gapLength;
                float next = Mathf.Min(segment, distance - drawn);
                if (draw)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(start + direction * drawn, start + direction * (drawn + next));
                }
                drawn += next;
                draw = !draw;
            }
        }
    }
# endif
}
