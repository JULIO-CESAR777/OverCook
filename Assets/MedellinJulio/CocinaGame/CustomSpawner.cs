using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Configuraci�n")]
    public GameObject customerPrefab;
    public Transform[] queuePositions; // 0 = barra, el resto = fila
    public float spawnDelay = 5f;

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
            //Debug.Log("Fila llena, no se puede spawnear otro cliente.");
            return;
        }

        GameObject newCustomerGO = Instantiate(customerPrefab, transform.position, Quaternion.identity);
        Customer newCustomer = newCustomerGO.GetComponent<Customer>();

        int positionIndex = customerQueue.Count;
        newCustomer.SetQueuePosition(this, positionIndex);

        customerQueue.Enqueue(newCustomer);
    }

    public void OnCustomerExit(Customer exitedCustomer)
    {
        Queue<Customer> tempQueue = new Queue<Customer>();

        foreach (var customer in customerQueue)
        {
            if (customer != exitedCustomer)
                tempQueue.Enqueue(customer);
        }

        customerQueue = tempQueue;

        // Reorganiza la fila
        int index = 0;
        foreach (Customer customer in customerQueue)
        {
            customer.SetQueuePosition(this, index);
            index++;
        }
    }

    public Transform GetQueueTarget(int index)
    {
        if (index >= 0 && index < queuePositions.Length)
            return queuePositions[index];

        return null;
    }
}
