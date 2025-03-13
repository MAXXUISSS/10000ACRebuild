using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BestiaController : MonoBehaviour
{
    [Header("Vision")]
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public float radius;
    [Range(0, 360)]
    public float angle;

    [Header("Movimiento")]
    public float moveSpeed;
    public float zigZagRangeMin = -5f;
    public float zigZagRangeMax = 5f;
    [Tooltip("Distancia m�nima para detenerse cerca del jugador")]
    public float stoppingDistance = 1.5f;

    public GameObject player;

    [Header("Ataque")]
    public float chargeSpeed = 10f; // Velocidad del ataque de carga
    public float waitTimeBeforeCharge = 2f; // Tiempo de espera antes de cargar

    [Header("Collision")]
    public static int collisionCount = 0; // Variable est�tica para el recuento de colisiones
    public int collisionThreshold = 3; // N�mero de colisiones antes de empujar al jugador
    public float pushBackForce = 5f;   // Fuerza de empuje hacia atr�s

    [Header("Deteccion de Player")]
    public bool canSeePlayer;
    private NavMeshAgent navMeshAgent;
    private attributesManager enemyAtm;
    private bool isCollidingWithPlayer = false;

    private bool isCharging = false;
    private bool isWaitingToCharge = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found. Make sure the player has the 'Player' tag.");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component not found on this game object.");
        }
        else
        {
            navMeshAgent.speed = moveSpeed;
            Debug.Log("Initial Move Speed Set: " + moveSpeed);
        }

        enemyAtm = GetComponent<attributesManager>();
        if (enemyAtm == null)
        {
            Debug.LogError("attributesManager component not found on this game object.");
        }

        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0 && !isCharging)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget < stoppingDistance)
            {
                navMeshAgent.isStopped = true;

                if (!isWaitingToCharge)
                {
                    StartCoroutine(WaitAndCharge(target));
                }

                return;
            }
            else
            {
                navMeshAgent.isStopped = false;
            }

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    // Calcular un punto desviado en zig zag
                    Vector3 zigZagDirection = Vector3.Cross(Vector3.up, directionToTarget);
                    Vector3 zigZagDestination = target.position + zigZagDirection * Random.Range(zigZagRangeMin, zigZagRangeMax);

                    navMeshAgent.SetDestination(zigZagDestination);
                }
            }
        }
    }

    private void Update()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f && !isCharging)
        {
            LookAtPlayer();
        }

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < stoppingDistance && !isCharging)
            {
                navMeshAgent.isStopped = true;
            }
        }
    }

    private void LookAtPlayer()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navMeshAgent.angularSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCollidingWithPlayer)
        {
            navMeshAgent.isStopped = true;
            isCollidingWithPlayer = true;

            collisionCount++;

            Debug.Log("Collision Count: " + collisionCount);

            if (collisionCount >= collisionThreshold)
            {
                Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRigidbody != null)
                {
                    Vector3 pushDirection = (collision.gameObject.transform.position - transform.position).normalized;
                    playerRigidbody.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
                    Debug.Log("Player pushed back.");

                    StartCoroutine(SlidePlayerBackwards(playerRigidbody, pushDirection));

                    collisionCount = 0;
                }
                else
                {
                    Debug.LogError("Player does not have a Rigidbody component.");
                }
            }

            var playerAtm = collision.gameObject.GetComponent<attributesManager>();
            if (playerAtm != null && enemyAtm != null)
            {
                enemyAtm.DealDamage(playerAtm.gameObject);
                
                Debug.Log("Vida restante de Player: " + playerAtm.health);
            }
            else
            {
                Debug.LogWarning("attributesManager del jugador o del enemigo es nulo.");
            }

            StartCoroutine(ReactivateMovement());
        }
    }

    private IEnumerator SlidePlayerBackwards(Rigidbody playerRigidbody, Vector3 pushDirection)
    {
        float slideDuration = 0.5f;
        float slideTime = 0f;

        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        while (slideTime < slideDuration)
        {
            playerRigidbody.linearVelocity = pushDirection * pushBackForce;
            slideTime += Time.deltaTime;
            yield return null;
        }

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.constraints = RigidbodyConstraints.None;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private IEnumerator ReactivateMovement()
    {
        yield return new WaitForSeconds(1.0f);
        navMeshAgent.isStopped = false;
        isCollidingWithPlayer = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator WaitAndCharge(Transform target)
    {
        isWaitingToCharge = true;
        yield return new WaitForSeconds(waitTimeBeforeCharge);

        isCharging = true;
        isWaitingToCharge = false;

        navMeshAgent.speed = chargeSpeed;
        Debug.Log("Charge Speed Set: " + chargeSpeed); // Mensaje de log para depuraci�n
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);

        yield return new WaitForSeconds(1.0f);
        isCharging = false;
        navMeshAgent.speed = moveSpeed;
        Debug.Log("Move Speed Reset: " + moveSpeed); // Mensaje de log para depuraci�n
    }
}
