using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePillar : MonoBehaviour
{

    public GameObject tellEffect;
    public GameObject pillarEffect;

    private EnemyWeapon enemyWeapon;

    void Awake()
    {
        enemyWeapon = this.GetComponent<EnemyWeapon>();

        tellEffect.SetActive(false);
        pillarEffect.SetActive(false);
    }

    public void PlayTellEffect()
    {
        AudioManager.instance.Play("Fire Pillar Light Sound");
        tellEffect.SetActive(true);
    }

    public void PlayPillarEffect()
    {
        AudioManager.instance.Play("Fire Pillar Ignition");
        pillarEffect.SetActive(true);

        enemyWeapon.EnableEnemyWeaponCollider();
    }
}
