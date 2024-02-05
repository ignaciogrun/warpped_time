using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameManager;
    public GameObject audioManager;
    public GameObject timeManager;
    public GameObject killCountManager;
    public GameObject slashEffectManager;

    void Awake()
    {
        if (AudioManager.instance == null)
            Instantiate(audioManager);
        
        if (GameManager.instance == null)
            Instantiate(gameManager);
       
        if (TimeManager.instance == null)
            Instantiate(timeManager);

        if (KillCountManager.instance == null)
            Instantiate(killCountManager);

        if (SlashEffectManager.instance == null)
            Instantiate(slashEffectManager);
    }
}
