using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sarten : MonoBehaviour
{
    [Header("IngredienteTema")]
    public IngredientInstance ingredientOnPan;
    public bool dentro;
    public Transform stackingPoint;
    public bool readyToCook;

    [Header("JaladodeIngrediente")]
    public List<IngredientInstance> currentIngredients = new List<IngredientInstance>();
    public bool CookCompleted;


    public bool onStove = false;
    private bool flag;

    public float progress;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Cooking")
        {
            flag = true;
        }
    }

    void Update()
    {
        if (onStove && readyToCook && ingredientOnPan != null && ingredientOnPan.canCook)
        {
            cookIngredient(FindObjectOfType<PlayerInteractions>());
        }
    }

    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (ingredient.currentState == "Crudo")
        {
            Debug.Log("Ingrediente crudo aceptado en el sartén");
            ingredient.transform.SetParent(stackingPoint.transform);
            ingredientOnPan = ingredient;
            readyToCook = true;
            return true;
        }

        Debug.Log("Ingrediente no está crudo. Estado actual: " + ingredient.currentState);
        return false;
    }

    public void cookIngredient(PlayerInteractions interactions)
    {
        if (ingredientOnPan.currentState != "Crudo") return;
        if (!readyToCook) return;
        if (ingredientOnPan == null) return;
        progress += 100f * Time.deltaTime;

       
        if (progress >= 100)
        {

            CompleteCook(interactions);
        }

        if(progress>=200)
        {

            BurntCook(interactions);
        }
    }

    public void CompleteCook(PlayerInteractions interactions)
    {
        if (ingredientOnPan == null) return;

        if (ingredientOnPan.cookMesh == null) return;


        // Cambiar el estado del ingrediente a "Cortado"
        ingredientOnPan.currentState = "Cocinado";

        // Cambiar el MeshRenderer y MeshFilter para mostrar la versión cortada
        MeshRenderer renderer = ingredientOnPan.GetComponent<MeshRenderer>();
        if (renderer != null && ingredientOnPan.cutMesh != null)
        {
            // Aplicar la nueva malla (si está disponible)
            MeshFilter filter = ingredientOnPan.GetComponent<MeshFilter>();
            if (filter != null)
            {
                filter.mesh = ingredientOnPan.cutMesh;
            }

        }

        ingredientOnPan.transform.localScale = new Vector3(1.3f, 1.6f, 1.3f);

        MeshCollider collider = ingredientOnPan.GetComponent<MeshCollider>();
        if (collider != null && ingredientOnPan.cutMesh != null)
        {
            // Actualizar el MeshCollider
            collider.sharedMesh = ingredientOnPan.cutMesh;
            collider.enabled = true;
            collider.providesContacts = true;
        }

        ingredientOnPan.transform.SetParent(null);


        // Aplicar fuerza para que salte un poquito (opcional)
        Rigidbody rb = ingredientOnPan.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        // IMPORTANTE: Bloqueamos recoger temporalmente
        StartCoroutine(EnablePickupAfterDelay(ingredientOnPan.gameObject, 1f)); // medio segundo

        // 4. Limpiar referencia
        ingredientOnPan = null;
        progress = 0;
        readyToCook = false;
        interactions.isDoingAnAction = false;

    }

    public void BurntCook(PlayerInteractions interactions)
    {
        if (ingredientOnPan == null) return;

        ingredientOnPan.currentState = "Quemado";
        ingredientOnPan.GetComponent<Renderer>().material.color = Color.black; // efecto simple
        ingredientOnPan.transform.SetParent(null);
        progress = 0;
        readyToCook = false;
        interactions.isDoingAnAction = false;

    }

    private IEnumerator EnablePickupAfterDelay(GameObject ingredient, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Al pasar el tiempo, permitimos recogerlo
        IngredientInstance instance = ingredient.GetComponent<IngredientInstance>();
        if (instance != null)
        {
            instance.canBePickedUp = true; // O cualquier sistema que uses para permitir agarrar
        }
    }
    void OnCollisionEnter(Collision other)
    {
        if (flag) return;

        IngredientInstance ingredient = other.gameObject.GetComponent<IngredientInstance>();
        if (ingredient != null && !ingredient.wasAddedToPlate)
        {
            if (TryAddIngredient(ingredient))
            {
                ingredient.wasAddedToPlate = true;
                Debug.Log("Ingrediente agregado al plato exitosamente.");
            }
            else
            {
                Debug.Log("Ingrediente no compatible, no se agregó.");
            }
        }
    }
}
