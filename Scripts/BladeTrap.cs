using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeTrap : MonoBehaviour
{

    //Publics
    public float damage = 5f;
    public float upwardMovement;


    //Privates
    private bool hasBeenShotUpward = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasBeenShotUpward)
        {
            other.SendMessage("TakeDamage", damage);
            other.gameObject.GetComponent<PlayerController>().moveDirection.y = upwardMovement;
            hasBeenShotUpward = true;
            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(.1f);

        hasBeenShotUpward = false;
    }
}
