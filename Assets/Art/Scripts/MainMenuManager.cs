using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public float ScreenFadeTime = 0.5f;
    public CanvasGroup FadeCanvasGroup;

    private string _loadSceneName = string.Empty;

    private void Start()
    {
        // Fade IN when scene starts
        StartCoroutine(Fade(1f, 0f));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        _loadSceneName = sceneName;
        StartCoroutine(FadeThenLoadScene());
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RestartCurrentScene()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        LoadSceneWithFade(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }

    private IEnumerator FadeThenLoadScene()
    {
        // Fade OUT
        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(_loadSceneName);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < ScreenFadeTime)
        {
            elapsedTime += Time.deltaTime;
            FadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / ScreenFadeTime);
            yield return null;
        }

        FadeCanvasGroup.alpha = endAlpha;
    }
}
