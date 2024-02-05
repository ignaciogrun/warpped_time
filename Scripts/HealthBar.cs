using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    PlayerController player;
    Image healthBarSize;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        healthBarSize = GetComponent<Image>();
    }
    
    // Update is called once per frame
    void Update()
    {
        healthBarSize.fillAmount = player.GetHealthData().HealthPercentage();
    }
}
