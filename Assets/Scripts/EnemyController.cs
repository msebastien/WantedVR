using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float lookRadius = 10f;
    Transform target;
    NavMeshAgent agent;

    public GameObject player;
    public Animator anim; 
    public GameObject weapon; // Revolver

    bool isAttacking = false;
    bool isRunning = false;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        target = player.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if(distance <= lookRadius)
        {
            // Defines a threshold distance for walking and running
            if(!isAttacking && distance >= 0.25 * lookRadius)
            {
                anim.SetTrigger("Running");
                agent.speed = 2;
                isRunning = true;
                
            } 
            else if(!isAttacking && distance < 0.25 * lookRadius)
            {
                anim.SetTrigger("Walking");
                agent.speed = 1;
                isRunning = false;
            }

            agent.SetDestination(target.position);

            if(distance <= agent.stoppingDistance + 1)
            {
                // Attack the target (player)
                anim.SetTrigger("Attack");
                AttackTarget();
                isAttacking = true;
            } 
            else
            {
                anim.SetTrigger("Walking");
                isAttacking = false;
            }
        }

    }

    void AttackTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Shoot + attack animation
        weapon.GetComponent<RaycastRevolver>().Shoot();
    }

    // For Unity Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
