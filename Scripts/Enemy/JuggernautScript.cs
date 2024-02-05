using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuggernautScript : Enemy
{
    //ZeekAPI zeekAPI = new ZeekAPI();

    //Audio
    //public AudioClip hurt;


    //Particle Effects
    [Header("Effects")]
    public GameObject blood;

    //public GameObject healthBalls; //Commented out till fully implemented


    private float animSpeed;

    new void FixedUpdate()
    {
        base.FixedUpdate();

        GetAnimator().SetFloat("Speed", Mathf.Abs(GetRigidBody().velocity.x));
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
        if (GetHealthData().GetHealth() <= 0)
        {
            OnDeath();
        }

        if (IsAttacking())
        {
            return;
        }
            

        if (!IsPlayerInAttackRange())
        {
            GetClose();
            return;
        }

        SetAttacking(true);
        GetAnimator().SetInteger("AttackChoice", Random.Range(1,7));
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
        {
            OnDeath();
            return;
        }

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
            float xKnockback = GetPlayer().transform.position.x >= transform.position.x
                ? -knockbackVelocity.x
                : knockbackVelocity.x;

            GetRigidBody().velocity = new Vector3(xKnockback * 2.5f, knockbackVelocity.y / 1.5f);
        }
    }

    public override void OnDeath()
    {
        GetAnimator().SetBool("Dead", true);
        SetState(State.Dead);
        GetAnimator().SetTrigger("Died");
    }

    public override void AfterDeath()
    {
        KillCountManager.instance.IncrementKillCount();

        enabled = false;
        Destroy(this.gameObject);
    }

    public void KnockBackPlayer()
    {
        GetPlayer().SendMessage("TakeKnockBack");
    }

    public bool IsJuggGrounded()
    {
        float distToGround = GetCollider().bounds.extents.y;

        return Physics.Raycast(transform.position, -Vector3.up, distToGround + .3f); ;
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

        if (IsJuggGrounded())
        {
            Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;
            desiredVelocity.y = 0;

            animSpeed = desiredVelocity.x;
            GetAnimator().SetFloat("Speed", Mathf.Abs(animSpeed));
            GetRigidBody().velocity = desiredVelocity;
        }
    }

    void PlayHurtSound()
    {
        //GetAudioSource().PlayOneShot(hurt);
        AudioManager.instance.Play("JuggHurt");
    }
}
