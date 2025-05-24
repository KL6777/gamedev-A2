using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Hover Settings")]
    public float hoverHeight = 2f;
    public float hoverSmoothness = 0.1f;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;
    private bool dropeditem = false;
    private bool isDead;

    [Header("Enemy Settings")]
    public float moveSpeed = 3.5f;

    private NavMeshAgent agent;
    

    [Header("Attack Settings and health")]
    private int health = 6;
    private int damage = 1;
    private bool isAttacking;
    private Animator anim;
    private bool canAttack;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.autoTraverseOffMeshLink = false; // Disable default handling for gaps

        anim = GetComponent<Animator>();
        Debug.Log("Animator component retrieved: " + anim);
        isAttacking = false;
        isDead = false;
        canAttack = true;
    }

    void Update()
    {
        if (isDead) return;

        // Continuously chase the player
        ChasePlayer();

        // Handle hovering over gaps
        HoverOverGaps();

        // Check if the enemy is dead
        if (health <= 0)
        {
            isDead = true;
            
        }
    }

    private void ChasePlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private void HoverOverGaps()
    {
        if (agent.isOnOffMeshLink)
        {
            Vector3 hoverTarget = new Vector3(transform.position.x, hoverHeight, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, hoverTarget, hoverSmoothness);
            agent.CompleteOffMeshLink();
        }
    }

    private void Respawn()
    {
        isDead = false;
        transform.position = respawnPoint.position;
        agent.isStopped = false;
    }

    private void Death()
    {   
        isDead = true;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Die");
        if (!dropeditem){
            dropeditem = true;
            // Drop item
        }

    }

    void OnTriggerEnter(Collider other)
    {
        

        if (other.gameObject.tag == "Player" && !isAttacking && canAttack)
        {
            StartCoroutine(HandleAttack());
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && isAttacking)
        {
            StartCoroutine(HandleExit());
        }
    }

    void onCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Powercell"){
            health --;
            canAttack = false;
            agent.isStopped = true;
            WaitForSeconds(3);
            canAttack = true;
            agent.isStopped = false;
        }
    }


    private IEnumerator HandleAttack()
    {
        isAttacking = true;
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);
        Debug.Log("Enemy is attacking: " + anim.GetBool("isAttacking"));

        // Wait for the attack animation to complete
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        anim.SetBool("isAttacking", false);
        anim.SetBool("isWalking", true);
        Debug.Log("Enemy finished attacking: " + anim.GetBool("isAttacking"));

        isAttacking = false;
    }

    private IEnumerator HandleExit()
    {
        // Wait for the attack animation to complete before stopping the attack
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        anim.SetBool("isAttacking", false);
        anim.SetBool("isWalking", true);
        Debug.Log("Enemy stopped attacking: " + anim.GetBool("isAttacking"));

        isAttacking = false;
    }

    private IEnumerator WaitForSeconds(int v)
    {
        yield return new WaitForSeconds(v);
    }

    
}
