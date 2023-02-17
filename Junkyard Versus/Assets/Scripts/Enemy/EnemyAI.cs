using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform enemyCameraTarget;

    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Stats")]
    public float health;

    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    public Transform projectileMuzzle;

    [Header("State")]
    public float sightRange, attackRange;
    public float phase2AttackRange = 50;
    public bool playerInSightRange, playerInAttackRange;

    enum BossPhases {phase1, phase2, phase3};
    BossPhases bossPhase = BossPhases.phase1;

    public int bulletHellSpread = 36;

    private void Awake()
    {
        enemyCameraTarget = GameObject.Find("EnemyCameraTarget").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        switch(bossPhase)
        {
            case BossPhases.phase1:
                if (health <= 0)
                {
                    //exiting phase1
                    if(agent.baseOffset<=15) //doing transition phase
                    {
                        agent.baseOffset = agent.baseOffset + 2 * Time.deltaTime;
                        attackRange = Mathf.Min(phase2AttackRange, attackRange + 2.5f * Time.deltaTime);
                    }
                    else
                    {
                        bossPhase = BossPhases.phase2;
                        health = 2500;
                    }
                }
                else
                {
                    if (!playerInSightRange && !playerInAttackRange) Patroling();
                    if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                    if (playerInAttackRange && playerInSightRange) AttackPlayer();
                }
                
                break;
            case BossPhases.phase2:
                if (!playerInSightRange && !playerInAttackRange) Patroling();
                if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                if (playerInAttackRange && playerInSightRange) BulletHell();
                break;
            case BossPhases.phase3:
                break;
            default:
                Debug.Log("Error: Unexpected boss phase");
                break;
        }
        
    }

    private void Patroling()
    {
        //Debug.Log("Enemy: Patroling");
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        //Debug.Log("Enemy: ChasePlayer,  enemyCameraTarget.position: "+enemyCameraTarget.position);
        agent.SetDestination(enemyCameraTarget.position);
    }

    private void AttackPlayer()
    {
        //Debug.Log("Enemy: AttackPlayer");
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(enemyCameraTarget.position);

        if (!alreadyAttacked)
        {
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, projectileMuzzle.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
        private void BulletHell()
    {
        //Debug.Log("Enemy: AttackPlayer");
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(enemyCameraTarget.position);

        if (!alreadyAttacked)
        {
            ///Attack code here
            for(int i=0; i<bulletHellSpread; i++)
            {
                Rigidbody rb = Instantiate(projectile, projectileMuzzle.position, projectileMuzzle.rotation).GetComponent<Rigidbody>();
                rb.transform.RotateAround(transform.position, new Vector3(0, 1, 0), i*(360/bulletHellSpread));
                rb.AddForce(rb.transform.forward * 32f, ForceMode.Impulse);
            }
            for(int i=0; i<bulletHellSpread; i++)
            {
                Rigidbody rb = Instantiate(projectile, projectileMuzzle.position, projectileMuzzle.rotation).GetComponent<Rigidbody>();
                rb.transform.RotateAround(transform.position, new Vector3(1, 0, 0), i*(360/bulletHellSpread));
                rb.AddForce(rb.transform.forward * 32f, ForceMode.Impulse);
            }
            ///End of attack code

            // Rigidbody rb = Instantiate(projectile, projectileMuzzle.position, Quaternion.identity).GetComponent<Rigidbody>();
            // rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        //Don't destroy but change phase
        //if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

}


