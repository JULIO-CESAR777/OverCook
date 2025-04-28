using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Entrega de pedidos")]
    public RecipeSO carriedRecipe; // El plato que lleva el jugador
    public float interactionRange = 2f; // Rango para poder servir
    public KeyCode serveKey = KeyCode.E; // Tecla para servir

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryServeCustomer();
        }
    }

    private void TryServeCustomer()
    {
        // Detectar todos los colliders cerca
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

        foreach (var hit in hits)
        {
            Customer customer = hit.GetComponent<Customer>();
            if (customer != null)
            {
                // Dirección del jugador al cliente
                Vector3 directionToCustomer = (customer.transform.position - transform.position).normalized;

                // Dot product para saber si estamos viendo al cliente
                float dotProduct = Vector3.Dot(transform.forward, directionToCustomer);

                if (dotProduct > 0.7f) // Ajusta el 0.7 si quieres hacerlo más estricto o permisivo
                {
                    customer.ServeOrder(carriedRecipe);
                    return; // Servimos a uno y salimos
                }
                else
                {
                    Debug.Log("El cliente está cerca, pero no estás mirando hacia él.");
                }
            }
        }

        Debug.Log("No hay cliente cerca para servir.");
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el área de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Dibuja la dirección hacia adelante
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
