using UnityEngine;

public enum PowerupType
{
    None,
    Heavy,
    Shrink,
    Clone,
    Swim,
    Health
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
    public float powerupTimer = 10f;
    private Vector3 startPos;
    private float timer = 0f;
    private bool isDisabled = false;

    // Start
    void Start()
    {
        startPos = transform.position;
    }

    // Update
    void Update()
    {
        // Hover up and down
        if (!isDisabled)
        {
            float offset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.position = startPos + new Vector3(0, offset, 0);
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= powerupTimer)
            {
                EnablePowerup();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDisabled && other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (type == PowerupType.Health)
                {
                    // Heal player by 1, do not overwrite powerup type
                    player.currentHealth = Mathf.Min(player.currentHealth + 1, player.maxHealth);
                    DisablePowerup();
                }
                else
                {
                    player.SetPowerup(type);
                    DisablePowerup();
                }
            }
        }
    }

    void DisablePowerup()
    {
        isDisabled = true;
        timer = 0f;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void EnablePowerup()
    {
        isDisabled = false;
        timer = 0f;
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        transform.position = startPos;
    }
}
