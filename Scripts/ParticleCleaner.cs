using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCleaner : MonoBehaviour
{
    

    void Start()
    {
        Destroy(this, 5);
    }

}