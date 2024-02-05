using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Ground,
        Flying,
        
        Chasing
    }

    public enum CombatType
    {
        Melee,
        Ranged
    }

    public enum State
    {
        Keep_Distance,
        Get_Close,
        Attack,
        Dead
    }

    public EnemyType type;
    public float speed = 1.5f;
    public float keepDistanceRange = 4.0f; // DEBUG
    public float maxHealth = 10;
    public float knockBack = 10f;

    private Rigidbody rigBody;
    private CapsuleCollider capsuleCollider;

    private GameObject player;
    private CharacterController controller;

    private Vector3 targetPostition;
    private Vector3 smoothVelocity = Vector3.zero;
    private Vector3 viewPos;

    private Camera cam;
    
    private float distance;
    
    // Chase's Crap
    [Range(0.0f, 10f)]
    public float jumpDelta = 3.5f;

    [Range(0.5f, 20f)]
    public float attackRange = 0.5f;

    public float flashDelta = 0.35f;
    
    [HideInInspector] public State state;
    [HideInInspector] public float xOffset;
    [HideInInspector] public float lastAttack = 0.0f;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool dead = false;
    [HideInInspector] public bool dying = false;


    private Animator anim;

    private Vector3 spawnPos;
    
    private bool active = false;
    

    private float lastVisable = 0.0f;
    private float lastJump = 0.0f;
    private float animSpeed;
    private float flashTime;
    
    private Vector3 target;
    private Vector3 desiredVelocity;
    

    //TEST AREA
    private Color originalColor;
    private Color hurtColor = new Color(1f, .3f, .3f, .85f);
    private SkinnedMeshRenderer meshRenderer;


    // Start is called before the first frame update
    void Awake()
    {
        spawnPos = transform.position;
        
        player = GameObject.Find("Player");
        controller = player.GetComponent<CharacterController>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        rigBody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        currentHealth = maxHealth;
        originalColor = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForDeath();
        
        if (!active)
            return;

        if (meshRenderer.material.color == hurtColor)
        {
            flashTime += Time.deltaTime;
            if (flashTime >= flashDelta)
                meshRenderer.material.color = originalColor;
        }

        if (!IsEnemyVisible())
        {
            lastVisable += Time.deltaTime;
            if (lastVisable > 1.0f)
            {
                Deactivate();
                return;
            }
        }
        else
            lastVisable = 0f;
        
        
        target = player.transform.position;
        if (state == State.Keep_Distance)
        {
            if (distance > keepDistanceRange)
                target += new Vector3(keepDistanceRange, 0);
            
            else if (distance < -keepDistanceRange)
                target -= new Vector3(keepDistanceRange, 0);
        }
        
        else if (state == State.Get_Close || state == State.Attack)
            target += new Vector3(xOffset, 0);
        
        desiredVelocity = (target - transform.position).normalized * speed;
        desiredVelocity.y = 0;

        animSpeed = desiredVelocity.x - 6;
        anim.SetFloat("Speed", Mathf.Abs(animSpeed));
    }

    void FixedUpdate()
    {
        CheckForDeath();

        if (!active)
            return;
        
        switch (type)
        {
            case EnemyType.Chasing:
                ChasePlayer();
                break;
            
            case EnemyType.Ground:
                GroundMelee();
                break;
        }
    }

    //Chase The Player
    void ChasePlayer()
    {

        LookAtPlayer();

        FindDistance();
        
        //transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref smoothVelocity, speed);
        //Debug.Log(transform.forward * speed);
        rigBody.AddForce(transform.forward * speed, ForceMode.Impulse);
    }
    
    void GroundMelee()
    {
        if (!dead)
        {
            MoveToDesiredPosition();
            LookAtPlayer();

            if (state == State.Attack)
            {
                if (!attacking)
                {
                    anim.SetInteger("AttackChoice", Random.Range(1, 4));
                    anim.SetTrigger("Attack");
                    attacking = true;
                }
                attacking = false;
                state = State.Get_Close;
            }
        }
    }

    void MoveToDesiredPosition()
    {
        lastJump += Time.deltaTime;

        if (!IsGrounded() || lastJump < 0.1f || attacking)
            return;

        float xDistanceToDesired = target.x - transform.position.x;
        float angle = 65;
        
        if (lastJump >= jumpDelta && Mathf.Abs(xDistanceToDesired) > 6 && RoomToJump())
        {

            if (xDistanceToDesired > 12)
                angle = 35;
            
            float iVelocity = Mathf.Sqrt(Mathf.Abs(-2 * Physics.gravity.y * xDistanceToDesired));
            float velocityX = iVelocity * Mathf.Cos(angle * Mathf.Deg2Rad);
            float velocityY = Mathf.Abs(iVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

            if (target.x - transform.position.x < 0)
                velocityX = -velocityX;

            lastJump = 0.0f;
            
            //Debug.Log("Vx: " + velocityX + " Vy: " + velocityY);
            
            rigBody.velocity = new Vector3(velocityX, velocityY);
            return;
        }
        
        rigBody.velocity = desiredVelocity;
    }

    bool IsGrounded()
    {
        Vector3 bottom = capsuleCollider.transform.position - new Vector3(0, (capsuleCollider.radius * .25f));
        bottom.y = bottom.y + (capsuleCollider.height * .65f);

        bool ray = Physics.Raycast(bottom, Vector3.down, 0.25f);
        
        Debug.DrawRay(bottom, new Vector3(0, -0.25f), ray ? Color.green : Color.red);

        return ray;
    }

    bool RoomToJump()
    {
        Vector3 top = transform.position + new Vector3(0, (capsuleCollider.height * 0.5f)); 
        return !Physics.Raycast(top, Vector3.up, 5f);
    }

    public bool CanAttack()
    {

        bool b = state == State.Get_Close &&
                 Mathf.Abs(player.transform.position.x - transform.position.x) <= attackRange
                           && Mathf.Abs(player.transform.position.y - transform.position.y) <= 2;

        return b;
    }

    public void FinishedAttack()
    {
        attacking = false;
        state = State.Get_Close;
    }

    //Look at player position
    void LookAtPlayer()
    {
        targetPostition = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);
        transform.LookAt(targetPostition);
    }
    
    //Find Distance Between Enemy and Player
    void FindDistance()
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
    }

    //Check to see if enemy is in camera
    public bool IsEnemyVisible()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        if (viewPos.x >= 0 && viewPos.x <= 1)
            return true;
        
        return false;
    }

    public bool IsActive()
    {
        return active;
    }

    public void Activate()
    {
        lastVisable = 0.0f;
        active = true;
    }

    public void Deactivate()
    {
        active = false;
        rigBody.velocity = Vector3.zero;
        transform.position = spawnPos;

        state = State.Keep_Distance;
        
        LookAtPlayer();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        FlashRed();
        //Vector3 moveDirection = new Vector3(-.5f, .2f, 0);
        //rigBody.AddForce(moveDirection * knockBack, ForceMode.Impulse);
        CheckForDeath();
    }

    void CheckForDeath()
    {
        if (!dying && !dead)
        {
            if (currentHealth <= 0)
            {
                dead = true;
                dying = true;
                anim.SetTrigger("Died");
                state = State.Dead;
            }
        }
    }

    public void FinishedDeath()
    {
        dying = false;
        anim.SetBool("Already Dead", dead);
    }

    public void DestroyEnemy()
    {
        if (dead && !dying)
            Destroy(this.gameObject);
    }

    private void DealDamageToPlayer()
    {
        if (player.transform.position.x - transform.position.x <= attackRange)
            player.SendMessage("TakeDamage", Random.Range(1,9));
    }

    private void FlashRed()
    {
        meshRenderer.material.color = hurtColor;
        flashTime = 0;
    }
}
