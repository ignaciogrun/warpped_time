using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    //used to make sure there is only one AudioManager per scene
    public static AudioManager instance;
    
    public Sound[] sounds;

    private string lastKnownScene = null;

    void Awake()
    {

        // If there is two AudioManagers in one scene, delete this one
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Makes it so that the AudioManager can transfer through the different scenes
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.output;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }
    
    public void Update()
    {
        //SetSliders();

        string currentScene = SceneManager.GetActiveScene().name;
        if (lastKnownScene != currentScene || lastKnownScene == null)
        {
            lastKnownScene = currentScene;
            ChangeSong();
        }
    }

    public void ChangeSong()
    {
        if (lastKnownScene == "MainMenu")
        {
            Stop();
            Play("Main Menu");
        }
        else if (lastKnownScene == "Forest Level")
        {
            Stop();
            Play("Forest Level");
        }
        else if (lastKnownScene == "Ignacio Working Scene")
        {
            Stop();
            Play("Temple Level");
        }
    }

    //Find the sound in the sound array and play it
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null && s.source != null)
            s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null && s.source != null)
            foreach (Sound sound in sounds)
                if (s.name == sound.name)
                    s.source.Stop();
    }

    public void Stop()
    {
        foreach (Sound s in sounds)
            s.source.Stop();
    }

}
