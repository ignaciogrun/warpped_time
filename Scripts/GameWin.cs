using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameWin : MonoBehaviour
{
    private LevelLoader levelLoader;

    private void Start()
    {
        levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();
    }

    public void LoadMainMenu()
    {
        levelLoader.LoadLevel("MainMenu");
    }
}
