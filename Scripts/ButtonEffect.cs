using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour
{

    public int saveSlot;

    private Button button;
    private Text buttonText;

    void Awake()
    {
        button = this.GetComponent<Button>();
        buttonText = this.GetComponentInChildren<Text>();
    }

    void Start()
    {
        if (!GameManager.instance.IsSlotOccupied(saveSlot))
        {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = new Color(1, 1, 1, 0);
            colorBlock.highlightedColor = Color.gray;

            button.colors = colorBlock;
            buttonText.color = new Color(1, 1, 1, 0.75f);
        } 
    }
}
