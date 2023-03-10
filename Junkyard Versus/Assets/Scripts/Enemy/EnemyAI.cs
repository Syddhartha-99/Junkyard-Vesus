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
    public float maxHealth = 5000;
    public float health;

    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    public Transform projectileMuzzle1;
    public Transform projectileMuzzle2;
    public Transform projectileMuzzle3;


    [Header("State")]
    public float sightRange, attackRange;
    public float phase2AttackRange = 30;
    float phase2HoverHeight = 15;
    public int bulletHellSpread = 36;
    public bool playerInSightRange, playerInAttackRange;

    enum BossPhases {phase1, phase1to2, phase2, phase2to3, phase3, phaseDead};
    BossPhases bossPhase = BossPhases.phase1;

    [SerializeField] private healthBarScript healthBar;

    [SerializeField] private MeshRenderer attackRangeMesh;
    private bool attackRangeMeshFlip = false;
    
    private void Awake()
    {
        enemyCameraTarget = GameObject.Find("EnemyCameraTarget").transform;
        agent = GetComponent<NavMeshAgent>();
        healthBar.UpdateHealthBar(maxHealth, health);
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
                    bossPhase = BossPhases.phase1to2;
                    attackRange = 0;
                    attackRangeMesh.enabled = true;
                }
                else
                {
                    if (!playerInSightRange && !playerInAttackRange) Patroling();
                    if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                    if (playerInAttackRange && playerInSightRange) AttackPlayer();
                }
                
                break;

            case BossPhases.phase1to2: //Transition Phase
                if(health >= maxHealth && agent.baseOffset >=phase2HoverHeight && attackRange >= phase2AttackRange)
                {
                    //Enter Phase 2
                    bossPhase = BossPhases.phase2;
                }
                health = Mathf.Min(maxHealth, health + 1);
                agent.baseOffset = Mathf.Min(phase2HoverHeight, agent.baseOffset + 2 * Time.deltaTime);
                attackRange = Mathf.Min(phase2AttackRange, attackRange + 2.5f * Time.deltaTime);
                // if(attackRangeMeshFlip)
                // {
                //     attackRange = Mathf.Min(phase2AttackRange, attackRange + 2.5f * Time.deltaTime);
                // }else
                // {
                //     attackRange = Mathf.Max(0, attackRange - 2f * Time.deltaTime);
                //     if(attackRange <= 0)
                //     {
                //         attackRangeMeshFlip = true;
                //     }
                // }
                
                healthBar.UpdateHealthBar(maxHealth,health);

                break;
            case BossPhases.phase2:

                if(health <= 0)
                {
                    bossPhase = BossPhases.phase2to3;
                }
                else
                {
                    if (!playerInSightRange && !playerInAttackRange) Patroling();
                    if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                    if (playerInAttackRange && playerInSightRange) BulletHell();
                }
                
                break;
            case BossPhases.phase2to3:

                if(health >= maxHealth)
                {
                    //Enter Phase 2
                    bossPhase = BossPhases.phase3;
                }
                health = Mathf.Min(maxHealth, health + 1);
                healthBar.UpdateHealthBar(maxHealth,health);

                break;
            case BossPhases.phase3:
                if(health <= 0)
                {
                    bossPhase = BossPhases.phaseDead;
                }
                else
                {
                    if (!playerInSightRange && !playerInAttackRange) Patroling();
                    if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                    if (playerInAttackRange && playerInSightRange) BulletMegaHell();
                }
                break;

            case BossPhases.phaseDead:
                if(attackRangeMeshFlip)
                {
                    attackRange = Mathf.Min(phase2AttackRange * 5, attackRange + 40f * Time.deltaTime);
                    if(attackRange>=phase2AttackRange * 5)
                    {
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    attackRange = Mathf.Max(0, attackRange - 15f * Time.deltaTime);
                    if(attackRange <= 0)
                    {
                        attackRangeMeshFlip = true;
                    }
                }
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
            //Attack code here
            Rigidbody rb = Instantiate(projectile, projectileMuzzle1.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            //End of attack code

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
            //Attack code here
            for(int i=0; i<bulletHellSpread; i++)
            {
                Rigidbody rb = Instantiate(projectile, projectileMuzzle1.position, projectileMuzzle1.rotation).GetComponent<Rigidbody>();
                rb.transform.RotateAround(transform.position, new Vector3(0, 1, 0), i*(360/bulletHellSpread));
                rb.AddForce(rb.transform.forward * 32f, ForceMode.Impulse);
            }

            //End of attack code

            // Rigidbody rb = Instantiate(projectile, projectileMuzzle.position, Quaternion.identity).GetComponent<Rigidbody>();
            // rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void BulletMegaHell()
    {
        //Debug.Log("Enemy: AttackPlayer");
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(enemyCameraTarget.position);

        if (!alreadyAttacked)
        {
            ///Attack code here
            for (int i = 0; i < bulletHellSpread; i++)
            {
                Rigidbody rb = Instantiate(projectile, projectileMuzzle1.position, projectileMuzzle1.rotation).GetComponent<Rigidbody>();
                rb.transform.RotateAround(transform.position, new Vector3(0, 1, 0), i * (360 / bulletHellSpread));
                rb.AddForce(rb.transform.forward * 32f, ForceMode.Impulse);

                Rigidbody rb2 = Instantiate(projectile, projectileMuzzle2.position, projectileMuzzle2.rotation).GetComponent<Rigidbody>();
                rb2.transform.RotateAround(transform.position, new Vector3(0, 1, 0), i * (360 / bulletHellSpread));
                rb2.AddForce(rb2.transform.forward * 32f, ForceMode.Impulse);

                Rigidbody rb3 = Instantiate(projectile, projectileMuzzle3.position, projectileMuzzle3.rotation).GetComponent<Rigidbody>();
                rb3.transform.RotateAround(transform.position, new Vector3(0, 1, 0), i * (360 / bulletHellSpread));
                rb3.AddForce(rb3.transform.forward * 32f, ForceMode.Impulse);
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
        healthBar.UpdateHealthBar(maxHealth, health);

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


