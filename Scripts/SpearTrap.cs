using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    private PlayerController playerController;
    private bool isHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !isHit)
        {
            playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.TakeDamage(playerController.GetHealthData().GetMaxHealth());
            isHit = true;
            StartCoroutine(Wait());
        }

        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.SetActive(false);
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(2f);

            isHit = false;
        }
    }
}
