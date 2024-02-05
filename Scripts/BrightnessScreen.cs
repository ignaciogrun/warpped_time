using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessScreen : MonoBehaviour
{
    Color dark;

    // Start is called before the first frame update
    void Start()
    {
        dark = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
