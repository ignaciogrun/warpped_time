using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public AudioMixer mixer;

    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider brightnessSlider;

    private Text killButtonText;
    private Text timerButtonText;

    private bool timerBool = false;
    private bool killBool = false;
    private float rbgOpposite;


    public Button killButton;
    public Button timerButton;

    private ColorBlock killColors;
    private ColorBlock timerColors;

    Color brightnessScreen;
    

    void Awake()
    {
        killButtonText = killButton.GetComponentInChildren<Text>();
        timerButtonText = timerButton.GetComponentInChildren<Text>();

        timerColors = timerButton.colors;
        killColors = killButton.colors;
    }
    
    void Start()
    {
        UpdateUI();
        

        brightnessScreen = GameObject.Find("Brightness Screen").GetComponentInChildren<SpriteRenderer>().color;
    }

    //Get Player Prefs
    public void GetPrefs()
    {
        musicSlider.value = PlayerPrefs.GetFloat("Slider MusicVol", 5f);
        sfxSlider.value = PlayerPrefs.GetFloat("Slider SFXVol", 0f);
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness Slider", 1f);
        timerBool = PlayerPrefs.GetInt("Timer Int", 0) == 1;
        killBool = PlayerPrefs.GetInt("Kill Int", 0) == 1;
        rbgOpposite = PlayerPrefs.GetFloat("Brightness Alpha", 0.1f);
    }

    public void UpdateUI()
    {
        GetPrefs();
        UpdateTimeUI();
        UpdateKillCountUI();
    }

    //Cahgnge music volume
    public void ChangeMusicVolume(float vol)
    {
        mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol", Mathf.Log10(vol) * 20));
        PlayerPrefs.SetFloat("Slider MusicVol", vol);
    }

    //Change SFX volume
    public void ChangeSFXVolume(float vol)
    {
        mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol", Mathf.Log10(vol) * 20));
        PlayerPrefs.SetFloat("Slider SFXVol", vol);
    }

    //Change brightness
    public void Brightness(float rbgValue)
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.name == "Directional Light")
                l.color = new Color(rbgValue, rbgValue, rbgValue, 1);
        }

        rbgOpposite = (1.0f - rbgValue);
        brightnessScreen.a = rbgOpposite;

        GameObject.Find("Brightness Screen").GetComponentInChildren<SpriteRenderer>().color = brightnessScreen;

        PlayerPrefs.SetFloat("Brightness Alpha", rbgOpposite);
        PlayerPrefs.SetFloat("Light", rbgValue);
        PlayerPrefs.SetFloat("Brightness Slider", rbgValue);
    }

    //Toggle the timer on the HUD (In-game time)
    private void UpdateTimeUI()
    {
        if (timerBool)
        {
            timerButtonText.text = "Enabled";
            timerColors.normalColor = Color.green;
            timerColors.highlightedColor = Color.green;
            timerButton.colors = timerColors;

            if (TimeManager.instance != null)
                TimeManager.instance.EnableText();
        }
        else
        {
            timerButtonText.text = "Disabled";
            timerColors.normalColor = Color.red;
            timerColors.highlightedColor = Color.red;
            timerButton.colors = timerColors;

            if (TimeManager.instance != null)
                TimeManager.instance.DisableText();
        }

        PlayerPrefs.SetInt("Timer Int", timerBool ? 1 : 0);
    }
    
    //Flip timer toggle
    public void FlipToggleTimer()
    {
        timerBool = !timerBool;
        UpdateTimeUI();
    }


    //Toggle the Kill Count on the HUD
    private void UpdateKillCountUI()
    {
        if (killBool)
        {
            killButtonText.text = "Enabled";
            killColors.normalColor = Color.green;
            killColors.highlightedColor = Color.green;
            killButton.colors = killColors;

            if (KillCountManager.instance != null)
                KillCountManager.instance.EnableText();
        }
        
        else
        {
            killButtonText.text = "Disabled";
            killColors.normalColor = Color.red;
            killColors.highlightedColor = Color.red;
            killButton.colors = killColors;

            if (KillCountManager.instance != null)
                KillCountManager.instance.DisableText();
        }

        PlayerPrefs.SetInt("Kill Int", killBool ? 1 : 0);

    }
    
    //Flip timer toggle
    public void FlipToggleKill()
    {
        killBool = !killBool;
        UpdateKillCountUI();
    }

}
