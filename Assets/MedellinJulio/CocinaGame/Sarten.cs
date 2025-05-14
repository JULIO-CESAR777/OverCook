using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public float progress;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (ingredient.currentState == "Crudo")
        {
            ingredient.transform.SetParent(stackingPoint.transform);
            ingredientOnPan = ingredient;
            readyToCook = true;
            return true;
        }
        return false;
    }

    public void cookIngredient(PlayerInteractions interactions)
    {

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

        // Cambiar el estado del ingrediente a "Cortado"
        ingredientOnPan.currentState = "Cortado";

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
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {

            dentro = true;
        }
    }
}
