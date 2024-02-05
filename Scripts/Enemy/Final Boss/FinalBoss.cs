using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FinalBoss : MonoBehaviour
{

    public enum Attack { IDLE, FLYING_SWORDS, FIRE_PILLARS, MELEE, FIREBALLS, REST }
    
    #region Public Fields

    [Header("General Settings")]
    public float attackDelta = 1.5f;
    public float afterDeathDelta = 4.5f;
    public Transform[] platformTargets;

    [Header("Effect Settings")]
    public float hitFlashDelta = 0.2f;
    public Color hitFlashColor = new Color(1f, .3f, .3f, .85f);

    [Header("Health Settings")]
    public float maxHealth = 250f;
    public Image healthBar;

    [Header("Melee Attack Settings")] 
    public MeleeZone leftZone;
    public MeleeZone rightZone;

    [Space] 
    public float meleeRotationDelta = 0.5f;
    public int meleeAttacksBeforeQuickAttacks = 7;
    public int meleeAttackQuickAttackThreshold = 30;

    [Space]
    [Header("Flying Sword Settings")] 
    public GameObject[] swordPrefab;

    [Space]
    public GameObject[] flyingSwordJumpRollDodgeSpawns = new GameObject[3];
    public GameObject[] flyingSwordDoubleJumpDodgeSpawns = new GameObject[3];
    public GameObject[] flyingSwordRollDodgeSpawns = new GameObject[3];

    [Space]
    public float swordYOffset = 2f;
    public float swordDespawnRangeSqr = 50f;
    public float swordFireSpeed = 3.5f;

    [Space]
    public float swordRearrangeDelta = 1f;
    public float swordFireDelayDelta = 0.5f;

    [Space]
    [Header("Fireball Attack Settings")]
    public GameObject fireballPrefab;

    [Space]
    public float fireballYSpawnOffset = 2f;
    public float fireBallHitDelta = 1.2f;
    public AnimationCurve fireBallCurve;

    [Space]
    [Header("Fire Pillar Attack Settings")]
    public GameObject firePillarPrefab;

    [Space]
    public float firePillarYSpawnOffset = 3.5f;
    public float firePillarSpawnDelta = 1.5f;
    public float firePillarActiveDelta = 0.8f;
    public float firePillarTellDelta = 0.7f;

    [Space] [Header("Rest Settings")]
    public GameObject restEffect;
    public float restDelta = 3f;

    #endregion

    #region Private Fields

    #region Components

    // Components
    private Transform spawnLocation;
    private Animator anim;
    private EnemyWeapon[] weapons;
    private CapsuleCollider eCollider;
    private Health healthData;

    private SkinnedMeshRenderer meshRenderer;
    private Color renderColor;

    // External Components
    private PlayerController player;
    private Camera mainCamera;
    private LevelLoader levelLoader;

    #endregion

    #region Movement Locking

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float colliderResetHeight;
    private float colliderResetRadius;

    // Rotation Lock
    private bool lockedRotation = true;
    private Quaternion desiredRotation;

    // Horizontal Movement Lock
    private bool lockedHorizontalMovement = true;
    private Vector3 desiredHorizontalPosition;

    #endregion

    #region Combat

    #region General Attack Data

    private readonly Attack[] stageOneAttacks = {Attack.FIREBALLS, Attack.FLYING_SWORDS, Attack.FIRE_PILLARS, Attack.REST};
    private readonly Attack[] stageTwoAttacks = {Attack.FLYING_SWORDS, Attack.FIRE_PILLARS, Attack.FIREBALLS, Attack.REST};
    private readonly Attack[] stageThreeAttacks = {Attack.FIREBALLS, Attack.FLYING_SWORDS, Attack.FIRE_PILLARS, Attack.FLYING_SWORDS, Attack.FIREBALLS, Attack.REST};

    private Attack currentAttack = Attack.IDLE;

    private int stage = 0;
    private int pattern = 0;
    private int meleeAttacks = 0;
    
    private bool attackActive = false;
    private float lastAttack = 0.0f;

    #endregion
    
    #region Flying Sword Attack

    enum FlyingSwordSpawn { JUMP_ROLL_DODGE, DOUBLE_JUMP_DODGE, ROLL_DODGE }

    private List<FlyingSwordSpawn[]> flyingSwordSpawns = new List<FlyingSwordSpawn[]>()
    {
        new[] { FlyingSwordSpawn.DOUBLE_JUMP_DODGE },
        new[] { FlyingSwordSpawn.DOUBLE_JUMP_DODGE, FlyingSwordSpawn.ROLL_DODGE },
        new[] { FlyingSwordSpawn.ROLL_DODGE, FlyingSwordSpawn.JUMP_ROLL_DODGE, FlyingSwordSpawn.DOUBLE_JUMP_DODGE }
    };

    private OrbitingSword[] orbitingSwords;

    private int flyingSwordSpawn = 0;
    private bool flyingSwordsFired = false;

    #endregion

    #region Melee Attack

    private enum MeleeDirection { LEFT, RIGHT }
    private bool meleeAttackFinished = false;

    #endregion

    #region Fire Pillar Attack

    private int[] firePillarPlatformOrder = {1, 0, 2};

    private int firePillarCurrentPlatform = 0;
    private float firePillarLastSpawn = 0;

    #endregion

    #endregion

    // Bool States
    private bool healthFilling = false;
    private bool isDead = false;
    private bool isHit = false;
    private bool isFlinching = false;
    private bool canFlinch = false;

    private List<GameObject> bossProjectiles;

    #endregion

    void Awake()
    {
        // Grab Components
        anim = GetComponent<Animator>();
        eCollider = GetComponentInChildren<CapsuleCollider>();
        weapons = GetComponentsInChildren<EnemyWeapon>();
        healthData = new Health(maxHealth);

        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        renderColor = meshRenderer.material.color;

        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        colliderResetHeight = eCollider.height;
        colliderResetRadius = eCollider.radius;

        // Assign Memory
        orbitingSwords = new OrbitingSword[3];
        bossProjectiles = new List<GameObject>();
    }

    void Start()
    {
        // Find External Components
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        levelLoader = GameObject.Find("LoadingNextLevel").GetComponentInChildren<LevelLoader>();

        // Static Spawn Location
        spawnLocation = this.transform.Find("Spawn Location");

        // Init Desired Rotation and Position
        desiredRotation = this.transform.rotation;
        desiredHorizontalPosition = this.transform.position;

        // Enable Health Bar UI Element
        healthBar.transform.parent.gameObject.SetActive(true);
        healthBar.fillAmount = 0f;

        // Melee Attack does 1/4th of player health on hit
        foreach (EnemyWeapon weapon in weapons)
        {
            weapon.SetMinDamage(player.GetHealthData().GetMaxHealth() / 4);
            weapon.SetMaxDamage(player.GetHealthData().GetMaxHealth() / 4);
        }

        // Fill the health bar
        StartCoroutine(FillHealthBar());
    }

    void FixedUpdate()
    {
        // Update Health Bar
        if (!healthFilling)
            healthBar.fillAmount = healthData.HealthPercentage();

        // Prevent stuff actions during death
        if (isDead)
            return;

        // Prevent rotation if locked
        if (lockedRotation)
            this.transform.rotation = desiredRotation;
        
        // Prevent horizontal movement if locked
        if (lockedHorizontalMovement)
            this.transform.position = new Vector3(desiredHorizontalPosition.x, this.transform.position.y, desiredHorizontalPosition.z);

        HandleAttacks();            
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword") && !isHit)
            StartCoroutine(Damage(Random.Range(2, 4)));
    }

    void HandleAttacks()
    {
        UpdateBossStage();
        TickMeleeAttack();

        if (!attackActive && lastAttack >= attackDelta)
        {
            lastAttack = 0.0f;
            RotateCurrentAttack();
        }

        switch (currentAttack)
        {
            case Attack.IDLE:
                lastAttack += Time.deltaTime;
                break;

            case Attack.FLYING_SWORDS:
                TickFlyingSwordAttack();
                break;

            case Attack.FIRE_PILLARS:
                TickFirePillarAttack();
                break;

            case Attack.FIREBALLS:
                TickFireballAttack();
                break;

            case Attack.REST:
                TickRestAttack();
                break;
        }
    }

    void UpdateBossStage()
    {
        if (healthData.HealthPercentage() > 0.6)
        {
            stage = 0;
        }

        else if (healthData.HealthPercentage() > 0.2 && stage == 0)
        {

            stage = 1;
            pattern = 0;
        }

        else if (healthData.HealthPercentage() <= 0.2 && stage == 1)
        {
            stage = 2;
            pattern = 0;
        }
    }

    // Sets the current attack to the next in pattern
    void RotateCurrentAttack()
    {
        if (stage == 0)
        {
            currentAttack = stageOneAttacks[pattern];
            if (++pattern > stageOneAttacks.Length - 1)
                pattern = 0;
        }

        else if (stage == 1)
        {
            currentAttack = stageTwoAttacks[pattern];
            if (++pattern > stageTwoAttacks.Length - 1)
                pattern = 0;
        }

        else
        {
            currentAttack = stageThreeAttacks[pattern];
            if (++pattern > stageThreeAttacks.Length - 1)
                pattern = 0;
        }
    }

    #region Flying Swords Functions

    // Sword Attack Tick
    // Called every frame while flying sword attack is active
    void TickFlyingSwordAttack()
    {
        if (!attackActive)
        {
            attackActive = true;
            anim.SetTrigger("Sword Attack");
            return;
        }

        if (!flyingSwordsFired)
        {
            bool allInPosition = true;
            foreach (OrbitingSword sword in orbitingSwords)
                if (sword == null || !sword.IsInPosition())
                {
                    allInPosition = false;
                    break;
                }

            if (allInPosition)
                StartCoroutine(FireSwords());
        }
    }

    // Spawn in Swords for Sword Attack
    // Used in animator to spawn one sword at a time
    void SpawnSwordAttack(int i)
    {
        orbitingSwords[i] = Instantiate(swordPrefab[i], spawnLocation.position + new Vector3(0, swordYOffset, 0),
            swordPrefab[i].transform.rotation).GetComponent<OrbitingSword>();
        
        bossProjectiles.Add(orbitingSwords[i].gameObject);

        AudioManager.instance.Play("Flying Sword Spawn");
        StartCoroutine(TranslateSwordIntoFireTransform(i));
    }

    IEnumerator FireSwords()
    {
        flyingSwordsFired = true;

        float time = 0f;
        while (swordFireDelayDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        AudioManager.instance.Play("Flying Sword Moving");
        for (int swordIndex = 0; swordIndex < orbitingSwords.Length; ++swordIndex)
            StartCoroutine(FireSwordAtPlayer(swordIndex));

        if (++flyingSwordSpawn > (flyingSwordSpawns[stage].Length - 1))
            flyingSwordSpawn = 0;

        orbitingSwords = new OrbitingSword[3]; // Clears array
        flyingSwordsFired = false; // Resets swords to be fired next time

        FinishedAttack();
    }

    IEnumerator FireSwordAtPlayer(int i)
    {
        OrbitingSword sword = orbitingSwords[i]; // Save instance of sword for clearing of array
        Vector3 originalPosition = sword.transform.position;

        sword.EnableHitCollider(); // Enable collider to deal damage

        // Continue at player till off screen for set delta
        while ((originalPosition - sword.transform.position).sqrMagnitude < swordDespawnRangeSqr)
        {
            sword.transform.Translate(Vector3.forward * swordFireSpeed * Time.deltaTime); // Move Sword 
            yield return null;
        }

        AudioManager.instance.Stop("Flying Sword Moving");
        Destroy(sword.gameObject); // Destory for being off screen too long
    }

    IEnumerator TranslateSwordIntoFireTransform(int i)
    {
        Transform swordStartTransform = orbitingSwords[i].transform;

        float time = 0.0f;
        while (swordRearrangeDelta > time)
        {
            float t = time / swordRearrangeDelta;
            float sinerp = Mathf.Sin(t * Mathf.PI * 0.5f); // Quick to Slow
            float coserp = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // Slow to Quick
            float smootherStep = t * t * t * (t * (6f * t - 15f) + 10f); // Slow to Quick to Slow

            Vector3 startPosition = swordStartTransform.position;
            Vector3 desiredPosition = GetFlyingSwordFirePosition(i);

            float x = Mathf.Lerp(startPosition.x, desiredPosition.x, sinerp);
            float y = Mathf.Lerp(startPosition.y, desiredPosition.y, coserp);
            float z = Mathf.Lerp(startPosition.z, desiredPosition.z, sinerp);

            orbitingSwords[i].transform.position = new Vector3(x, y, z);
            orbitingSwords[i].transform.rotation = Quaternion.Lerp(swordStartTransform.rotation, Quaternion.Euler(0, -90, 180), smootherStep);

            time += Time.deltaTime;
            yield return null;
        }

        orbitingSwords[i].SetZSpinning(true);
        orbitingSwords[i].SetIsInPosition(true);
    }

    Vector3 GetFlyingSwordFirePosition(int swordIndex)
    {
        switch (flyingSwordSpawns[stage][flyingSwordSpawn])
        {
            case FlyingSwordSpawn.DOUBLE_JUMP_DODGE:
                return flyingSwordDoubleJumpDodgeSpawns[swordIndex].transform.position;

            case FlyingSwordSpawn.ROLL_DODGE:
                return flyingSwordRollDodgeSpawns[swordIndex].transform.position;

            case FlyingSwordSpawn.JUMP_ROLL_DODGE:
                return flyingSwordJumpRollDodgeSpawns[swordIndex].transform.position;

            default:
                return flyingSwordJumpRollDodgeSpawns[swordIndex].transform.position;
        }
    }

    #endregion

    #region Melee Attack Functions

    void TickMeleeAttack()
    {
        if (!attackActive)
        {
            if (!attackActive && (rightZone.ContainsPlayer() || leftZone.ContainsPlayer()))
            {
                MeleeDirection direction;
                if (rightZone.ContainsPlayer() && leftZone.ContainsPlayer())
                    direction = (MeleeDirection)Random.Range(0, 1);

                else
                    direction = rightZone.ContainsPlayer() ? MeleeDirection.RIGHT : MeleeDirection.LEFT;

                attackActive = true;
                currentAttack = Attack.MELEE;

                bool regularAttack = true;
                if (++meleeAttacks >= meleeAttacksBeforeQuickAttacks)
                {
                    int rand = Random.Range(0, meleeAttackQuickAttackThreshold);
                    int chance = (int) Mathf.Lerp(0, meleeAttackQuickAttackThreshold, (float) meleeAttacks / meleeAttackQuickAttackThreshold);

                    if (rand >= chance)
                        regularAttack = false;
                }

                StartCoroutine(regularAttack ? PlayMeleeAttack(direction) : PlayQuickAttack(direction));
            }

            else
                FinishedAttack();
        }
    }

    IEnumerator PlayMeleeAttack(MeleeDirection direction)
    {
        StartCoroutine(RotateBoss(direction == MeleeDirection.LEFT ? 270 : 90, meleeRotationDelta));
        while (!lockedRotation)
            yield return null;

        anim.SetTrigger("Melee Attack");
        AudioManager.instance.Play("Boss Sword Swing");
        
        while (!meleeAttackFinished)
            yield return null;

        if (rightZone.ContainsPlayer() || leftZone.ContainsPlayer())
        {

            MeleeDirection newDirection = rightZone.ContainsPlayer() ? MeleeDirection.RIGHT : MeleeDirection.LEFT;
            if (newDirection != direction)
            {
                StartCoroutine(RotateBoss(transform.eulerAngles.y + 180, meleeRotationDelta / 0.66f));
                while (!lockedRotation)
                    yield return null;
            }

            meleeAttackFinished = false;
            anim.SetTrigger("Follow Up Melee");
            AudioManager.instance.Play("Boss Sword Swing");

            while (!meleeAttackFinished)
                yield return null;
        }

        StartCoroutine(RotateBoss(180, meleeRotationDelta));
        while (!lockedRotation)
            yield return null;

        meleeAttackFinished = false;
        FinishedAttack();
    }

    IEnumerator PlayQuickAttack(MeleeDirection direction)
    {
        StartCoroutine(RotateBoss(direction == MeleeDirection.LEFT ? 270 : 90, meleeRotationDelta));
        while (!lockedRotation)
            yield return null;

        anim.SetTrigger("Follow Up Melee");
        AudioManager.instance.Play("Boss Sword Swing");

        while (!meleeAttackFinished)
            yield return null;

        StartCoroutine(RotateBoss(180, meleeRotationDelta));
        while (!lockedRotation)
            yield return null;

        meleeAttackFinished = false;
        FinishedAttack();
    }

    void MeleeAttackFinished()
    {
        meleeAttackFinished = true;
    }

    void EnableSwordCollider()
    {
        foreach (EnemyWeapon weapon in weapons)
            weapon.EnableEnemyWeaponCollider();
    }

    void DisableSwordCollider()
    {
        foreach (EnemyWeapon weapon in weapons)
            weapon.DisableEnemyWeaponCollider();
    }

    #endregion

    #region Fireball Functions

    private void TickFireballAttack()
    {
        if (!attackActive)
        {
            attackActive = true;
            anim.SetTrigger("Fireball Attack");
        }
    }

    private IEnumerator FireFireball(int platform)
    {
        Fireball fireball = Instantiate(fireballPrefab, spawnLocation.position + new Vector3(0, fireballYSpawnOffset, 0), fireballPrefab.transform.rotation).GetComponent<Fireball>();
        bossProjectiles.Add(fireball.gameObject);
        
        AudioManager.instance.Play("Fireball Cast");

        Vector3 startPos = fireball.transform.position;
        Vector3 desiredPos = platformTargets[platform].position;

        float time = 0.0f;
        while (fireBallHitDelta > time)
        {
            if (fireball == null)
                break;

            float t = time / fireBallHitDelta;
            float x = Mathf.Lerp(startPos.x, desiredPos.x, t);
            float y = Mathf.LerpUnclamped(startPos.y, desiredPos.y, fireBallCurve.Evaluate(t));
            float z = Mathf.Lerp(startPos.z, desiredPos.z, Mathf.Sin(t * Mathf.PI * 0.5f));

            fireball.transform.position = new Vector3(x, y, z);
            fireball.transform.LookAt(platformTargets[platform]);

            time += Time.deltaTime;
            yield return null;
        }

        if (fireball != null)
        {
            fireball.gameObject.SetActive(false);
            Destroy(fireball.gameObject);
        }
    }

    #endregion

    #region Fire Pillar Functions

    void TickFirePillarAttack()
    {
        if (!attackActive)
        {
            attackActive = true;
            anim.SetTrigger("Fire Pillar Attack");
        }

        else
        {

            if (firePillarSpawnDelta <= firePillarLastSpawn)
            {
                firePillarLastSpawn = 0;
                StartCoroutine(SpawnFirePillar(firePillarPlatformOrder[firePillarCurrentPlatform]));

                if (++firePillarCurrentPlatform > (platformTargets.Length - 1))
                {
                    firePillarCurrentPlatform = 0;
                    FinishedAttack();
                }
            }

            else
                firePillarLastSpawn += Time.deltaTime;
        }
    }

    IEnumerator SpawnFirePillar(int platform)
    {
        FirePillar firePillar = Instantiate(firePillarPrefab, platformTargets[platform].position + new Vector3(0, firePillarYSpawnOffset, 0),
            firePillarPrefab.transform.rotation).GetComponent<FirePillar>();
        bossProjectiles.Add(firePillar.gameObject);
        
        firePillar.PlayTellEffect();
        
        float time = 0.0f;
        while (firePillarTellDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        firePillar.PlayPillarEffect();

        time = 0.0f;
        while (firePillarActiveDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        firePillar.gameObject.SetActive(false);
        Destroy(firePillar.gameObject);

        if (platform == firePillarPlatformOrder[firePillarPlatformOrder.Length - 1])
            AudioManager.instance.Stop("Fire Pillar Ignition");
    }

    #endregion

    #region Rest Functions

    void TickRestAttack()
    {
        if (!attackActive)
        {
            AudioManager.instance.Play("Boss Rest");
            restEffect.SetActive(false);

            anim.SetTrigger("Rest Start");
            StartCoroutine(Rest());
        }
    }

    IEnumerator Rest()
    {
        attackActive = canFlinch = true;
        isFlinching = false;
        
        anim.SetBool("Can Flinch", true);

        float time = 0.0f;
        while (restDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        canFlinch = false;
        anim.SetBool("Can Flinch", false);
        restEffect.SetActive(true);
        anim.SetTrigger("Rest Finish");
        AudioManager.instance.Play("Boss Roar");
    }

    #endregion

    #region Damage/Death Functions

    IEnumerator Damage(float damage)
    {
        isHit = true;
        OnHit(damage);

        float time = 0.0f;
        while (0.2 > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        isHit = false;
    }

    // When Player Sword Hits Enemy 
    void OnHit(float damage)
    {
        healthData.Damage(damage);

        if (healthData.GetHealth() <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("Dead");
            anim.SetBool("Death Lock", true);
            StartCoroutine(AfterDeath());

            // TODO Disable player movement

            return;
        }

        if (canFlinch && !isFlinching)
        {
            isFlinching = true;
            anim.SetTrigger("Flinch");
        }
            

        StartCoroutine(PlayFlashEffect());
    }

    IEnumerator AfterDeath()
    {
        KillCount.AddToKillCount(1);

        float time = 0.0f;
        while (afterDeathDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        levelLoader.LoadLevel("GameWon");
    }

    public void FinishedFlinchAnimation()
    {
        isFlinching = false;
    }

    #endregion

    #region General Utility Functions

    public bool IsTransformVisible(Transform objTransform)
    {
        Vector3 viewPos = mainCamera.WorldToViewportPoint(objTransform.position);
        return viewPos.x >= 0 && viewPos.x <= 1;
    }

    IEnumerator RotateBoss(float yAngle, float duration)
    {
        lockedRotation = false;

        float rotateTime = 0.0f;
        float initialYRotation = transform.eulerAngles.y;

        while (duration > rotateTime)
        {
            float yRotation = Mathf.Lerp(initialYRotation, yAngle, (rotateTime / duration) * Mathf.PI * 0.5f);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);

            rotateTime += Time.deltaTime;
            yield return null;
        }

        desiredRotation = transform.rotation;
        lockedRotation = true;
    }

    void FinishedAttack()
    {
        attackActive = false;
        currentAttack = Attack.IDLE;
    }

    public void Reset()
    {
        anim.enabled = false;

        foreach (GameObject go in bossProjectiles)
            if (go != null)
            {
                go.SetActive(false);
                Destroy(go);
            }

        eCollider.height = colliderResetHeight;
        eCollider.radius = colliderResetRadius;

        lockedHorizontalMovement = lockedRotation = false;

        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        desiredHorizontalPosition = startPosition;
        desiredRotation = startRotation;

        lockedHorizontalMovement = lockedRotation = true;

        healthData.SetHealth(maxHealth);
        healthBar.transform.parent.gameObject.SetActive(false);

        stage = 0;
        currentAttack = 0;
        meleeAttacks = 0;
        lastAttack = 0.0f;
        flyingSwordSpawn = 0;
        flyingSwordsFired = false;
        isDead = false;

        FinishedAttack();

        anim.enabled = true;
    }

    public void EnableHealthBar()
    {
        // Enable Health Bar UI Element
        healthBar.transform.parent.gameObject.SetActive(true);
        healthBar.fillAmount = 0f;

        // Fill the health bar
        StartCoroutine(FillHealthBar());
    }

    #endregion

    #region Effect Functions

    IEnumerator FillHealthBar()
    {
        healthFilling = true;

        float fillAmount = 0.0f;
        while (fillAmount < 1f)
        {
            fillAmount += Time.deltaTime * 0.5f;
            healthBar.fillAmount = fillAmount;

            yield return null;
        }

        healthFilling = false;
    }

    IEnumerator PlayFlashEffect()
    {
        meshRenderer.material.color = hitFlashColor;

        float time = 0.0f;
        while (hitFlashDelta > time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        meshRenderer.material.color = renderColor;
    }

    #endregion

}
