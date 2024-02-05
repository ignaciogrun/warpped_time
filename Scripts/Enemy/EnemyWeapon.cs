using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{

    public string weaponSoundName;

    public float minDamage = 1f;
    public float maxDamage = 5f;

    //public AudioClip weaponSound;

    private Collider eCollider;
    //private AudioSource audioSource;

    private bool hitPlayer = false;
    
    void Start()
    {
        eCollider = GetComponent<Collider>();
        DisableEnemyWeaponCollider();
        //audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hitPlayer)
        {
            other.gameObject.SendMessage("TakeDamage", Random.Range(minDamage, maxDamage));
            hitPlayer = true;
            PlayWeaponSound();
        }

        if (other.gameObject.tag == "Floor")
        {
            PlayWeaponSound();
        }
    }

    void PlayWeaponSound()
    {
        //audioSource.PlayOneShot(weaponSound);
        AudioManager.instance.Play(weaponSoundName);
    }

    public void EnableEnemyWeaponCollider()
    {
        eCollider.enabled = true;
    }

    public void DisableEnemyWeaponCollider()
    {
        eCollider.enabled = hitPlayer = false;
    }

    public float GetMinDamage()
    {
        return minDamage;
    }

    public void SetMinDamage(float min)
    {
        this.minDamage = min;
    }

    public float GetMaxDamage()
    {
        return maxDamage;
    }

    public void SetMaxDamage(float max)
    {
        this.maxDamage = max;
    }
}
