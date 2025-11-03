using UnityEngine;

/*
 
    BigButton : Platforming
    Button that activates attached components when stood on / box collided with.
 
 */

public class BigButton : ButtonBase
{
    public string[] activatorTags;

    private bool IsActivatorTag(string tag)
    {
        foreach (var t in activatorTags)
        {
            if (tag == t)
                return true;
        }
        return false;
    }

    // OnTriggerEnter2D
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsActivatorTag(other.tag))
        {
            OnPressed();
        }
    }

    // OnTriggerExit2D
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsActivatorTag(other.tag))
        {
            OnPressed();
        }
    }
}
