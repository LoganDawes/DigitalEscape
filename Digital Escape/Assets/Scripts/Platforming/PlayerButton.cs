using UnityEngine;

/*
 
    PlayerButton : Platforming
    Button that activates attached components when stood on.
 
 */

public class PlayerButton : ButtonBase
{
    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPressed();
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPressed();
        }
    }
}
