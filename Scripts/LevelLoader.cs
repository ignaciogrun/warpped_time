using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public RawImage background;
    public GameObject loadingScreen;
    public Canvas loadingScreenCanvas;
    public Slider slider;

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadAsynchrously(sceneName));
    }

    IEnumerator LoadAsynchrously(string sceneName)
    {
        loadingScreenCanvas.sortingOrder = 11;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        background.color = new Color(1, 1, 1, 1);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            slider.value = progress;

            yield return null;
        }

        background.color = new Color(1, 1, 1, 0);
    }

}
