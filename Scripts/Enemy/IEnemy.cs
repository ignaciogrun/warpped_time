public interface IEnemy
{
    void KeepDistance();

    void GetClose();

    void Attack();

    void AfterAttack();

    void OnHit(float damage);

    void OnDeath();

    void AfterDeath();
}
