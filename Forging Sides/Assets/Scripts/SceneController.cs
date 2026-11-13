using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    [Header("Transition Settings")]
    public Image fadeImage;
    public float transitionTime = 0.5f;

    private bool isTransitioning = false;

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

    void Start()
    {
        string currentName = SceneManager.GetActiveScene().name;

        // Fade in when the very first scene launches
        if (fadeImage != null)
        {
            StartCoroutine(Fade(1f, 0f));
        }
    }

    public void NextLevel()
    {
        if (!isTransitioning)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            StartCoroutine(TransitionRoutine(nextIndex, null));
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionRoutine(-1, sceneName));
        }
    }

    private IEnumerator TransitionRoutine(int sceneIndex, string sceneName)
    {
        isTransitioning = true;

        // 1. Fade out to black
        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

        // 2. Load the scene asynchronously and wait until it finishes
        AsyncOperation asyncLoad = (sceneIndex >= 0) 
            ? SceneManager.LoadSceneAsync(sceneIndex) 
            : SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Fade back in to reveal the new level
        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.unscaledDeltaTime; // Works even if Time.timeScale is 0
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / transitionTime);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    public bool IsLastScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1;

        return currentIndex == lastSceneIndex;
    }
}