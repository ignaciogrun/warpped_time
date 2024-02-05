using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public enum Direction { LEFT, RIGHT }
    public enum AttackType { A, B, NONE }

    [Header("Movement Speed Settings")]
    public float speed = 6.0f;
    public float rotationSpeed = 5.0f;
    public float airSpeed = 4.0f;

    [Header("Jump Settings")]
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float coyoteTime = 0.05f;

    [Header("Dash Settings")]
    public float dashDelta = 1.0f;
    public float dashSpeed = 12.0f;

    public float maxHealth = 50;
    public float flashDelta = 0.35f;
    public float attackBonusUpwardVelocity = 1f;

    [HideInInspector] public Vector3 moveDirection = Vector2.zero;

    // Z Position To Keep Player On
    [HideInInspector] public float zKeep;

    private float airTime;

    private Quaternion targetRotation;

    private AttackType queuedAttack = AttackType.NONE;
    private bool comboWindowActive = false;

    private bool facingRight = true;
    private bool canDoubleJump = true;
    private bool canDash = true;
    private bool isDashing = false;

    private bool isTeleporting = false;

    private PlayerInput input;
    private CharacterController controller;
    private CameraFollow mainCamera;
    private Animator anim;
    private PlayerWeapon playerWeapon;
    private Health health;
    private GameObject swordTrail;

    private Color originalColor;
    private Color hurtColor = new Color(1f, .3f, .3f, .85f);
    private SkinnedMeshRenderer meshRenderer;

    public AudioSource runAudio;

    [Space] [Space]
    public GameObject blood;
    public GameObject dust;

    [HideInInspector] public bool dead = false;

    public RawImage damageImage;    // Reference to an image to flash on the screen on being hurt.
    public float flashSpeed = 50f; // The speed the damageImage will fade at.
    public Color flashColour;     // The colour the damageImage is set to, to flash.
    private float alpha = 100f;
    private float finalAlpha = .3f;

    public Color color;

    void Awake()
    {   
        // Components
        input = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        // Children Components
        playerWeapon = GetComponentInChildren<PlayerWeapon>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // Grabs Original Values
        originalColor = meshRenderer.material.color;
        targetRotation = transform.rotation;
        zKeep = transform.position.z;

        health = new Health(maxHealth);
    }

    // Start is called before the first frame update
    void Start()
    {
        // External Components
        mainCamera = GameObject.Find("Main Camera").GetComponent<CameraFollow>();
        
        Physics.IgnoreLayerCollision(13, 14);

        // Lock Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (Light l in GameObject.FindObjectsOfType<Light>())
            if (l.name == "Directional Light")
                l.color = new Color(PlayerPrefs.GetFloat("Light"), PlayerPrefs.GetFloat("Light"), PlayerPrefs.GetFloat("Light"), 1);

        foreach (SpriteRenderer sr in GameObject.FindObjectsOfType<SpriteRenderer>())
            if (sr.name == "Brightness Screen")
            {
                color = sr.color;
                color.a = PlayerPrefs.GetFloat("Brightness Alpha");
                sr.color = color;
            }
                

        foreach (RawImage image in FindObjectsOfType<RawImage>())
            if (image.name == "edgeTransparency")
                damageImage = image;

        if (GameManager.instance.GetPlayerStartPosition() != Vector3.zero && GameManager.instance.GetPlayerStartScene() == SceneManager.GetActiveScene().name)
        {
            controller.enabled = false;
            controller.transform.position = GameManager.instance.GetPlayerStartPosition();
            controller.enabled = true;
        }

        swordTrail = GameObject.Find("SwordTrail");

        GameManager.instance.ResumeGame();
    }

    void Update()
    {
        // Exit out of loop if paused
        if (GameManager.instance.IsGamePaused())
            return;

        Gravity(); // Apply gravity
        
        if (dead)
            return;
        
        Movement(); // Apply ALL X and Z Movement
        Jump(); // Jump
        UpdateFacing(); // Update direction player is facing
        Attack(); // Have player Attack
        
        if (controller.enabled)
            controller.Move(moveDirection * Time.deltaTime); // Move the controller

        #region HealthEdge
        //Red Edge for low health for when current health is at 30%
        if (health.HealthPercentage() <= .3 && damageImage != null) {
            finalAlpha = (alpha - health.GetHealth()) / 100;
            flashColour = new Color(255, 255, 255, finalAlpha); // ... set the alpha of the damageImage.
            damageImage.color = flashColour;
        }
        // Otherwise...
        else if (damageImage != null)
        {
            // ... transition the colour back to clear.
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        #endregion
    }

    private void Movement()
    {
        if (!isTeleporting)
        {
            // Lock Z Position
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, zKeep);
            controller.enabled = true;

            // Dash
            if (!isDashing && canDash && (input.dashLeft || input.dashRight))
                if (!(input.dashLeft && input.dashRight))
                    StartCoroutine(Dash(input.dashLeft ? Direction.LEFT : Direction.RIGHT));

            bool grounded = controller.isGrounded;
            anim.SetFloat("Speed", Mathf.Abs(input.horizontal));

            // Prevent Player Moving while dashing
            if (isDashing)
                return;

            // Ground Movement
            if (grounded)
            {
                // Reset Conditions
                airTime = 0.0f;
                canDoubleJump = true;
                canDash = true;

                moveDirection.x = input.horizontal * speed;
                anim.SetBool("Land", true);

                runAudio.volume = Mathf.Abs(controller.velocity.x);
            }

            // Air Movement
            else
            {
                airTime += Time.deltaTime;
                moveDirection.x = input.horizontal * airSpeed;
                runAudio.volume = Mathf.Lerp(runAudio.volume, 0, 5 * Time.deltaTime);
            }
        }
    }

    private void Jump()
    {
        // Prevent Player Jumping while dashing
        if (isDashing)
            return;

        if (input.jumpPressed)
        {
            if (controller.isGrounded || airTime <= coyoteTime) // First Jump Check
            {
                StartCoroutine(CheckHeadCollision());
                moveDirection.y = jumpSpeed;
                anim.SetTrigger("Jump");
                anim.SetBool("Land", false);
            }

            else if (canDoubleJump) // Second Jump Check
            {
                moveDirection.y = jumpSpeed;
                canDoubleJump = false;
                anim.SetTrigger("Jump Double");
                anim.SetBool("Land", false);
            }
        }
    }

    // Updates the direction the player is facing
    private void UpdateFacing()
    {
        if (!isTeleporting && !isDashing && (input.horizontal < 0 && facingRight) || (input.horizontal > 0 && !facingRight))
        {
            facingRight = !facingRight;
            targetRotation = Quaternion.Inverse(targetRotation);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
    }

    // Applies a gravity to the player
    private void Gravity()
    {
        if (!controller.isGrounded)
            moveDirection.y = moveDirection.y - (gravity * Time.deltaTime) * (isDashing ? 0.5f : 1);

        else if (moveDirection.y < 0)
            moveDirection.y = 0;
    }

    private IEnumerator Dash(Direction dir)
    {
        isDashing = true;
        canDash = false;

        queuedAttack = AttackType.NONE;
        anim.ResetTrigger("Attack A");
        anim.ResetTrigger("Attack B");
        anim.SetBool("Reset", true);

        DisableComboWindow();
        DisableSwordCollider();

        controller.height /= 2;
        controller.center = new Vector3(controller.center.x, controller.center.y / 2, controller.center.z);

        anim.SetTrigger(dir == Direction.RIGHT ? (facingRight ? "Dash Forward" : "Dash Back") : facingRight ? "Dash Back" : "Dash Forward");

        float speedWithDirection = (dir == Direction.RIGHT ? dashSpeed : -dashSpeed);
        float time = 0.0f;

        while (dashDelta > time)
        {
            controller.Move(new Vector3(speedWithDirection * Time.deltaTime, 0f, 0f));

            time += Time.deltaTime;
            yield return null;
        }

        anim.SetBool("Reset", false);

        controller.height *= 2;
        controller.center = new Vector3(controller.center.x, controller.center.y * 2, controller.center.z);

        isDashing = false;
    }

    private IEnumerator FlashEffect()
    {
        meshRenderer.material.color = hurtColor;

        float time = 0.0f;
        while (flashDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        meshRenderer.material.color = originalColor;
    }

    private IEnumerator CheckHeadCollision()
    {
        while (!controller.isGrounded)
        {
            Vector3 top = controller.transform.position + new Vector3(0, controller.height * 0.5f);

            RaycastHit hit;
            if (Physics.Raycast(top, Vector3.up, out hit, 0.08f) && moveDirection.y > 0)
            {
                moveDirection.y = 0;
                break;
            }

            yield return null;
        }
    }

    private void Attack()
    {
        if ((input.attackA && input.attackB) || isDashing)
            return;

        if (input.attackA || queuedAttack == AttackType.A)
        {
            if (comboWindowActive)
            {
                queuedAttack = AttackType.A;
                DisableComboWindow();
                return;
            }

            anim.SetTrigger("Attack A");
            queuedAttack = AttackType.NONE;
        }

        else if (input.attackB || queuedAttack == AttackType.B)
        {
            if (comboWindowActive)
            {
                queuedAttack = AttackType.B;
                DisableComboWindow();
                return;
            }

            anim.SetTrigger("Attack B");
            queuedAttack = AttackType.NONE;
        }
    }

    public void TakeDamage(float dmg)
    {
        if (dead)
            return;

        health.Damage(dmg);
        if (health.GetHealth() <= 0)
        {
            dead = true;
            anim.SetBool("Death", true);
            this.GetComponent<PlayerEnemyPool>().ResetAllEnemies();
            return;
        }

        StartCoroutine(FlashEffect());
        Instantiate(blood, transform.position + new Vector3(0, 1.2f, -0.2f), transform.rotation);

        anim.SetFloat("Min Hurt Choice", dmg);
        anim.SetFloat("Max Hurt Choice", dmg);
        anim.SetTrigger("Hurt");

        mainCamera.ShakeCamera(0.065f, 0.1f);
    }

    // Slight Upward Affect On Sword Slash
    public void UpwardVelocity()
    {
        if (!controller.isGrounded)
            moveDirection.y = Mathf.Max(attackBonusUpwardVelocity, moveDirection.y + attackBonusUpwardVelocity);
    }

    

    #region Getter/Setters

    public Animator GetAnimator()
    {
        return anim;
    }

    public void SetDead(bool isDead)
    {
        dead = isDead;
    }

    public Health GetHealthData()
    {
        return health;
    }

    public void SetIsTeleporting(bool op)
    {
        isTeleporting = op;
    }

    public void SetSwordTrailActive(bool active)
    {
        swordTrail.SetActive(active);
    }

    #endregion

    // Animation Call Functions
    #region Animation Functions

    public void AfterDeath()
    {
        GameManager.instance.DisplayDeathMenu();
    }

    public void EnableComboWindow()
    {
        comboWindowActive = true;
        UpwardVelocity();
    }

    public void DisableComboWindow()
    {
        comboWindowActive = false;
    }

    public void EnableSwordCollider()
    {
        playerWeapon.EnableWeapon();
    }

    public void DisableSwordCollider()
    {
        playerWeapon.DisableWeapon();
        //playerWeapon.SetHitEnemy(false);
    }

    #endregion

    // Audio Playback
    #region Audio Playback Functions

    public void PlaySwordSwish()
    {
        AudioManager.instance.Play("Swish");
    }
    public void PlayJumpSound()
    {
        AudioManager.instance.Play("Jump");
    }
    public void PlayDoubleJumpSound()
    {
        AudioManager.instance.Play("Double Jump");
    }
    public void PlayLandSound()
    {
        Instantiate(dust, transform.position + new Vector3(0, 0.1f, -0.2f), transform.rotation);
        AudioManager.instance.Play("Land");
    }
    public void PlayRollSound()
    {
        AudioManager.instance.Play("Roll");
    }
    public void PlaySmallGrunt()
    {

        AudioManager.instance.Play("Small Grunt");
    }
    public void PlayMediumGrunt()
    {
        AudioManager.instance.Play("Medium Grunt");
    }
    public void PlayLargeGrunt()
    {
        AudioManager.instance.Play("Large Grunt");
    }

    #endregion

}
