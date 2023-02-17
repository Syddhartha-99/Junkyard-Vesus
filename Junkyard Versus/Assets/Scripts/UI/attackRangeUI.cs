using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackRangeUI : MonoBehaviour
{
    // Start is called before the first frame update
    EnemyAI enemyAI;
    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = (new Vector3(enemyAI.attackRange*2.2f, enemyAI.attackRange*2.2f, enemyAI.attackRange*2.2f));
    }
}
