using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    public float surroundRadius = 2.0f; 
    public float attackRange = 1.0f; 
    public float surroundThreshold = 0.5f; 
    private NavMeshAgent agent;
    private Transform player; 
    private static int enemyCount = 0; 
    private int enemyIndex;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform; 

        
        enemyIndex = enemyCount;
        enemyCount++;
    }

    private void Update()
    {
        if (player != null)
        {
            Vector3 surroundPosition = CalculateSurroundPosition(player.position, enemyIndex, enemyCount, surroundRadius);
            float distanceToSurroundPosition = Vector3.Distance(transform.position, surroundPosition);
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > attackRange && distanceToSurroundPosition > surroundThreshold)
            {
                
                agent.destination = surroundPosition;
            }
            else
            {
                
                agent.destination = player.position;
            }
        }
    }

   
    private Vector3 CalculateSurroundPosition(Vector3 playerPosition, int index, int totalEnemies, float radius)
    {
        float angle = index * Mathf.PI * 2 / totalEnemies;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        return playerPosition + offset;
    }
}
