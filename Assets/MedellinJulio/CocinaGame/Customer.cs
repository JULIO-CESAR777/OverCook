using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;

public class Customer : MonoBehaviour
{

    [Header("ParticleSystem")]

    public ParticleSystem particleHappy;
    public ParticleSystem particleÑo;

    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float moveSpeedExit = 0.5f;
    public Transform exitPosition;

    [Header("Pedido")]
    private RecipeSO selectedRecipe;



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
    private GameObject instantiatedSliderGO;
    private Slider patienceSlider;

    // Referencia al spawner
    private CustomerSpawner spawner;
    private int queueIndex;

    public IconPanel iconPanel;


    private void Start()
    {
        ChooseRandomRecipe();
        patienceTime = Random.Range(30, 40);
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

        iconPanel = FindAnyObjectByType<IconPanel>();

        if (iconPanel == null)
        {
            Debug.LogWarning("No se encontró la pantalla externa.");
        }

    }

    private void Update()
    {
        if (esperanding)
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

        if (patienceSliderPrefab != null)
        {
            instantiatedSliderGO = Instantiate(patienceSliderPrefab, transform.position + new Vector3(0, 2f, 0), Quaternion.identity, transform);

            instantiatedSliderGO.transform.SetParent(transform); // Sigue al cliente
            patienceSlider = instantiatedSliderGO.GetComponentInChildren<Slider>();
            if (patienceSlider != null)
            {
                patienceSlider.maxValue = patienceTime;
                patienceSlider.value = currentPatience;
            }
        }
        else
        {
            Debug.LogWarning("No hay prefab de slider asignado.");
        }


        if (iconPanel != null && selectedRecipe != null)
        {
            iconPanel.SetRecipeIcon(selectedRecipe.icon);
        }
    }

    public bool ServeOrder(GameObject servedRecipe)
    {




        if (servedRecipe.gameObject.name == selectedRecipe.recipeName)
        {
            ParticleSystem psHappy = Instantiate(particleHappy, transform.position + new Vector3(0, 3f, 0), Quaternion.identity);
            psHappy.Play();
            Destroy(psHappy.gameObject, psHappy.main.duration);

            // ✅ Destruir el slider y la burbuja si existen

            if (instantiatedSliderGO != null)
            {
                Destroy(instantiatedSliderGO);
            }

            isServed = true;

            // Dar puntos al jugador
            GameManager.Instance.AddPoints(pointsForOrder);

            // Mostrar puntos flotantes
            if (floatingPointsPrefab != null)
            {
                Debug.Log("Puntos Volando");
                Vector3 spawnPosition = transform.position + new Vector3(0, 3f, 0);
                GameObject floating = Instantiate(floatingPointsPrefab, spawnPosition, Quaternion.identity);

                //ROTAR LA CAMARAAAAA AAAAAAAAAAAAAAAAAAAAAAAAAAA
                floating.transform.LookAt(Camera.main.transform);
                floating.transform.Rotate(0, 180f, 0);

                FloatingPoints fp = floating.GetComponent<FloatingPoints>();
                if (fp != null)
                {
                    fp.SetPoints(pointsForOrder);
                }
            }

            //AGREGUE ESTO PARA VRRRRRRRR, ESTOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO NO SE OLVIEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            Destroy(servedRecipe);
            StartCoroutine(MoveToExit());
            return true;
        }
        else
        {
            ParticleSystem psÑo = Instantiate(particleÑo, transform.position + new Vector3(0, 3f, 0), Quaternion.identity);
            psÑo.Play();
            Destroy(psÑo.gameObject, psÑo.main.duration);

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

        if (queueIndex == 0)
        {
            ShowOrder();

            // Mostrar ícono de receta en la pantalla externa
            if (iconPanel != null && selectedRecipe != null)
            {
                iconPanel.SetRecipeIcon(selectedRecipe.icon);
            }
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
    // Agrega este método público al final:
    public RecipeSO GetRecipe()
    {
        return selectedRecipe;
    }


    //AGREGANDO ESTO SOLO PARA VR
    private void OnTriggerEnter(Collider other)
    {

        //Debug.Log("Entre a colisionar");
        if (other.CompareTag("Recipe") && esperanding)
        {
            Debug.Log("Entre en lka instancia");

            ServeOrder(other.gameObject);

        }
    }
}