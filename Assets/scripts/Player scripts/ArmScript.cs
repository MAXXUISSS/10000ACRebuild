using UnityEngine;

public class ArmScript : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Collider collider;
    public attributesManager playerAtm;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();

        Debug.Log($"{gameObject.name} - Iniciando con MeshRenderer.enabled: {meshRenderer.enabled}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy"))
        {
            if (meshRenderer.enabled)
            {
                Debug.Log($"{gameObject.name} - Colisión con enemigo detectada");

                var enemyAtm = other.GetComponent<attributesManager>();
                if (playerAtm != null && enemyAtm != null)
                {
                    playerAtm.DealDamage(enemyAtm.gameObject);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} - playerAtm o enemyAtm es nulo.");
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} - Colisión detectada pero MeshRenderer está desactivado");
            }
        }
    }

    private void Update()
    {
        if (meshRenderer.enabled != collider.enabled)
        {
            collider.enabled = meshRenderer.enabled;
            //Debug.Log($"{gameObject.name} - Collider activado: {collider.enabled}");
        }
    }
}
