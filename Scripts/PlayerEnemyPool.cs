using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerEnemyPool : MonoBehaviour
{

    // Public Members

    [Range(0.1f, 5.0f)]
    public float attackDelta = 2.5f;

    [Range(2, 10)]
    public int enemyPoolSize = 2;

    // Private Members

    private EnemyGroup[] enemyGroups;
    private Enemy[] enemyPool;

    private float timeSinceAttack = 0.0f;
    private Enemy attacker = null;

    private List<Enemy> validEnemies;

    public enum OffsetDirection { Right, Left }

    void Awake()
    {
        enemyGroups = FindObjectsOfType<EnemyGroup>();

        enemyPool = new Enemy[enemyPoolSize];
        validEnemies = new List<Enemy>();
    }

    void Update()
    {        
        CleanPool();
        FillPool();
        AssignAttacker();
    }

    void CleanPool()
    {
        for (int i = 0; i < enemyPool.Length; ++i)
        {
            if (enemyPool[i] == null)
                continue;

            if (!enemyPool[i].IsActive() || enemyPool[i].GetState() == Enemy.State.Dead)
            {
                if (attacker == enemyPool[i])
                    attacker = null;

                enemyPool[i] = null;
                continue;
            }

            if (!enemyPool[i].IsPlayerInAttackRange())
            {
                Enemy closeEnemy = null;
                foreach (EnemyGroup group in enemyGroups)
                    if (group.active && closeEnemy == null)
                        foreach (Enemy enemy in group.enemies)
                            if (enemy.IsActive() && enemy.GetState() == Enemy.State.Keep_Distance &&
                                enemy.IsPlayerInAttackRange())
                            {
                                closeEnemy = enemy;
                                break;
                            }

                if (closeEnemy == null)
                    continue;
                
                RemoveFromPool(enemyPool[i]);
                AddToPool(closeEnemy);
            }
        }
    }

    void FillPool()
    {
        if (!IsPoolOpen())
            return;

        validEnemies.Clear();
        foreach (EnemyGroup group in enemyGroups)
            if (group.active)
                foreach (Enemy enemy in group.enemies)
                    if (enemy.IsActive() && enemy.GetState() == Enemy.State.Keep_Distance && enemy.IsPlayerInAttackRange())
                        validEnemies.Add(enemy);

        FillPoolWithValidList();

        if (!IsPoolOpen())
            return;

        validEnemies.Clear();
        foreach (EnemyGroup group in enemyGroups)
            if (group.active)
                foreach (Enemy enemy in group.enemies)
                    if (enemy.IsActive() && enemy.GetState() == Enemy.State.Keep_Distance)
                        validEnemies.Add(enemy);

        FillPoolWithValidList();
    }

    void FillPoolWithValidList()
    {
        while (validEnemies.Count > 0)
        {
            Enemy enemy = validEnemies[Random.Range(0, validEnemies.Count - 1)];

            AddToPool(enemy);
            validEnemies.Remove(enemy);

            if (!IsPoolOpen())
                return;
        }
    }

    void AssignAttacker()
    {
        if ((attacker == null || attacker.GetState() == Enemy.State.Get_Close) && timeSinceAttack >= attackDelta)
        {
            int leastRecentAttacker = -1;
            for (int i = 0; i < enemyPool.Length; ++i)
            {
                if (enemyPool[i] == null || !enemyPool[i].IsPlayerInAttackRange())
                    continue;

                if (leastRecentAttacker == -1 || enemyPool[i].GetLastAttackTime() > enemyPool[leastRecentAttacker].GetLastAttackTime())
                    leastRecentAttacker = i;
            }

            if (leastRecentAttacker > -1)
            {
                enemyPool[leastRecentAttacker].SetState(Enemy.State.Attack);
                attacker = enemyPool[leastRecentAttacker];
            }
        }

        else
            timeSinceAttack += Time.deltaTime;
    }

    void ResetAttackTime()
    {
        timeSinceAttack = 0.0f;
    }

    bool IsPoolOpen()
    {
        foreach (Enemy enemy in enemyPool)
            if (enemy == null)
                return true;

        return false;
    }

    void AddToPool(Enemy enemy)
    {
        for (int i = 0; i < enemyPool.Length; ++i)
            if (enemyPool[i] == null)
            {
                enemyPool[i] = enemy;
                enemy.SetState(Enemy.State.Get_Close);
                enemy.SetOffsetDirection((OffsetDirection) (i % 2));
                return;
            }
    }

    void RemoveFromPool(Enemy enemy)
    {
        for (int i = 0; i < enemyPool.Length; ++i)
            if (enemyPool[i] == enemy)
            {
                if (enemyPool[i] == attacker)
                    attacker = null;

                enemy.SetState(Enemy.State.Keep_Distance);
                enemyPool[i] = null;
                return;
            }
    }

    public void ResetAllEnemies()
    {
        foreach (EnemyGroup group in enemyGroups)
            foreach (Enemy enemy in group.enemies)
                if (enemy != null && enemy.IsActive())
                    enemy.Deactivate();
    }
}
