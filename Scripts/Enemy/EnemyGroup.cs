using UnityEngine;

[DefaultExecutionOrder(-1)]
public class EnemyGroup : MonoBehaviour
{

    [HideInInspector] public bool active = false;
    [HideInInspector] public Enemy[] enemies;
    [HideInInspector] public bool canReturnEnemies = true;
    
    private bool containsPlayer = false;

    void Awake()
    {
        enemies = GetComponentsInChildren<Enemy>();
    }

    void FixedUpdate()
    {
        if (containsPlayer)
            canReturnEnemies = containsPlayer = false;

        else
            canReturnEnemies = true;
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Player")
            ActivateEnemyGroup();
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.name == "Player")
            containsPlayer = true;
    }

    private void ActivateEnemyGroup()
    {
        active = true;

        foreach (Enemy enemy in enemies)
            if (!enemy.IsActive())
                enemy.Activate();
    }
}
