using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPrompt : MonoBehaviour
{
    public Canvas prompt;
    private bool inCollider = false;

    //Set if the player is in the teleporter
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            inCollider = true;
    }

    void Update()
    {
        prompt.gameObject.SetActive(inCollider);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            inCollider = false;
    }
}
