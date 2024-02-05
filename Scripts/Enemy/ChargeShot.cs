using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeShot : MonoBehaviour
{
    //Publics
    public float speed = .75f;
    public float boltSize = .75f;

    //Privates
    private Transform playerTarget;
    private Camera cam;
    private Rigidbody rigidBody;
    private EnemyWeapon weapon;

    //booleans
    public bool hasFired = false; //Currently Public for debug
    
    //Floats
    private float size;
    private float zKeep;

    // Start is called before the first frame update
    void Start()
    {
        //PLayer
        playerTarget = GameObject.Find("Player").transform.Find("Target Location");

        //Camera
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        //RigidBody
        rigidBody = GetComponent<Rigidbody>();

        //Weapon
        weapon = GetComponent<EnemyWeapon>();

        //Set Size to the local transform size
        size = transform.localScale.x;

        //Set z pos
        zKeep = transform.position.z;

        AudioManager.instance.Play("SkeletonWeapon");
    }

    // Update is called once per frame
    void Update()
    {
        //Keep Original Z Position
        transform.position = new Vector3(transform.position.x, transform.position.y, zKeep);

        //If the bolt has not fired at player yet
        if (!hasFired)
        {
            //Make sure weapon collider is disabled
            weapon.DisableEnemyWeaponCollider();

            //Did the bolt reach its fullsize
            if (transform.localScale.x < boltSize)
            {
                //Grow the bolt to it's full size
                size += size * Time.deltaTime;
                transform.localScale = new Vector3(size, size, size);
            }
            else
            {
                hasFired = true;
            }

            //Look At Players Current Position
            transform.LookAt(playerTarget);
        }

        //Don't update the look and just move forwards
        else
        {
            //If not on screen destroy the bolt
            if (!IsShotVisible())
            {
                weapon.DisableEnemyWeaponCollider();
                Destroy(this.gameObject);
            }

            //Enable Weapon Collider
            weapon.EnableEnemyWeaponCollider();

            //Move the arcane bolt forward in a straight line
            rigidBody.AddForce(transform.forward * speed, ForceMode.Impulse);
        }
    }

    //Check To See If Bolt Is On Screen
    bool IsShotVisible()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        return viewPos.x >= 0 && viewPos.x <= 1;
    }
}
