using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private BoxCollider eCollider;
    private GameObject player;

    private string attackName;

    //private bool enemyHit = false;

    void Start()
    {
        eCollider = GetComponent<BoxCollider>();
        player = GameObject.Find("Player");

        DisableWeapon();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            //SlashEffectManager.instance.PlaySlash();
            player.SendMessage("UpwardVelocity");
        } 
    }

    public void EnableWeapon()
    {
        eCollider.enabled = true;
    }

    public void DisableWeapon()
    {
        eCollider.enabled = false;
    }

    public void SetCurrentAttack(string name)
    {
        attackName = name;
    }
}
