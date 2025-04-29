using UnityEngine;
using TMPro; // Para mostrar el nombre del pedido
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform targetPosition; // Punto donde se debe parar (en la barra)
    public Transform exitPosition;
    public float moveSpeed = 2f;

    [Header("Pedido")]
    private RecipeSO selectedRecipe;
    public GameObject speechBubblePrefab; // Prefab para mostrar el pedido
    private GameObject currentSpeechBubble;

    [Header("Paciencia")]
    public float patienceTime = 10f; // Segundos que el cliente espera
    private float currentPatience;

    private bool isServed = false;

    private void Start()
    {
        ChooseRandomRecipe();
        currentPatience = patienceTime;

        // Empieza el movimiento hacia la barra
        StartCoroutine(MoveToBar());
    }

    private void Update()
    {
        if (!isServed)
        {
            currentPatience -= Time.deltaTime;

            if (currentPatience <= 0)
            {
                LeaveRestaurant(false); // Se fue enojado
            }
        }
    }

    private void ChooseRandomRecipe()
    {
        var allRecipes = GameManager.Instance.GetAllRecipes();
        if (allRecipes.Count == 0)
        {
            Debug.LogWarning("No hay recetas disponibles para elegir.");
            return;
        }

        int randomIndex = Random.Range(0, allRecipes.Count);
        selectedRecipe = allRecipes[randomIndex];

        Debug.Log("El cliente ha pedido: " + selectedRecipe.recipeName);
    }

    private IEnumerator MoveToBar()
    {
        while (Vector3.Distance(transform.position, targetPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Cuando llegue, muestra el pedido
        ShowOrder();
    }

    private void ShowOrder()
    {
        if (speechBubblePrefab != null)
        {
            currentSpeechBubble = Instantiate(speechBubblePrefab, transform.position + new Vector3(0, 4f, 0), Quaternion.identity, transform);
            var textComponent = currentSpeechBubble.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = selectedRecipe.recipeName;
            }
        }
        else
        {
            Debug.LogWarning("No asignaste un prefab de speech bubble.");
        }
    }

    public void ServeOrder(RecipeSO servedRecipe)
    {
        if (servedRecipe == selectedRecipe)
        {
            Debug.Log("¡Pedido correcto!");
            isServed = true;
            LeaveRestaurant(true);
        }
        else
        {
            Debug.Log("Pedido incorrecto.");
            // Puedes decidir penalizar aquí
        }
    }

    private void LeaveRestaurant(bool happy)
    {
        

        if (happy)
        {
            Debug.Log("El cliente se fue feliz.");
        }
        else
        {
            Debug.Log("El cliente se fue molesto.");
        }
        MoveToExit();
        Destroy(gameObject); // El cliente desaparece
    }

    private IEnumerator MoveToExit()
    {
        while (Vector3.Distance(transform.position, exitPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPosition.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Cuando llegue, muestra el pedido
        ShowOrder();
    }
}
