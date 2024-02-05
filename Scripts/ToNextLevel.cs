using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToNextLevel : MonoBehaviour
{
    public string sceneName;

    private LevelLoader levelLoader;

    private bool alreadyLoading = false;
    //private bool miniBossDefeated = false;

    private void Start()
    {
        //Find the levelLoader
        levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //If object is a player 
        if (other.gameObject.tag == "Player" && !alreadyLoading)
        {
            levelLoader.LoadLevel(sceneName);
            alreadyLoading = true;
        }
    }

    //void MiniBossDefeated()
    //{
    //    miniBossDefeated = true;
    //}


}
