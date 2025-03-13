using UnityEngine;
using UnityEngine.AI;

public class EnemyCollisionHandler : MonoBehaviour
{
    private bool isCollidingWithPlayer = false; // Bandera para evitar múltiples colisiones
    private NavMeshAgent navMeshAgent; // Referencia al componente NavMeshAgent
    private attributesManager enemyAtm; // Referencia al attributesManager del enemigo

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAtm = GetComponent<attributesManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCollidingWithPlayer)
        {
            Debug.Log("Enemigo colisionó con Player.");

            // Detén el movimiento del enemigo actual
            navMeshAgent.isStopped = true;

            // Establece la bandera para evitar más colisiones
            isCollidingWithPlayer = true;

            // Inflige daño al jugador
            var playerAtm = collision.gameObject.GetComponent<attributesManager>();
            if (playerAtm != null && enemyAtm != null)
            {
                enemyAtm.DealDamage(playerAtm.gameObject);
            }
            else
            {
                Debug.LogWarning("attributesManager del jugador o del enemigo es nulo.");
            }

            // Espera un tiempo antes de reactivar el movimiento (ajusta según tus necesidades)
            StartCoroutine(ReactivateMovement());
        }
    }

    private System.Collections.IEnumerator ReactivateMovement()
    {
        yield return new WaitForSeconds(1.0f); // Espera 1 segundo (ajusta según tus necesidades)
        navMeshAgent.isStopped = false;
        isCollidingWithPlayer = false;
    }
}
