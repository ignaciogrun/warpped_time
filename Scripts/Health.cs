public class Health
{

    private float maxHealth = 20.0f;
    private float health;

    public Health(float maxHealth)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float health)
    {
        this.health = health;
    }

    public void Damage(float damage)
    {
        SetHealth(GetHealth() - damage);
    }

    public void Heal(float heal)
    {
        SetHealth(GetHealth() + heal);
    }

    public float HealthPercentage()
    {
        return health / maxHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
