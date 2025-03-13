using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Ataque basico y a distancia")]
    public float cantidad;
    public int amplitud;
    public int casting;
    public float duracion;
    public float cooldown;
    public float alcance;

    [Header("Vision")]
    public int intensidadLuz;
    public int alcanceLuz;
    public int amplitudLuz;
    public int visionRange;

    [Header("Movimiento")]
    public float incercia;
    public float empuje;
    public float rotationSpeed = 5f;

    public float _speed = 5;
    public float _turnSpeed = 360;

    [Header("Acceleration and Deceleration")]
    public float _accelerationRate = 5f;
    public float _decelerationRate = 5f;

    [Header("Body")]
    public GameObject leftArm;
    public GameObject rightArm;

    [Header("Thunder Ability")]
    public float damageAmount = 10f;
    public float damageRadius = 5f;
    public float maxRange = 10f;
    public Transform cameraPivot;
    public float stopDuration = 1f; // Tiempo que el jugador se queda quieto
    public float slideForce = 10f; // Fuerza de deslizamiento

    private bool isLeftArmActive = true;
    private bool isRightArmActive = true;

    private Vector3 _input;
    [SerializeField] private Rigidbody _rb;

    private float _currentSpeed;
    private float _targetSpeed;
    private bool _isMoving;

    private Coroutine _disableArmCoroutine;
    private float _lastArmActivationTime;

    private bool isCooldown = false;
    private Vector3 clickPosition;
    private bool isStopped = false;
    [Header("Turbo attack")]
    private bool isTurboAttacking = false;
    [SerializeField] private float turboAttackCooldown = 10f;
    private bool isTurboAttackCooldown = false;

    // Lista Mutaciones
    private HashSet<Mutation> equippedMutations = new HashSet<Mutation>();

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isStopped)
        {
            GatherInput();
            Look();
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPosition = hit.point;
                targetPosition.y = transform.position.y;

                Vector3 lookDirection = (targetPosition - transform.position).normalized;

                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        if (Input.GetMouseButtonDown(0) && !isTurboAttacking)
        {
            Debug.Log("Click izquierdo detectado");

            if (!rightArm.GetComponent<MeshRenderer>().enabled && CanActivateArm())
            {
                //Debug.Log("Activando LeftArm");
                leftArm.GetComponent<MeshRenderer>().enabled = !leftArm.GetComponent<MeshRenderer>().enabled;
                isLeftArmActive = true;
                _lastArmActivationTime = Time.time;

                if (_disableArmCoroutine != null)
                    StopCoroutine(_disableArmCoroutine);
                _disableArmCoroutine = StartCoroutine(DisableArmAfterDelay(leftArm));
            }
            else
            {
                Debug.Log("LeftArm no se puede activar, RightArm ya está activo o no se puede activar el brazo.");
            }
        }

        if (Input.GetMouseButtonDown(1) && !isTurboAttacking)
        {
            Debug.Log("Click derecho detectado");

            if (!leftArm.GetComponent<MeshRenderer>().enabled && CanActivateArm())
            {
                //Debug.Log("Activando RightArm");
                rightArm.GetComponent<MeshRenderer>().enabled = !rightArm.GetComponent<MeshRenderer>().enabled;
                isRightArmActive = true;
                _lastArmActivationTime = Time.time;

                if (_disableArmCoroutine != null)
                    StopCoroutine(_disableArmCoroutine);
                _disableArmCoroutine = StartCoroutine(DisableArmAfterDelay(rightArm));
            }
            else
            {
                Debug.Log("RightArm no se puede activar, LeftArm ya está activo o no se puede activar el brazo.");
            }
        }

        if (Input.GetMouseButtonDown(2) && !isCooldown)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (Vector3.Distance(transform.position, hit.point) <= maxRange)
                {
                    clickPosition = hit.point;

                    Vector3 lookDirection = (clickPosition - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

                    isCooldown = true;
                    StartCoroutine(ThunderAbilityRoutine());

                    Collider[] hitEnemies = Physics.OverlapSphere(clickPosition, damageRadius);

                    foreach (Collider enemy in hitEnemies)
                    {
                        if (enemy.CompareTag("enemy"))
                        {
                            attributesManager enemyAttributes = enemy.GetComponent<attributesManager>();

                            if (enemyAttributes != null)
                            {
                                enemyAttributes.TakeDamage((int)damageAmount);
                            }
                            else
                            {
                                Debug.LogWarning($"El objeto {enemy.name} no tiene el componente attributesManager.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Fuera de rango");
                }
            }
        }
        else if (Input.GetMouseButtonDown(2) && isCooldown)
        {
            Debug.Log("Habilidad en cooldown");
        }

        if (Input.GetMouseButton(0) && Input.GetMouseButton(1) && !isTurboAttacking && !isLeftArmActive && !isRightArmActive && !isTurboAttackCooldown)
        {
            StartCoroutine(TurboAttackRoutine());
        }
    }

    void FixedUpdate()
    {
        if (!isStopped)
        {
            Move();
        }
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _input.Normalize();
        _isMoving = _input.magnitude > 0;
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {
            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            float currentXAngle = transform.rotation.eulerAngles.x;

            transform.rotation = Quaternion.RotateTowards(rot, Quaternion.Euler(currentXAngle, rot.eulerAngles.y, rot.eulerAngles.z), _turnSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        _targetSpeed = _isMoving ? _speed : 0;
        _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * _accelerationRate);
        _rb.MovePosition(transform.position + (transform.forward * _currentSpeed) * Time.deltaTime);
    }

    bool CanActivateArm()
    {
        return Time.time - _lastArmActivationTime >= 0.3f;
    }

    IEnumerator DisableArmAfterDelay(GameObject arm)
    {
        yield return new WaitForSeconds(0.3f);
        arm.GetComponent<MeshRenderer>().enabled = false;

        if (arm == leftArm)
            isLeftArmActive = false;
        else if (arm == rightArm)
            isRightArmActive = false;

        Debug.Log($"{arm.name} desactivado después del retraso");
    }

    void ResetCooldown()
    {
        isCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(clickPosition, damageRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }

    IEnumerator ThunderAbilityRoutine()
    {
        isStopped = true;

        // Aplicar fuerza de deslizamiento en la dirección opuesta
        Vector3 slideDirection = -transform.forward;
        _rb.AddForce(slideDirection * slideForce, ForceMode.Impulse);

        yield return new WaitForSeconds(stopDuration);

        isStopped = false;
        ResetCooldown();
    }

    IEnumerator TurboAttackRoutine()
    {
        isTurboAttacking = true;

        // Si un brazo está activo, lo desactivamos
        if (isLeftArmActive)
        {
            leftArm.GetComponent<MeshRenderer>().enabled = false;
            isLeftArmActive = false;
        }

        if (isRightArmActive)
        {
            rightArm.GetComponent<MeshRenderer>().enabled = false;
            isRightArmActive = false;
        }

        // Alternar visualización de brazos con intervalos cortos
        for (int i = 0; i < 5; i++)
        {
            // Activate left arm
            leftArm.GetComponent<MeshRenderer>().enabled = true;
            yield return new WaitForSeconds(0.3f);
            leftArm.GetComponent<MeshRenderer>().enabled = false;

            // Activate right arm
            rightArm.GetComponent<MeshRenderer>().enabled = true;
            yield return new WaitForSeconds(0.3f);
            rightArm.GetComponent<MeshRenderer>().enabled = false;
        }

        isTurboAttacking = false;
        StartCoroutine(TurboAttackCooldownRoutine());
    }

    IEnumerator TurboAttackCooldownRoutine()
    {
        isTurboAttackCooldown = true;
        yield return new WaitForSeconds(turboAttackCooldown);
        isTurboAttackCooldown = false;
    }



    public void ApplyMutation(Mutation mutation)
    {
        if (equippedMutations.Contains(mutation))
        {
            Debug.Log("Already equipped with mutation: " + mutation.Name);
        }
        else
        {
            equippedMutations.Add(mutation);
            ApplyMutationEffects(mutation);
            Debug.Log("Equipped with mutation: " + mutation.Name);
        }
    }

    public void RemoveAllMutations()
    {
        foreach (var mutation in equippedMutations)
        {
            Debug.Log("Removed mutation: " + mutation.Name);
            RemoveMutationEffects(mutation);
        }
        equippedMutations.Clear();
    }

    private void ApplyMutationEffects(Mutation mutation)
    {
        switch (mutation.Name)
        {
            case "Garra":
                cantidad *= 2f; // Ejemplo: Aumenta el daño
                alcance *= 2f; // Ejemplo: Aumenta el alcance
                _speed *= 0.5f; // Ejemplo: Reduce la velocidad de movimiento
                _decelerationRate *= 0.5f; // Ejemplo: Aumenta la inercia (reduce la desaceleración)
                break;
            case "Coraza":
                cantidad *= 2f; // Ejemplo: Aumenta la energía
                _speed *= 0.5f; // Ejemplo: Reduce la velocidad de movimiento
                _decelerationRate *= 0.5f; // Ejemplo: Aumenta la inercia (reduce la desaceleración)
                break;
            case "Ojo monstruoso":
                alcance *= 3f; // Ejemplo: Aumenta el alcance del ataque
                // Aquí podrías añadir la lógica del rayo ocular
                break;
            case "Ala atrofiada":
                _speed *= 2f; // Ejemplo: Aumenta la velocidad de movimiento
                _turnSpeed *= 2f; // Ejemplo: Aumenta la velocidad de rotación
                _accelerationRate *= 2f; // Ejemplo: Aumenta la aceleración
                _decelerationRate *= 0.5f; // Ejemplo: Reduce la desaceleración (aumenta la inercia)
                break;
            case "Tentáculo":
                // Aquí podrías añadir la lógica del ataque CC pasivo
                _speed *= 0.5f; // Ejemplo: Reduce la velocidad de movimiento
                _decelerationRate *= 0.5f; // Ejemplo: Aumenta la inercia (reduce la desaceleración)
                break;
            case "Antena Bioluminiscente":
                // Aquí podrías añadir la lógica del ataque 360° con área de acción
                break;
        }
    }

    private void RemoveMutationEffects(Mutation mutation)
    {
        switch (mutation.Name)
        {
            case "Garra":
                cantidad /= 2f; // Revertir el aumento del daño
                alcance /= 2f; // Revertir el aumento del alcance
                _speed /= 0.5f; // Revertir la reducción de la velocidad de movimiento
                _decelerationRate /= 0.5f; // Revertir el aumento de la inercia
                break;
            case "Coraza":
                cantidad /= 2f; // Revertir el aumento de la energía
                _speed /= 0.5f; // Revertir la reducción de la velocidad de movimiento
                _decelerationRate /= 0.5f; // Revertir el aumento de la inercia
                break;
            case "Ojo monstruoso":
                alcance /= 3f; // Revertir el aumento del alcance del ataque
                // Aquí podrías revertir la lógica del rayo ocular
                break;
            case "Ala atrofiada":
                _speed /= 2f; // Revertir el aumento de la velocidad de movimiento
                _turnSpeed /= 2f; // Revertir el aumento de la velocidad de rotación
                _accelerationRate /= 2f; // Revertir el aumento de la aceleración
                _decelerationRate /= 0.5f; // Revertir la reducción de la desaceleración
                break;
            case "Tentáculo":
                // Aquí podrías revertir la lógica del ataque CC pasivo
                _speed /= 0.5f; // Revertir la reducción de la velocidad de movimiento
                _decelerationRate /= 0.5f; // Revertir el aumento de la inercia
                break;
            case "Antena Bioluminiscente":
                // Aquí podrías revertir la lógica del ataque 360° con área de acción
                break;
        }
    }

    public void ApplyClawMutation()
    {
        Mutation claw = new Mutation { Name = "Garra" };
        ApplyMutation(claw);
    }

    public void ApplyArmorMutation()
    {
        Mutation armor = new Mutation { Name = "Coraza" };
        ApplyMutation(armor);
    }

    public void ApplyMonsterEyeMutation()
    {
        Mutation monsterEye = new Mutation { Name = "Ojo monstruoso" };
        ApplyMutation(monsterEye);
    }

    public void ApplyAtrophiedWingMutation()
    {
        Mutation atrophiedWing = new Mutation { Name = "Ala atrofiada" };
        ApplyMutation(atrophiedWing);
    }

    public void ApplyTentacleMutation()
    {
        Mutation tentacle = new Mutation { Name = "Tentáculo" };
        ApplyMutation(tentacle);
    }

    public void ApplyBioluminescentAntennaMutation()
    {
        Mutation bioluminescentAntenna = new Mutation { Name = "Antena Bioluminiscente" };
        ApplyMutation(bioluminescentAntenna);
    }
  





}

