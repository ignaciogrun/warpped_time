using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScroll : MonoBehaviour
{
    //time before starting to scroll
    public float scrollTime = 2.0f;
    //rate at which it scrolls
    public float scrollSpeed = 0.5f;

    public Text text;

    void Update()
    {
        scrollTime -= Time.deltaTime;

        if(scrollTime < 0)
        {
            text.transform.Translate(Vector3.up * scrollSpeed);
        }

    }
}
