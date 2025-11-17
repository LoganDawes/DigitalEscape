using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 
    GameOverUI : UI
    UI for the game over screen
 
 */

public class GameOverUI : MonoBehaviour
{
    // Variables
    [SerializeField] private float fadeDuration = 1.0f;

    public static GameOverUI instance;

    private Graphic[] graphics;
    private float[] initialAlphas;

    // Components

    // Awake
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

    // Start
    void Start()
    {
        // Cache all child graphics for fading
        graphics = GetComponentsInChildren<Graphic>(true);
        initialAlphas = new float[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
        {
            initialAlphas[i] = graphics[i].color.a;
        }
        SetAlpha(0f);
    }

    // Update
    void Update()
    {

    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private void SetAlpha(float t)
    {
        if (graphics != null && initialAlphas != null)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                var color = graphics[i].color;
                color.a = Mathf.Lerp(0f, initialAlphas[i], t);
                graphics[i].color = color;
            }
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;
            SetAlpha(t);
            timer += Time.deltaTime;
            yield return null;
        }
        SetAlpha(1f);
    }
}
