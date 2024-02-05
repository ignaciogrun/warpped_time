using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    public GameObject effects;

    private Vector3 startRotation;

    void Start()
    {
        startRotation = effects.transform.eulerAngles;
    }
    
    void Update()
    {
        effects.transform.eulerAngles = new Vector3(effects.transform.eulerAngles.x, startRotation.y, startRotation.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.SendMessage("TakeDamage", 10);
            this.enabled = false;
            Destroy(this.gameObject);
            AudioManager.instance.Play("Fireball Impact");
        }

        else if (other.CompareTag("Floor"))
        {
            this.enabled = false;
            Destroy(this.gameObject);
            AudioManager.instance.Play("Fireball Impact");
        }
    }

}
