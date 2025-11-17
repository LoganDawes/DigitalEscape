using UnityEngine;
using TMPro;
using System.Collections;

/*
 
    LevelTitle : UI
    UI overlay for displaying the level title.
 
 */

public class LevelTitle : MonoBehaviour
{
    // Variables
    [SerializeField] private TextMeshProUGUI titleText;
    public static LevelTitle instance;
    private Coroutine fadeCoroutine;

    // Awake
    private void Awake()
    {
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
        if (titleText == null)
        {
            titleText = GetComponent<TextMeshProUGUI>();
        }
        SetAlpha(1f);
    }

    public void SetAlpha(float alpha)
    {
        if (titleText != null)
        {
            var color = titleText.color;
            color.a = alpha;
            titleText.color = color;
        }
    }

    private IEnumerator ShowAndFadeOut()
    {
        SetAlpha(1f);
        yield return new WaitForSeconds(1f);
        yield return FadeOut(1f);
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

    public void ShowTitle(string text)
    {
        if (titleText != null)
        {
            titleText.text = text;
            SetAlpha(1f);
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(ShowAndFadeOut());
        }
    }
}
