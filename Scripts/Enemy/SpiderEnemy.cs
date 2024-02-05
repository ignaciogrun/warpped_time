using UnityEngine;
using Random = UnityEngine.Random;

public class SpiderEnemy : Enemy
{
    // Public Members
    [Header("Jump Settings")]
    public float jumpDelta = 4.5f;
    public float jumpAngle = 60f;
    public float requiredDistance = 6.0f;

    [Header("Effects")]
    public GameObject blood;

    // Private Members
    private float lastJump = 0.0f;

    new void FixedUpdate()
    {
        base.FixedUpdate();

        if (IsGrounded())
            lastJump += Time.deltaTime;
        
        GetAnimator().SetFloat("Speed", IsGrounded() ? (Mathf.Abs(GetRigidBody().velocity.x) + 0.0001f) / speed : 0);
    }

    public override void KeepDistance()
    {
        MoveToDesiredRange(minKeepDistance, maxKeepDistance);
    }

    public override void GetClose()
    {
        MoveToDesiredRange(minAttackRange, maxAttackRange);
    }

    public override void Attack()
    {
        if (IsAttacking())
            return;

        if (!IsPlayerInAttackRange())
        {
            GetClose();
            return;
        }

        SetAttacking(true);
        GetAnimator().SetInteger("AttackChoice", Random.Range(1, 4));
        GetAnimator().SetTrigger("Attack");
    }

    public override void AfterAttack()
    {
        GetPlayer().SendMessage("ResetAttackTime");
        SetState(State.Get_Close);
        SetAttacking(false);
        ResetLastAttackTime();
    }

    public override void OnHit(float damage)
    {
        if (GetState() == State.Dead)
            return;

        //Audio
        AudioManager.instance.Play("SpiderHurt");

        //Blood Effect
        Vector3 particleSpawn = transform.position + new Vector3(0, 1.2f, -0.2f);
        Instantiate(blood, particleSpawn, transform.rotation);


        //Hurt Effect
        PlayFlashEffect();
        GetHealthData().Damage(damage);
        UpdateHealthBarFill();
        
        if (GetHealthData().GetHealth() <= 0)
            OnDeath();

        else
        {
            float xKnockback = GetPlayer().transform.position.x >= transform.position.x
                ? -knockbackVelocity.x
                : knockbackVelocity.x;

            GetRigidBody().velocity = new Vector3(xKnockback, knockbackVelocity.y);
            ResetLastAttackTime();
        }
    }

    public override void OnDeath()
    {
        SetState(State.Dead);
        GetAnimator().SetTrigger("Died");
    }

    public override void AfterDeath()
    {
        KillCountManager.instance.IncrementKillCount();

        GetAnimator().SetBool("Dead", true);
        this.enabled = false;
        Destroy(this.gameObject);
    }

    private void MoveToDesiredRange(Vector3 min, Vector3 max)
    {
        Vector3 playerPos = GetPlayer().transform.position;
        float xDelta = playerPos.x - transform.position.x;
        
        if (xDelta > 0 && IsVectorBetween(transform.position, playerPos - min, playerPos - max)
            || xDelta <= 0 && IsVectorBetween(transform.position, playerPos + min, playerPos + max))
            return;

        float xOffset = ConvertBasedOnOffsetDirection(Random.Range(min.x, max.x));
        float yOffset = ConvertBasedOnOffsetDirection(Random.Range(min.y, max.y));

        Vector3 targetPos = playerPos + new Vector3(xOffset, yOffset);

        if (IsGrounded())
        {
            if (lastJump >= jumpDelta && Mathf.Abs(targetPos.x - transform.position.x) >= requiredDistance && HasRoomToJump())
            {
                Jump(targetPos);
                return;
            }

            Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;
            desiredVelocity.y = GetRigidBody().velocity.y;

            GetRigidBody().velocity = desiredVelocity;
        }
    }

    private bool HasRoomToJump()
    {
        Vector3 top = transform.position + new Vector3(0, (GetCollider().height * 0.5f));
        return !Physics.Raycast(top, Vector3.up, 5f);
    }

    private void Jump(Vector3 target)
    {
        float iVelocity = Mathf.Sqrt(Mathf.Abs(-2 * Physics.gravity.y * (target.x - transform.position.x)));
        float velocityX = iVelocity * Mathf.Cos(jumpAngle * Mathf.Deg2Rad);
        float velocityY = Mathf.Abs(iVelocity * Mathf.Sin(jumpAngle * Mathf.Deg2Rad));

        if (target.x - transform.position.x < 0)
            velocityX = -velocityX;

        lastJump = 0.0f;
        GetRigidBody().velocity = new Vector3(velocityX, velocityY);
    }
}
