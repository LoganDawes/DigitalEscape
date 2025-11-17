using UnityEngine;
using UnityEngine.Audio;

/*

    AudioManager : Core
    Manages audio playback, including background music and sound effects. Keeps track of volume levels and audio settings.

 */

public class AudioManager : MonoBehaviour
{
    // Variables
    private string musicVolumeParameter = "MusicVolume";
    private string sfxVolumeParameter = "SFXVolume";

    // Components
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private AudioSource musicSource;

    // Instance
    public static AudioManager instance;

    // Awake
    private void Awake()
    {
        gameObject.SetActive(true); // Ensure object is enabled
        
        // Initialize components
        musicSource = GetComponent<AudioSource>();

        // Variable & Component checks
        if (musicSource == null)
        {
            Debug.LogError("Audio source component not found on audio manager object");
        }

        // Creates one instance of the audio manager
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

    // Play music
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(musicVolumeParameter, Mathf.Log10(volume) * 20);
    }

    // Set sound effect volume
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(sfxVolumeParameter, Mathf.Log10(volume) * 20);
    }
}
