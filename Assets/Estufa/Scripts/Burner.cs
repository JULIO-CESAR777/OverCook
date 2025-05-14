using UnityEngine;

public class Burner : MonoBehaviour
{
    public bool isOccupied = false;
    public GameObject currentPan;
    public float cookingTime = 5f;
    private float timer;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOccupied && other.CompareTag("Pan"))
        {
            currentPan = other.gameObject;
            isOccupied = true;
            timer = 0f;
            Debug.Log("Sartén colocada. Comenzando cocción.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPan)
        {
            currentPan = null;
            isOccupied = false;
            timer = 0f;
            Debug.Log("Sartén retirada. Cocción cancelada.");
        }
    }

    private void Update()
    {
        if (isOccupied && currentPan != null)
        {
            timer += Time.deltaTime;
            Debug.Log("Tiempo cocinandose: " + timer);
            if (timer >= cookingTime)
            {
                Sarten panScript = currentPan.GetComponent<Sarten>();
                if (panScript != null)
                {
                    panScript.CompleteCook();
                    Debug.Log("Se termino cocinandose.");
                }
                timer = 0f;
            }
        }
    }
    
}
