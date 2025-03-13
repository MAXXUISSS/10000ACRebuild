using UnityEngine;

public class attributesManager : MonoBehaviour
{
    public int health;
    public int attack;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Vida restante de {gameObject.name}: {health}");
        if (health <= 0)
        {
            Debug.Log($"El objeto {gameObject.name} ha sido destruido.");
            Destroy(gameObject); 
        }
    }

    public void DealDamage(GameObject target)
    {
        var atm = target.GetComponent<attributesManager>();
        if (atm != null)
        {
            Debug.Log($"Daño infligido a {target.name} por {gameObject.name}: {attack}");
            atm.TakeDamage(attack);
        }
        else
        {
            Debug.LogWarning($"No se encontró attributesManager en {target.name}");
        }
    }
}
