using UnityEngine;
using System.Collections;

/*
 
    Fade : UI
    UI overlay for fading in and out of the game.
 
 */

public class Fade : MonoBehaviour
{
    // Variables
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    public static Fade instance;

    private void Awake()
    {
        gameObject.SetActive(true); // Ensure object is enabled
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

    void Start()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<UnityEngine.UI.Image>();
        }
        SetAlpha(1f); // Start opaque
        StartCoroutine(FadeOut(1f));
    }

    public void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            var color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }

    public IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, elapsed / duration));
            yield return null;
        }
        SetAlpha(1f);
    }

    public IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }
        SetAlpha(0f);
    }
}
