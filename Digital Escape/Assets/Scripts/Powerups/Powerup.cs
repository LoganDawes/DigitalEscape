using UnityEngine;

public enum PowerupType
{
    None,
    Heavy,
    Shrink,
    Clone,
    Swim
}

/*
 
    PowerupBase : Powerups
    Base class for all powerups.
 
 */

public class Powerup : MonoBehaviour
{
    // Variables
    public PowerupType type;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 1.5f;
    private Vector3 startPos;

    // Start
    void Start()
    {
        startPos = transform.position;
    }

    // Update
    void Update()
    {
        // Hover up and down
        float offset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = startPos + new Vector3(0, offset, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.SetPowerup(type))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
