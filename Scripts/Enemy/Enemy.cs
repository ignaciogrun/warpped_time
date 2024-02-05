using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour, IEnemy
{

    // Public Members
    public bool debug = false;

    [Header("Attack Range")]
    public Vector2 minAttackRange = new Vector2(1.0f, 1.0f);
    public Vector2 maxAttackRange = new Vector2(2.0f, 2.0f);

    [Header("Keep Distance")]
    public Vector2 minKeepDistance = new Vector2(4.0f, 4.0f);
    public Vector2 maxKeepDistance = new Vector2(6.0f, 6.0f);

    [Header("Effect Settings")]
    public float hitFlashDelta = 0.2f;
    public Color hitFlashColor = new Color(1f, .3f, .3f, .85f);
    
    [Header("Other Data")]
    public Vector2 damageRange = new Vector2(1.0f, 3.0f);
    public Vector2 knockbackVelocity = new Vector2(2.5f, 4.0f);
    public float speed = 5.0f;
    public float disappearDelta = 1.5f;
    public float rotationSpeed = 18f;

    [Header("Health Settings")]
    public float maxHealth = 20.0f;
    public Image healthImage;

    // Private Members

    private Rigidbody rigbody;
    private CapsuleCollider eCollider;
    private Animator anim;
    private SkinnedMeshRenderer meshRenderer;
    private Color originalColor;
    private Camera cam;
    private Health health;
    private EnemyWeapon[] enemyWeapons;
    private EnemyGroup enemyGroup;

    private GameObject player;
    private CharacterController playerController;

    private State state;
    private PlayerEnemyPool.OffsetDirection offsetDirection;

    private Vector3 spawnPos;
    private Quaternion desiredRotation;
    

    private float hitFlashTime;
    private float lastVisible;
    private float lastAttack;
    private float zKeep;
    private bool active;
    private bool attacking;
    private bool isFlashColor;
    private bool amHit;
    private bool facingRight;

    // Enemy States
    public enum State { Keep_Distance, Get_Close, Attack, Dead }

    //Audio
    private AudioSource audioSource;

    public void Awake()
    {
        // Grab Enemy Components
        rigbody = GetComponent<Rigidbody>();
        eCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        originalColor = meshRenderer.material.color;
        health = new Health(maxHealth);
        enemyWeapons = GetComponentsInChildren<EnemyWeapon>();
        audioSource = GetComponent<AudioSource>();
        enemyGroup = GetComponentInParent<EnemyGroup>();

        // Grab Player Components
        player = GameObject.Find("Player");
        playerController = player.GetComponent<CharacterController>();

        // Set Default Values
        state = State.Keep_Distance;
        spawnPos = transform.position;
        desiredRotation = transform.rotation;
        lastVisible = 0.0f;
        lastAttack = 0.0f;
        hitFlashTime = 0.0f;
        zKeep = transform.position.z;
        isFlashColor = false;
        active = false;
        attacking = false;
        amHit = false;
        facingRight = false;
    }

    public void Update()
    {
        if (debug)
        {
            DrawAttackRange();
            DrawKeepDistance();
        }

        if (!active)
            return;

        if (!IsEnemyVisible())
        {
            lastVisible += Time.deltaTime;
            if (lastVisible >= disappearDelta && enemyGroup.canReturnEnemies)
                Deactivate();
        }

        else
            lastVisible = 0f;

        healthImage.fillOrigin = player.transform.position.x <= transform.position.x ? 0 : 1;
        
        transform.position = new Vector3(transform.position.x, transform.position.y, zKeep);

        // E5 P10 E -Right
        // 

        if ((transform.position.x < player.transform.position.x && !facingRight) ||
            (transform.position.x > player.transform.position.x && facingRight))
        {
            facingRight = !facingRight;
            desiredRotation = Quaternion.Inverse(desiredRotation);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, rotationSpeed);
        lastAttack += Time.deltaTime;
    }

    public void FixedUpdate()
    {
        if (!active)
            return;
        
        if (isFlashColor)
        {
            if (hitFlashTime >= hitFlashDelta)
            {
                meshRenderer.material.color = originalColor;
                hitFlashTime = 0.0f;
                isFlashColor = false;
            }

            else
                hitFlashTime += Time.deltaTime;
        }

        switch (state)
        {
            case State.Keep_Distance:
                KeepDistance();
                break;

            case State.Get_Close:
                GetClose();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Dead:
                break;
        }
    }
    
    public abstract void KeepDistance();

    public abstract void GetClose();

    public abstract void Attack();

    public abstract void AfterAttack();

    public abstract void OnHit(float damage);

    public abstract void OnDeath();

    public abstract void AfterDeath();

    private bool IsEnemyVisible()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        return viewPos.x >= 0 && viewPos.x <= 1;
    }

    public bool IsGrounded()
    {
        Vector3 bottom = eCollider.transform.position - new Vector3(0, (eCollider.radius * .25f));
        bottom.y = bottom.y + (eCollider.height * .65f);

        return Physics.Raycast(bottom, Vector3.down, 0.25f);
    }

    public bool IsPlayerInAttackRange()
    {
        Vector3 minAttack = transform.position;
        Vector3 maxAttack = transform.position;
        
        if (transform.forward.x <= 0)
        {
            minAttack += new Vector3(-maxAttackRange.x, minAttackRange.y);
            maxAttack += new Vector3(-minAttackRange.x, maxAttackRange.y);
        }

        else
        {
            minAttack += new Vector3(minAttackRange.x, minAttackRange.y);
            maxAttack += new Vector3(maxAttackRange.x, maxAttackRange.y);
        }

        return IsVectorBetween(player.transform.position, minAttack, maxAttack);
    }

    //For Multiple enemies to take damage in one swing
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword") && !amHit)
        {
            OnHit(Random.Range(1, 5));
            amHit = true;
            StartCoroutine(WaitTillHitAgain());
        }
            
    }

    private void DrawAttackRange()
    {
        Vector3 minAttack = transform.position;
        Vector3 maxAttack = transform.position;

        if (transform.forward.x <= 0)
        {
            minAttack += new Vector3(-maxAttackRange.x, minAttackRange.y);
            maxAttack += new Vector3(-minAttackRange.x, maxAttackRange.y);
        }

        else
        {
            minAttack += new Vector3(minAttackRange.x, minAttackRange.y);
            maxAttack += new Vector3(maxAttackRange.x, maxAttackRange.y);
        }

        Debug.DrawRay(minAttack, new Vector3((maxAttack.x - minAttack.x), 0, 0), Color.red);
        Debug.DrawRay(minAttack, new Vector3(0, (maxAttack.y - minAttack.y), 0), Color.red);

        Debug.DrawRay(maxAttack, new Vector3((minAttack.x - maxAttack.x), 0, 0), Color.red);
        Debug.DrawRay(maxAttack, new Vector3(0, (minAttack.y - maxAttack.y), 0), Color.red);
    }

    private void DrawKeepDistance()
    {
        Vector3 minKeep = GetPlayer().transform.position;
        Vector3 maxKeep = GetPlayer().transform.position;

        if (GetPlayer().transform.position.x > transform.position.x)
        {
            minKeep += new Vector3(-maxKeepDistance.x, minKeepDistance.y);
            maxKeep += new Vector3(-minKeepDistance.x, maxKeepDistance.y);
        }

        else
        {
            minKeep += new Vector3(minKeepDistance.x, minKeepDistance.y);
            maxKeep += new Vector3(maxKeepDistance.x, maxKeepDistance.y);
        }

        Debug.DrawRay(minKeep, new Vector3((maxKeep.x - minKeep.x), 0, 0), Color.blue);
        Debug.DrawRay(minKeep, new Vector3(0, (maxKeep.y - minKeep.y), 0), Color.blue);

        Debug.DrawRay(maxKeep, new Vector3((minKeep.x - maxKeep.x), 0, 0), Color.blue);
        Debug.DrawRay(maxKeep, new Vector3(0, (minKeep.y - maxKeep.y), 0), Color.blue);
    }

    public void Activate()
    {
        lastVisible = 0.0f;
        active = true;
    }

    public void Deactivate()
    {
        state = State.Keep_Distance;
        lastVisible = 0.0f;
        transform.position = spawnPos;
        health.SetHealth(maxHealth);
        UpdateHealthBarFill();
        active = false;
    }

    IEnumerator WaitTillHitAgain()
    {
        yield return new WaitForSeconds(.2f);

        amHit = false;
    }

    public bool IsVectorBetween(Vector3 e, Vector3 a, Vector3 b)
    {
        float minX = Mathf.Min(a.x, b.x);
        float maxX = Mathf.Max(a.x, b.x);

        float minY = Mathf.Min(a.y, b.y);
        float maxY = Mathf.Max(a.y, b.y);

        return (minX <= e.x && e.x <= maxX) && (minY <= e.y && e.y <= maxY);
    }

    public float ConvertBasedOnOffsetDirection(float f)
    {
        return offsetDirection == PlayerEnemyPool.OffsetDirection.Right ? f : -f;
    }

    public Animator GetAnimator()
    {
        return anim;
    }

    public CapsuleCollider GetCollider()
    {
        return eCollider;
    }

    public Rigidbody GetRigidBody()
    {
        return rigbody;
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    public CharacterController GetPlayerController()
    {
        return playerController;
    }

    public EnemyWeapon[] GetEnemyWeapons()
    {
        return enemyWeapons;
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public bool IsActive()
    {
        return active;
    }

    public State GetState()
    {
        return state;
    }

    public void SetState(State state)
    {
        this.state = state;
    }

    public PlayerEnemyPool.OffsetDirection GetOffsetDirection()
    {
        return offsetDirection;
    }

    public void SetOffsetDirection(PlayerEnemyPool.OffsetDirection offsetDirection)
    {
        this.offsetDirection = offsetDirection;
    }

    public float GetLastAttackTime()
    {
        return lastAttack;
    }

    public void ResetLastAttackTime()
    {
        this.lastAttack = 0.0f;
    }

    public Health GetHealthData()
    {
        return health;
    }

    public bool IsAttacking()
    {
        return attacking;
    }

    public void SetAttacking(bool attacking)
    {
        this.attacking = attacking;
    }

    public void DealDamageToPlayer()
    {
        if (IsPlayerInAttackRange())
            player.SendMessage("TakeDamage", Random.Range(damageRange.x, damageRange.y));
    }

    public void PlayFlashEffect()
    {
        meshRenderer.material.color = hitFlashColor;
        isFlashColor = true;
    }

    public void UpdateHealthBarFill()
    {
        healthImage.fillAmount = health.GetHealth() / maxHealth;
    }

    public void DisableEnemyWeapons()
    {
        foreach (EnemyWeapon weapon in GetEnemyWeapons())
        {
            weapon.DisableEnemyWeaponCollider();
        }
    }

    public void EnableEnemyWeapons()
    {
        foreach (EnemyWeapon weapon in GetEnemyWeapons())
        {
            weapon.EnableEnemyWeaponCollider();
        }
    }

}
