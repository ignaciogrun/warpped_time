using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public PauseMenu pauseMenu;
    public DeathMenu deathMenu;

    public string startScene;

    private LevelLoader levelLoader;

    private bool[] slotsOccupied = new bool[3];

    private bool gamePaused = false;
    private bool deathScreen = false;

    private Vector3 playerPosition;
    private string scene;
    private int saveSlot;

    private const string saveSlotString = "_saveSlot ";
    private const string xPosString = "posX" + saveSlotString;
    private const string yPosString = "posY" + saveSlotString;
    private const string zPosString = "posZ" + saveSlotString;
    private const string killCountString = "killCount" + saveSlotString;
    private const string timeString = "time" + saveSlotString;
    private const string sceneString = "scene" + saveSlotString;

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

    void Start()
    {
        for (int i = 0; i < 3; ++i)
            slotsOccupied[i] = !string.IsNullOrEmpty(PlayerPrefs.GetString(sceneString + i));
    }

    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (deathScreen || scene == "MainMenu" || !Input.GetButtonDown("Pause"))
            return;

        if (scene == "GameWon")
        {
            LoadScene("MainMenu");
            return;
        }

        if (gamePaused)
            ResumeGame();

        else
            PauseGame();

    }

    private void LoadScene(string sceneName)
    {
        if (levelLoader == null)
            levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();

        levelLoader.LoadLevel(sceneName);
    }

    public void DisplayDeathMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        deathScreen = true;
        deathMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideDeathMenu()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        deathScreen = false;
        deathMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void PauseGame()
    {
        if (gamePaused)
            return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gamePaused = true;
        pauseMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!gamePaused)
            return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gamePaused = false;
        pauseMenu.HideSettings();
        pauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool IsGamePaused()
    {
        return gamePaused;
    }

    //Function Save
    public void Save(Vector3 pos)
    {
        PlayerPrefs.SetFloat(xPosString + saveSlot, pos.x);
        PlayerPrefs.SetFloat(yPosString + saveSlot, pos.y);
        PlayerPrefs.SetFloat(zPosString + saveSlot, pos.z);
        PlayerPrefs.SetFloat(killCountString + saveSlot, KillCountManager.instance.GetKillCount());
        PlayerPrefs.SetFloat(timeString + saveSlot, TimeManager.instance.GetTimePassed());
        PlayerPrefs.SetString(sceneString + saveSlot, SceneManager.GetActiveScene().name);

        for (int i = 0; i < 3; ++i)
            slotsOccupied[i] = !string.IsNullOrEmpty(PlayerPrefs.GetString(sceneString + i));
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(killCountString + saveSlot, KillCountManager.instance.GetKillCount());
        PlayerPrefs.SetFloat(timeString + saveSlot, TimeManager.instance.GetTimePassed());

        for (int i = 0; i < 3; ++i)
            slotsOccupied[i] = !string.IsNullOrEmpty(PlayerPrefs.GetString(sceneString + i));
    }

    //Function Load:
    //Set local playerPosition, set killCount, set timePassed, and load scene.
    public void Load()
    {
        playerPosition = new Vector3(PlayerPrefs.GetFloat(xPosString + saveSlot, 0), PlayerPrefs.GetFloat(yPosString + saveSlot, 0), PlayerPrefs.GetFloat(zPosString + saveSlot, 0));
        KillCountManager.instance.SetKillCount(PlayerPrefs.GetFloat(killCountString + saveSlot, 0));
        TimeManager.instance.SetTimePassed(PlayerPrefs.GetFloat(timeString + saveSlot, 0));

        string foundScene = PlayerPrefs.GetString(sceneString + saveSlot);
        if (string.IsNullOrEmpty(foundScene))
            foundScene = startScene;

        scene = foundScene;
        LoadScene(foundScene);
    }

    //Function SetSaveSlot
    public void SetSaveSlot(int setValue)
    {
        saveSlot = setValue;
        Load();
    }

    public void SetSaveSlot(int saveValue, bool overwrite)
    {
        saveSlot = saveValue;

        if (!overwrite)
        {
            Load();
            return;
        }

        TimeManager.instance.SetTimePassed(0);
        KillCountManager.instance.SetKillCount(0);
        LoadScene(startScene);
    }

    public Vector3 GetPlayerStartPosition()
    {
        return playerPosition;
    }

    public void ResetPlayerPosition()
    {
        playerPosition = Vector3.zero;
    }

    public string GetPlayerStartScene()
    {
        return scene;
    }

    public bool HasEmptySaveSlots()
    {
        foreach (bool slotFull in slotsOccupied)
            if (!slotFull)
                return true;

        return false;
    }

    public bool HasOnlyEmptySaveSlots()
    {
        foreach (bool slotFull in slotsOccupied)
            if (slotFull)
                return false;

        return true;
    }

    public bool IsSlotOccupied(int slot)
    {
        return slotsOccupied[slot];
    }
}
