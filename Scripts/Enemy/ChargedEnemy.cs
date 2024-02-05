using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedEnemy : Enemy
{
    
    //ZeekAPI zeekAPI = new ZeekAPI();

    //Audio
    public AudioClip hurt;

    [Header("Effects")]
    //ArcaneBolt
    public GameObject arcaneBolt;

    //Particle Effects
    public GameObject blood;
    //public GameObject healthBalls;

    //AnimationSpeed
    private float animSpeed;

    new void FixedUpdate()
    {
        //Movement Speed for animation
        GetAnimator().SetFloat("Speed", Mathf.Abs(GetRigidBody().velocity.x));

        base.FixedUpdate();
    }

    public override void KeepDistance()
    {
        MoveToDesiredRange(minKeepDistance, maxKeepDistance);
        transform.LookAt(new Vector3(GetPlayer().transform.position.x, transform.position.y, GetPlayer().transform.position.z));
    }

    public override void GetClose()
    {
        MoveToDesiredRange(minAttackRange, maxAttackRange);
        transform.LookAt(new Vector3(GetPlayer().transform.position.x, transform.position.y, GetPlayer().transform.position.z));
    }

    public override void Attack()
    {
        if (GetHealthData().GetHealth() <= 0)
        {
            OnDeath();
        }

        if (IsAttacking())
            return;


        if (!IsPlayerInAttackRange())
        {
            GetClose();
            return;
        }

        SetAttacking(true);
        GetAnimator().SetTrigger("Attack");

        Vector3 boltSpawnPoint = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
        Instantiate(arcaneBolt, boltSpawnPoint, transform.rotation);
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
        {
            OnDeath();
            return;
        }

        AfterAttack();

        //Blood Effect
        Vector3 particleSpawn = transform.position + new Vector3(0, 1.2f, -0.2f);
        Instantiate(blood, particleSpawn, transform.rotation);

        //Hurt Effects
        GetAnimator().SetTrigger("GotHit");
        PlayHurtSound();
        PlayFlashEffect();

        //Update Health Bar
        GetHealthData().Damage(damage);
        UpdateHealthBarFill();

        //Check For Death
        if (GetHealthData().GetHealth() <= 0)
            OnDeath();

        else
        {
            int decideKnockback = Random.Range(1, 3);

            if (decideKnockback == 1)
            {
                float xKnockback = GetPlayer().transform.position.x >= transform.position.x
                ? -knockbackVelocity.x
                : knockbackVelocity.x;

                GetRigidBody().velocity = new Vector3(xKnockback * 2, knockbackVelocity.y / 1.5f);
            }
        }
    }

    public override void OnDeath()
    {
        KillCount.AddToKillCount(1);

        GetAnimator().SetBool("Dead", true);
        SetState(State.Dead);
    }

    public override void AfterDeath()
    {
        KillCountManager.instance.IncrementKillCount();

        this.enabled = false;
        Destroy(this.gameObject);
    }

    private void MoveToDesiredRange(Vector3 min, Vector3 max)
    {
        Vector3 playerPos = GetPlayer().transform.position;
        float xDelta = playerPos.x - transform.position.x;

        if (xDelta > 0 && IsVectorBetween(transform.position, playerPos, playerPos - max)
            || xDelta <= 0 && IsVectorBetween(transform.position, playerPos, playerPos + max))
            return;

        float xOffset = ConvertBasedOnOffsetDirection(Random.Range(min.x, max.x));
        float yOffset = ConvertBasedOnOffsetDirection(Random.Range(min.y, max.y));

        Vector3 targetPos = playerPos + new Vector3(xOffset, yOffset);
        
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;
        desiredVelocity.y = 0;

        animSpeed = desiredVelocity.x;
        GetAnimator().SetFloat("Speed", Mathf.Abs(animSpeed));
        GetRigidBody().velocity = desiredVelocity;
    }

    void PlayHurtSound()
    {
        AudioManager.instance.Play("SkeletonHurt");
    }
}
