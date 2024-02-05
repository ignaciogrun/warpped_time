using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlashEffectManager : MonoBehaviour
{
    public static SlashEffectManager instance;

    public Image[] images;
    int currentSlash = 0;
    bool playing = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void PlaySlash()
    {
        StartCoroutine(PlaySlashEffect());
    }

    IEnumerator PlaySlashEffect()
    {
        if (!playing)
        {
            playing = true;
            images[currentSlash].gameObject.SetActive(true);
            float fillAmount = 0.0f;
            while (fillAmount < 1f)
            {
                fillAmount += Time.deltaTime * 6f;
                images[currentSlash].fillAmount = fillAmount;

                yield return null;
            }

            yield return new WaitForSeconds(.1f);
            images[currentSlash].gameObject.SetActive(false);
            images[currentSlash].fillAmount = 0;

            currentSlash++;
            if (currentSlash >= images.Length)
            {
                currentSlash = 0;
            }

            playing = false;
        }
    }
}
