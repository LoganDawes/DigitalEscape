using UnityEngine;

/*
 
    Button : Platforming
    When interacted with, activates attached components.
 
 */

public class Button : ButtonBase
{
    // Variables
    private bool playerInRange = false;

    // Update
    void Update()
    {
        // Player interaction with E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnPressed();
        }
    }

    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}