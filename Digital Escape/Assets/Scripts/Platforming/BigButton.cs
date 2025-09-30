using UnityEngine;

/*
 
    BigButton : Platforming
    Button that activates attached components when stood on / box collided with.
 
 */

public class BigButton : ButtonBase
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
