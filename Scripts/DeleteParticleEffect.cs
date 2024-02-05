using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteParticleEffect : MonoBehaviour
{
    private float targetTime;


    void Start()
    {
        targetTime = 3;
    }

    // Update is called once per frame
    void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
