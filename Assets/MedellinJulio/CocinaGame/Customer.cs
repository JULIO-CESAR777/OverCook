using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float moveSpeedExit = 0.5f;
    public Transform exitPosition;

    [Header("Pedido")]
    private RecipeSO selectedRecipe;
    public GameObject speechBubblePrefab;
    private GameObject currentSpeechBubble;
   

    [Header("Paciencia")]
    public float patienceTime = 10f;
    private float currentPatience;
    private bool isServed = false;
    private bool esperanding;

    [Header("Puntos")]
    public int pointsForOrder = 100;
    public GameObject floatingPointsPrefab;

    [Header("UI de Paciencia")]
    public GameObject patienceSliderPrefab;
    private Slider patienceSlider;

    // Referencia al spawner
    private CustomerSpawner spawner;
    private int queueIndex;

    private void Start()
    {
        ChooseRandomRecipe();
        currentPatience = patienceTime;

        GameObject exit = GameObject.Find("ExitCustomer");
        if (exit != null)
        {
            exitPosition = exit.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto 'ExitCustomer' en la escena.");
        }
        // Instanciar slider de paciencia
        if (patienceSliderPrefab != null)
        {
            GameObject sliderGO = Instantiate(patienceSliderPrefab, transform.position + new Vector3(0, 3f, 0), Quaternion.identity, transform);
            sliderGO.transform.SetParent(transform); // Sigue al cliente
            patienceSlider = sliderGO.GetComponentInChildren<Slider>();

            if (patienceSlider != null)
            {
                patienceSlider.maxValue = patienceTime;
                patienceSlider.value = currentPatience;
            }
        }
    }

    private void Update()
    {
        if(esperanding)
        {
            if (!isServed)
            {
                currentPatience -= Time.deltaTime;

                if (patienceSlider != null)
                {
                    patienceSlider.value = currentPatience;
                }

                if (currentPatience <= 0)
                {
                    StartCoroutine(MoveToExit()); // Se fue molesto
                }
            }

        }
      
    }

    private void ChooseRandomRecipe()
    {
        var allRecipes = GameManager.Instance.GetAllRecipes();

        if (allRecipes.Count == 0)
        {
            Debug.LogWarning("No hay recetas disponibles.");
            return;
        }

        // Seleccionar una receta al azar (puedes ajustar esto)
        selectedRecipe = allRecipes[Random.Range(0, allRecipes.Count)];
    }

    private void ShowOrder()
    {

        esperanding = true;
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
            Debug.LogWarning("No hay prefab de burbuja asignado.");
        }
    }

    public bool ServeOrder(GameObject servedRecipe)
    {
        if (servedRecipe.gameObject.name == selectedRecipe.recipeName)
        {
            Debug.Log("¡Pedido correcto!");
            isServed = true;

            // Dar puntos al jugador
            GameManager.Instance.AddPoints(pointsForOrder);

            // Mostrar puntos flotantes
            if (floatingPointsPrefab != null)
            {
                Vector3 spawnPosition = transform.position + new Vector3(0, 2f, 0);
                GameObject floating = Instantiate(floatingPointsPrefab, spawnPosition, Quaternion.identity);
                FloatingPoints fp = floating.GetComponent<FloatingPoints>();
                if (fp != null)
                {
                    fp.SetPoints(pointsForOrder);
                }
            }

            StartCoroutine(MoveToExit());
            return true;
        }

        return false;
    }


    private IEnumerator MoveToExit()
    {

        spawner.OnCustomerExit(this);


       
        Vector3 direction = (exitPosition.position - transform.position).normalized;
        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float rotationSpeed = 5f;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(transform.position, exitPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPosition.position, moveSpeedExit * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    // Llamado desde el spawner
    public void SetQueuePosition(CustomerSpawner spawner, int index)
    {
        this.spawner = spawner;
        this.queueIndex = index;

        Transform target = spawner.GetQueueTarget(index);
        if (target != null)
        {
            StartCoroutine(MoveToQueuePosition(target));
            
        }
    }

    private IEnumerator MoveToQueuePosition(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (queueIndex == 0)
        {
            ShowOrder();
        }
    }
}
