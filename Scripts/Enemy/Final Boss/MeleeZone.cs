using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZone : MonoBehaviour
{

    private bool containsPlayer = false;
    private bool stayActive = false;

    void FixedUpdate()
    {
        if (stayActive)
        {
            containsPlayer = true;
            stayActive = false;
        }

        else
            containsPlayer = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            stayActive = true;
    }

    public bool ContainsPlayer()
    {
        return containsPlayer;
    }

}
