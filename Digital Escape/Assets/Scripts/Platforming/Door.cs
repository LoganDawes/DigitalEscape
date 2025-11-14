using UnityEngine;
using UnityEngine.SceneManagement;

/*
 
    Door : Platforming
    Door that leads to the next level/scene when interacted with.
 
 */

public class Door : MonoBehaviour
{
    // Variables
    [SerializeField] private string sceneToLoad;

    // Components

    // Start
    void Start()
    {

    }

    // Update
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.isClone)
            {
                if (GameManager.instance != null)
                {
                    GameManager.instance.LoadScene(sceneToLoad);
                }
                else
                {
                    // Fallback if GameManager not found
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
                }
            }
        }
    }
}
