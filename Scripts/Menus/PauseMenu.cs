using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    
    private GameObject settingsMenu;
    private CheckPointManager checkPointManager;

    void Start()
    {
        settingsMenu = transform.Find("Settings Menu").gameObject;
        checkPointManager = GameObject.Find("CheckPointManager").GetComponent<CheckPointManager>();
    }

    public void PlayPressSound()
    {
        AudioManager.instance.Play("Player Click");
    }

    public void ResumeGame()
    {
        HideSettings();
        GameManager.instance.ResumeGame();
    }
    
    public void DisplaySettings()
    {
        settingsMenu.SetActive(true);
        settingsMenu.SendMessageUpwards("UpdateUI");
    }

    public void HideSettings()
    {
        settingsMenu.SetActive(false);
    }
    
    public void LoadMainMenu()
    {
        GameManager.instance.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void Respawn()
    {
        if (checkPointManager == null)
            checkPointManager = GameObject.Find("CheckPointManager").GetComponent<CheckPointManager>();

        checkPointManager.SetCurrentStats();
        GameManager.instance.ResumeGame();
    }

}
