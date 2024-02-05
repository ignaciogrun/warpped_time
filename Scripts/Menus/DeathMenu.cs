using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    
    private LevelLoader levelLoader;
    private CheckPointManager checkPointManager;

    void Start()
    {
		checkPointManager = GameObject.Find("CheckPointManager").GetComponent<CheckPointManager>();
        levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();
    }
    
    public void PlayPressSound()
    {
        AudioManager.instance.Play("Player Click");
    }

    public void ChangeScene(string sceneName)
    {
        if (levelLoader == null)
            levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();

        GameManager.instance.Save();
        levelLoader.LoadLevel(sceneName);
        GameManager.instance.HideDeathMenu();
        GameManager.instance.PauseGame();
    }    
    
    //Respawn At CheckPoint
    public void GoToCheckPoint()
    {
        if (checkPointManager == null)
            checkPointManager = GameObject.Find("CheckPointManager").GetComponent<CheckPointManager>();

        //Tell CheckpointManager to reset the player and their health
        checkPointManager.SetCurrentStats();
    }
    
}

