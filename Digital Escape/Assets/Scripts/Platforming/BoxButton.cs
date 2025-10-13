using UnityEngine;

/*
 
    BoxButton : Platforming
    Button that can be pressed by a box to activate attached component.
 
 */

public class BoxButton : ButtonBase
{
    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            OnPressed();
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            OnPressed();
        }
    }
}
