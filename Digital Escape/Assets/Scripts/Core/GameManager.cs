using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    private Fade fade;
    private LevelTitle levelTitle;


    // Awake
    private void Awake()
    {
        // Creates one instance of the game manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            fade = Fade.instance;
            levelTitle = LevelTitle.instance;
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
        }
    }
    public void RegisterCamera(CameraController cam)
    {
        cameraController = cam;
    }

    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip player/camera actions if on TitleScreen
        if (scene.name == "TitleScreen")
        {
            return;
        }

        GameObject entryDoor = FindEntryDoor();
        EnsurePlayerReference();
        DestroyPlayerClone();
        MovePlayerToEntryDoor(entryDoor);
        RestoreCameraTarget();

        // Show LevelTitle on scene load
        if (levelTitle == null)
        {
            levelTitle = LevelTitle.instance;
        }
        if (levelTitle != null)
        {
            levelTitle.ShowTitle(scene.name);
        }
    }

    // Find EntryDoor in the scene
    private GameObject FindEntryDoor()
    {
        var entryDoor = GameObject.FindGameObjectWithTag("EntryDoor");
        if (entryDoor == null)
        {
            Debug.LogWarning("[GameManager] EntryDoor not found in scene.");
        }
        return entryDoor;
    }

    // Ensure player reference is set
    private void EnsurePlayerReference()
    {
        if (player == null)
        {
            var foundPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var foundPlayer in foundPlayers)
            {
                if (!foundPlayer.isClone)
                {
                    player = foundPlayer;
                    break;
                }
            }
            if (player == null)
            {
                Debug.LogWarning("[GameManager] No non-clone PlayerController found in scene.");
            }
        }
    }

    // Destroy clone if active
    private void DestroyPlayerClone()
    {
        if (player != null && player.cloneInstance != null)
        {
            Object.Destroy(player.cloneInstance);
            player.cloneInstance = null;
            player.hasClone = false;
            player.cloneOwnerInstance = null;
        }
    }

    // Move player to EntryDoor
    private void MovePlayerToEntryDoor(GameObject entryDoor)
    {
        if (player != null && entryDoor != null)
        {
            player.transform.position = entryDoor.transform.position;
        }
        else if (player == null)
        {
            Debug.LogWarning("[GameManager] Player reference is null when trying to move to EntryDoor.");
        }
        else if (entryDoor == null)
        {
            Debug.LogWarning("[GameManager] EntryDoor not found, player not moved.");
        }
    }

    // Restore camera target to player
    private void RestoreCameraTarget()
    {
        if (cameraController == null)
        {
            cameraController = Object.FindFirstObjectByType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("[GameManager] No CameraController found in scene.");
            }
        }
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
        }
        else
        {
            Debug.LogWarning("[GameManager] Camera or player reference missing when setting camera target.");
        }
    }

    // Scene loading API for Door
    public void LoadScene(string sceneName)
    {
        LockPlayerControlOnTransition();
        StartCoroutine(TransitionScene(sceneName));
    }

    private void LockPlayerControlOnTransition()
    {
        if (player != null)
        {
            player.SetControlLocked(true);
        }
    }

    private IEnumerator TransitionScene(string sceneName)
    {
        if (fade == null)
        {
            fade = Fade.instance;
        }
        if (fade != null)
        {
            yield return fade.FadeIn(1f); // Fade to black
        }
        SceneManager.LoadScene(sceneName);
        // Wait for scene to load
        yield return new WaitForSeconds(0.2f);
        if (fade == null)
        {
            fade = Fade.instance;
        }
        if (fade != null)
        {
            player.SetControlLocked(false);
            yield return fade.FadeOut(1f); // Fade out to transparent
        }
    }
}
