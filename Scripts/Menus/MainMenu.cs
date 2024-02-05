using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject credits;
    public GameObject exitConfirmation;
    public GameObject newGame;
    public GameObject loadGame;
    public GameObject settingsMenu;
    public GameObject overwriteMenu;

    private Text overwriteMenuText;

    private int overwriteSaveSlotInt = -1;

    void Start()
    {
        overwriteMenuText = overwriteMenu.GetComponentInChildren<Text>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.instance.ResetPlayerPosition();
    }

    public void LoadGame(int saveSlot)
    {
        if (!GameManager.instance.IsSlotOccupied(saveSlot))
            return;

        GameManager.instance.SetSaveSlot(saveSlot);
    }

    public void NewGame(int saveSlot)
    {
        if (GameManager.instance.IsSlotOccupied(saveSlot))
        {
            overwriteSaveSlotInt = saveSlot;
            overwriteMenu.SetActive(true);
            overwriteMenuText.text = "Overwrite Game Slot " + (saveSlot + 1) + "?";
            return;
        }

        GameManager.instance.SetSaveSlot(saveSlot);
    }

    public void ConfirmNewGameOverwrite()
    {
        if (overwriteSaveSlotInt == -1)
            return;

        GameManager.instance.SetSaveSlot(overwriteSaveSlotInt, true);
    }

    public void PlayPressSound()
    {
        //Input.GetButtonDown("Click");
        AudioManager.instance.Play("Player Click");
    }

    public void CloseMainMenu() //sets everything in the main menu to false
    {
        credits.SetActive(false);
        exitConfirmation.SetActive(false);
        newGame.SetActive(false); 
        loadGame.SetActive(false);
        settingsMenu.SetActive(false);
        overwriteMenu.SetActive(false);

        overwriteSaveSlotInt = -1;
    }

    public void DisplayNewGame()
    {
        CloseMainMenu();
        newGame.SetActive(true);
    }

    public void DisplayLoadGame()
    {
        CloseMainMenu();
        loadGame.SetActive(true);
    }

    public void DisplaySettings()
    {
        CloseMainMenu();
        settingsMenu.SetActive(true);
    }

    public void DisplayCredits()
    {
        CloseMainMenu();
        credits.SetActive(true);
    }

    public void DisplayExitConfirmation()
    {
        CloseMainMenu();
        exitConfirmation.SetActive(true);
    }

    public void CancelExit()
    {
        exitConfirmation.SetActive(false);
    }

    public void ConfirmExit()
    {
        Application.Quit(1);
    }

}
