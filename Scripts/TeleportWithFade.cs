using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleportWithFade : MonoBehaviour
{
    /* To use the teleport with fade you must have both the teleport layer 
       and tag on both the teleporter and teleportLocation object */

    
    //Publics
    [Header("Fade Settings")]
    public Image fadeImage;

    [Range(0.0f, 1.0f)]
    public float fadeSpeed = .7f;

    [Header("Locations")]
    [SerializeField] public Transform teleportLocation;

    //Private
    private GameObject mainCamera;
    private PlayerController playerController;
    private CharacterController controller;
    private Transform Player;


    //Private Booleans
    private bool isInTeleporter = false;

    //Private Floats
    private float alphaValue;

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        Player = GameObject.Find("Player").GetComponent<Transform>();
        controller = GameObject.Find("Player").GetComponent<CharacterController>();
    }

    //Set if the player is in the teleporter
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isInTeleporter = true;
        }
    }

    void Update()
    {
        if (isInTeleporter)
        {
            if (Input.GetButtonDown("Teleport") && alphaValue <= 0 && isInTeleporter)
            {
                StartCoroutine(Fade());
                AudioManager.instance.Play("Teleporter");
            }
        }

        isInTeleporter = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isInTeleporter = false;
        }
    }

    IEnumerator Fade()
    {
        playerController.SetIsTeleporting(true);
        controller.enabled = false;

        while (alphaValue < 1f)
        {
            alphaValue += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0f, 0f, 0f, alphaValue);
            yield return null;
        }

        mainCamera.transform.position = new Vector3(teleportLocation.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        Vector3 destination = new Vector3(teleportLocation.position.x, teleportLocation.position.y, 0);
        isInTeleporter = false;
        Player.position = destination;

        controller.enabled = true;
        playerController.SetIsTeleporting(false);

        while (alphaValue > 0)
        {
            alphaValue -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0f, 0f, 0f, alphaValue);
            yield return null;
        }
    }
}

