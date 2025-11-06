using UnityEngine;

/*
 
    Button : Platforming
    When interacted with, activates attached components.
 
 */

public class Button : ButtonBase
{
    // Variables
    private bool playerInRange = false;
    private bool cloneInRange = false;

    // Update
    protected virtual void Update()
    {
        // Player interaction with E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnPressed();
        }
        // Clone interaction with F
        if (cloneInRange && Input.GetKeyDown(KeyCode.F))
        {
            OnPressed();
        }
    }

    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.isClone)
            {
                playerInRange = true;
            }
            else
            {
                cloneInRange = true;
            }
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.isClone)
            {
                playerInRange = false;
            }
            else
            {
                cloneInRange = false;
            }
        }
    }
}