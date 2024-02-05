using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private PlayerController player;
    private GameObject mainCamera;

    private Vector3 savedCheckPoint;
    private float savedHealth;
    
    private FinalBoss demonBladeLord;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        mainCamera = GameObject.Find("Main Camera");
        GameObject demonBladeLordObject = GameObject.Find("Demon Blade Lord");
        if (demonBladeLordObject != null)
        {
            demonBladeLord = demonBladeLordObject.GetComponent<FinalBoss>();
            demonBladeLordObject.SetActive(false);
        }
    }

    private void SaveCurrentStats()
    {
        savedCheckPoint = new Vector3(player.gameObject.transform.position.x, player.gameObject.transform.position.y, player.zKeep);
        GameManager.instance.Save(player.gameObject.transform.position);

        savedHealth = player.GetHealthData().GetMaxHealth();
        player.GetHealthData().SetHealth(savedHealth);
    }

    public void SetCurrentStats()
    {
        player.SetSwordTrailActive(false);
        player.gameObject.GetComponent<CharacterController>().enabled = false;
        player.transform.position = savedCheckPoint;
        //Save_Load.LoadGame(PlayerPrefs.GetFloat("currentSaveSlot"));
        player.gameObject.GetComponent<CharacterController>().enabled = true;

        // TODO: Translate camera additionally
        mainCamera.transform.position = new Vector3(savedCheckPoint.x, mainCamera.transform.position.y, mainCamera.transform.position.z);

        //Set animation boolean to false for death
        player.GetHealthData().SetHealth(savedHealth);
        player.SetDead(false);
        player.GetAnimator().SetBool("Death", false);

        if (demonBladeLord != null && demonBladeLord.gameObject.activeSelf)
        {
            demonBladeLord.Reset();
            demonBladeLord.gameObject.SetActive(false);
        }

        player.SetSwordTrailActive(true);
        GameManager.instance.Save(savedCheckPoint);
        //Turns off the death menu and resumes the game
        GameManager.instance.HideDeathMenu();
    }
}
