using UnityEngine;

/*

    GameManager : Core
    Responsible for managing the game state, including starting, pausing, and ending the game.
    Keeps track of checkpoints and player progress through saves.

 */

public class GameManager : MonoBehaviour
{
    // Variables

    // Components

    // Instance
    private static GameManager instance;

    // Awake
    private void Awake()
    {
        // Creates one instance of the game manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start
    void Start()
    {
        
    }

    // Update
    void Update()
    {
        
    }
}
