using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckPoint : MonoBehaviour
{
    private CheckPointManager manager;
    private BoxCollider eCollider;
    private Light cLight;

    void Start()
    {
        manager = GameObject.Find("CheckPointManager").GetComponent<CheckPointManager>();
        eCollider = GetComponent<BoxCollider>();
        cLight = GetComponentInChildren<Light>();

        cLight.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            manager.SendMessage("SaveCurrentStats", other.gameObject.GetComponent<PlayerController>());
            cLight.enabled = true;
            eCollider.enabled = false;
        }
    }
}
