using UnityEngine;

/*
 
    PlayerHealthUI : UI
    UI overlay for displaying the player's health over their head.
 
 */

public class PlayerHealthUI : MonoBehaviour
{
    // Variables
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] private UnityEngine.UI.Image[] healthBars;

    // Start
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    // Update
    void Update()
    {
        if (player == null || healthBars == null || healthBars.Length == 0)
            return;

    // Position health bar above player in world space
    transform.position = player.position + offset;
    // Optional: make the health bar always face the camera
    transform.LookAt(Camera.main.transform);

        // Get health from PlayerController
        int health = 3;
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
            health = (int)pc.currentHealth;

        // Update health bar visibility
        for (int i = 0; i < healthBars.Length; i++)
        {
            healthBars[i].enabled = i < health;
        }
    }
}
