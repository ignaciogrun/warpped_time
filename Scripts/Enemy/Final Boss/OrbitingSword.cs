using UnityEngine;

public class OrbitingSword : MonoBehaviour
{

    // Orbit Spread
    [Header("Orbit Spread")]
    public float xSpread = 3f;
    public float zSpread = 3f;
    public float yOffset = 0f;

    // Orbit/Spin Speed
    [Header("Speed")]
    public float orbitSpeed = 2f;
    public float spinSpeed = 180f;
    public float zSpinMultiplier = 5f;

    // Misc public Settings
    [Header("Misc")]
    public GameObject trailEffect;

    [Space]
    public Vector3 orbitCenterPoint;
    public bool orbitClockwise = true;

    // Active Bools
    private bool inPosition = false;
    private bool orbitActive = false;
    private bool spinYActive = false;
    private bool spinZActive = false;
    
    // Timing
    private float orbitTimer = 0f;
    private float outOfBoundsTimer = 0f;

    private EnemyWeapon weapon;

    void Start()
    {
        weapon = this.GetComponent<EnemyWeapon>();
        trailEffect.SetActive(false);
    }

    void Update()
    {
        if (spinYActive)
            SpinY();
        
        if (spinZActive)
            SpinZ();

        if (orbitActive)
        {
            orbitTimer += Time.deltaTime * orbitSpeed;
            Rotate();
        }
    }

    private void Rotate()
    {
        float x = Mathf.Cos(orbitTimer) * xSpread * (orbitClockwise ? -1 : 1);
        float z = Mathf.Sin(orbitTimer) * zSpread;
        
        Vector3 offset = new Vector3(x, yOffset, z);
        transform.position = offset + orbitCenterPoint;
    }

    private void SpinY()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + (spinSpeed * Time.deltaTime), transform.eulerAngles.z);
    }

    private void SpinZ()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - (spinSpeed * Time.deltaTime * zSpinMultiplier));
    }

    public bool IsOrbiting()
    {
        return orbitActive;
    }

    public void SetOrbiting(bool orbiting)
    {
        orbitActive = orbiting;
    }

    public bool IsYSpinning()
    {
        return spinYActive;
    }

    public void SetYSpinning(bool spinning)
    {
        spinYActive = spinning;
    }
    
    public bool IsZSpinning()
    {
        return spinZActive;
    }

    public void SetZSpinning(bool spinning)
    {
        spinZActive = spinning;
    }

    public void SetOrbitTimer(float timing)
    {
        this.orbitTimer = timing;
    }

    public float GetOutOfBoundsTimer()
    {
        return outOfBoundsTimer;
    }

    public void SetOutOfBoundsTimer(float timing)
    {
        this.outOfBoundsTimer = timing;
    }

    public void EnableHitCollider()
    {
        weapon.EnableEnemyWeaponCollider();
    }

    public void setDamageRange(float min, float max)
    {
        weapon.SetMinDamage(min);
        weapon.SetMaxDamage(max);
    }

    public bool IsInPosition()
    {
        return inPosition;
    }

    public void SetIsInPosition(bool isInPosition)
    {
        inPosition = isInPosition;
        trailEffect.SetActive(inPosition);
    }

}
