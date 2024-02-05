using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class KillCountManager : MonoBehaviour
{

    public static KillCountManager instance;
    
    private Text killsText;

    private float killCount;
    private bool textEnabled;
    
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
        
        killsText = GetComponentInChildren<Text>();
        killsText.gameObject.SetActive(PlayerPrefs.GetInt("Kill Int") == 1);
    }

    void Update()
    {
        if (!textEnabled)
            return;
            
        string scene = SceneManager.GetActiveScene().name;
        bool validScene = !(scene == "GameWon" || scene == "MainMenu");

        killsText.enabled = textEnabled && validScene;

        if (validScene && !GameManager.instance.IsGamePaused())
            killsText.text = "Kills: " + killCount.ToString("0");
    }

    public float GetKillCount()
    {
        return killCount;
    }

    public void SetKillCount(float kills)
    {
        this.killCount = kills;
    }
    
    public void IncrementKillCount()
    {
        this.killCount += 1;
    }
    
    public void EnableText()
    {
        textEnabled = true;
        killsText.gameObject.SetActive(true);
        killsText.text = "Kills: " + killCount.ToString("0");
        PlayerPrefs.SetInt("Kill Int", 1);
    }

    public void DisableText()
    {
        textEnabled = false;
        killsText.gameObject.SetActive(false);
        PlayerPrefs.SetInt("Kill Int", 0);
    }
    
}
