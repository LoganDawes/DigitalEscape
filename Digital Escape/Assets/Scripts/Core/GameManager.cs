using UnityEngine;
using UnityEngine.SceneManagement;

/*

    GameManager : Core
    Responsible for managing the game state, including starting, pausing, and ending the game.
    Keeps track of checkpoints and player progress through saves.

 */

public class GameManager : MonoBehaviour
{
    // Instance
    public static GameManager instance;

    // References
    private PlayerController player;
    private CameraController cameraController;


    // Awake
    private void Awake()
    {
        // Creates one instance of the game manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[GameManager] Awake: GameManager instance created and set to persist.");
        }
        else
        {
            Debug.LogWarning("[GameManager] Awake: Duplicate GameManager destroyed.");
            Destroy(gameObject);
        }
    }

    // Assign player and camera references
    public void RegisterPlayer(PlayerController p)
    {
        if (p != null && !p.isClone)
        {
            player = p;
            Debug.Log($"[GameManager] RegisterPlayer: Player registered ({player?.name})");
        }
    }
    public void RegisterCamera(CameraController cam)
    {
        cameraController = cam;
        Debug.Log($"[GameManager] RegisterCamera: Camera registered ({cameraController?.name})");
    }

    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] OnSceneLoaded: Scene '{scene.name}' loaded.");

        // Find EntryDoor in the new scene
        GameObject entryDoor = GameObject.FindGameObjectWithTag("EntryDoor");
        if (entryDoor != null)
        {
            Debug.Log($"[GameManager] EntryDoor found: {entryDoor.name} at {entryDoor.transform.position}");
        }
        else
        {
            Debug.LogWarning("[GameManager] EntryDoor not found in scene.");
        }

        // Ensure player reference is set from DontDestroyOnLoad
        if (player == null)
        {
            var foundPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var foundPlayer in foundPlayers)
            {
                if (!foundPlayer.isClone)
                {
                    player = foundPlayer;
                    Debug.Log($"[GameManager] Found and assigned player: {player.name}");
                    break;
                }
            }
            if (player == null)
            {
                Debug.LogWarning("[GameManager] No non-clone PlayerController found in scene.");
            }
        }

        // Destroy clone if active
        if (player != null && player.cloneInstance != null)
        {
            Object.Destroy(player.cloneInstance);
            player.cloneInstance = null;
            player.hasClone = false;
            player.cloneOwnerInstance = null;
            player.cloneCollectedPowerup = PowerupType.None;
            Debug.Log("[GameManager] Clone destroyed on scene load.");
        }

        // Move player to EntryDoor if possible
        if (player != null && entryDoor != null)
        {
            player.transform.position = entryDoor.transform.position;
            Debug.Log($"[GameManager] Player '{player.name}' moved to EntryDoor position.");
        }
        else if (player == null)
        {
            Debug.LogWarning("[GameManager] Player reference is null when trying to move to EntryDoor.");
        }
        else if (entryDoor == null)
        {
            Debug.LogWarning("[GameManager] EntryDoor not found, player not moved.");
        }

        // Restore camera target to player
        if (cameraController == null)
        {
            cameraController = Object.FindFirstObjectByType<CameraController>();
            if (cameraController != null)
            {
                Debug.Log($"[GameManager] Found and assigned camera: {cameraController.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] No CameraController found in scene.");
            }
        }
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
            Debug.Log($"[GameManager] Camera target set to player: {player.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Camera or player reference missing when setting camera target.");
        }
    }

    // Scene loading API for Door
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[GameManager] LoadScene called for scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
