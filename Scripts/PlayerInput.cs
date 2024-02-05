using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public float horizontal;      //Float that stores horizontal input
    [HideInInspector] public float vertical;
    [HideInInspector] public bool jumpPressed;      //Bool that stores jump held

    [HideInInspector] public bool dashRight;
    [HideInInspector] public bool dashLeft;
    [HideInInspector] public bool attackA;
    [HideInInspector] public bool attackB;

    bool readyToClear;                              //Bool used to keep input in sync

    void Update()
    {

        // Don't read inputs while the game is paused
        if (GameManager.instance.IsGamePaused())
            return;
        
        //Clear out existing input values
        ClearInput();

        //Process keyboard, mouse, gamepad (etc) inputs
        ProcessInputs();

        //Clamp the horizontal input to be between -1 and 1
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        vertical = Mathf.Clamp(vertical, -1f, 1f);
    }

    void FixedUpdate()
    {
        //In FixedUpdate() we set a flag that lets inputs to be cleared out during the 
        //next Update(). This ensures that all code gets to use the current inputs
        readyToClear = true;
    }

    void ClearInput()
    {
        //If we're not ready to clear input, exit
        if (!readyToClear)
            return;

        //Reset all inputs
        horizontal = vertical = 0f;
        jumpPressed = false;
        dashRight = false;
        dashLeft = false;
        attackA = false;
        attackB = false;

        readyToClear = false;
    }

    void ProcessInputs()
    {
        //Accumulate horizontal axis input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //Accumulate button inputs
        jumpPressed = jumpPressed || Input.GetButtonDown("Jump");
        dashRight = dashRight || Input.GetButtonDown("Dash Right");
        dashLeft = dashLeft || Input.GetButtonDown("Dash Left");
        attackA = attackA || Input.GetButtonDown("Attack A");
        attackB = attackB || Input.GetButtonDown("Attack B");
    }
}
