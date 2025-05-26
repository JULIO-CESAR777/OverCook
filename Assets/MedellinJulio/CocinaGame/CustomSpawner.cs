using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject customerPrefab;
    public Transform[] queuePositions; // 0 = barra, el resto = fila
    public float spawnDelay = 5f;
    public IconPanel iconPanel; // ← Añadir esto desde el inspector

    private Queue<Customer> customerQueue = new Queue<Customer>();

    private void Start()
    {
        SpawnCustomer();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        if (customerQueue.Count >= queuePositions.Length)
        {
            return;
        }

        GameObject newCustomerGO = Instantiate(customerPrefab, transform.position, Quaternion.identity);
        Customer newCustomer = newCustomerGO.GetComponent<Customer>();

        int positionIndex = customerQueue.Count;
        newCustomer.SetQueuePosition(this, positionIndex);

        customerQueue.Enqueue(newCustomer);

        // Si es el primero, mostrar icono
        if (positionIndex == 0 && iconPanel != null)
        {
            iconPanel.SetRecipeIcon(newCustomer.GetRecipe()?.icon);
        }
    }

    public void OnCustomerExit(Customer customer)
    {
        customerQueue = new Queue<Customer>(customerQueue); // recrea para evitar errores al remover
        customerQueue = new Queue<Customer>(new List<Customer>(customerQueue).FindAll(c => c != customer));

        // Reasignar posiciones
        int i = 0;
        foreach (var c in customerQueue)
        {
            c.SetQueuePosition(this, i);
            i++;
        }

        // Actualizar pantalla externa
        if (customerQueue.Count > 0)
        {
            RecipeSO recipe = customerQueue.Peek().GetRecipe();
            iconPanel.SetRecipeIcon(recipe != null ? recipe.icon : null);
        }
        else
        {
            iconPanel.SetRecipeIcon(null);
        }
    }

    public Transform GetQueueTarget(int index)
    {
        if (index >= 0 && index < queuePositions.Length)
            return queuePositions[index];

        return null;
    }
}
