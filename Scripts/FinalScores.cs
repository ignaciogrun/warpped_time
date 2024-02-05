using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FinalScores : MonoBehaviour
{
    void Start()
    {
        float timePassed = TimeManager.instance.GetTimePassed();
        float minutesPassed = Mathf.FloorToInt((timePassed % 3600) / 60);
        float secondsPassed = Mathf.FloorToInt(timePassed % 60);

        this.GetComponent<Text>().text = "Time took to complete game: " + minutesPassed + " minutes and " + secondsPassed + " seconds" + "\n Total Kills: " + KillCountManager.instance.GetKillCount();
    }
}
