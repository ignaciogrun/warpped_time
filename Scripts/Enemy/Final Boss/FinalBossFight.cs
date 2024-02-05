using UnityEngine;

public class FinalBossFight : MonoBehaviour
{

    public GameObject boss;

    private FinalBoss bossScript;
    private bool playerDidIt = false;

    void Start()
    {
        bossScript = boss.GetComponent<FinalBoss>();
    }

    void Update()
    {
        if (playerDidIt && !boss.activeSelf)
        {
            boss.SetActive(true);
            bossScript.Reset();
            bossScript.EnableHealthBar();
        }

        playerDidIt = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerDidIt = true;
        }
            
    }

}
