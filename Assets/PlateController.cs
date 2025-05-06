using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientNameRequirement
{
    public string ingredientName;
    public string requiredState;
    public int quantity = 2;
}

[System.Serializable]
public class SimpleRecipe
{
    public string recipeName;
    public List<IngredientNameRequirement> requiredIngredients;
    public GameObject resultPrefab;
}

public class PlateController : MonoBehaviour
{
    public List<IngredientInstance> currentIngredients = new List<IngredientInstance>();
    public bool recipeCompleted;

    [Header("Apilado de ingredientes")]
    public Transform stackingPoint;
    public float stackHeight = 0.05f;
    

    [Header("Punto de aparición del resultado especial")]
    public Transform spawnPoint;

    [Header("Recetas disponibles")]
    public List<SimpleRecipe> possibleRecipes;

    private void Start()
    {
        recipeCompleted = false;
    }

    public bool TryAddIngredient(IngredientInstance ingredientInstance)
    {
        if (ingredientInstance == null)
            return false;

        StackIngredient(ingredientInstance);
        currentIngredients.Add(ingredientInstance);
        CheckRecipeCompletion();

        return true;
    }

    private void StackIngredient(IngredientInstance ingredientInstance)
    {
        ingredientInstance.transform.SetParent(stackingPoint);
        Vector3 newPosition = Vector3.up * (stackHeight * stackingPoint.childCount);
        ingredientInstance.transform.localPosition = newPosition;
        ingredientInstance.transform.localRotation = Quaternion.identity;

        Rigidbody rb = ingredientInstance.GetComponent<Rigidbody>();

       
        if (rb != null)
        {
            rb.isKinematic = true; // Desactiva la física mientras se mueve
            rb.detectCollisions = false;
        }

        Collider col = ingredientInstance.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // Evita colisiones mientras se mueve
        }


      


    }

    public void CheckRecipeCompletion()
    {
        if (currentIngredients.Count == 0) return;
        foreach (var recipe in possibleRecipes)
        {
            if (RecipeMatches(recipe))
            {
                recipeCompleted = true;
                
                if (recipe.resultPrefab != null && stackingPoint != null)
                {
                    Vector3 spawnPosition = stackingPoint.position + Vector3.up * 0.5f; // Aumenta 0.3 en Y
                    GameObject result= Instantiate(recipe.resultPrefab, stackingPoint.position, Quaternion.identity);
                    result.name = recipe.resultPrefab.name;
                  
                    gameObject.GetComponent<Rigidbody>().useGravity=true;
                }

              
                Destroy(gameObject);
                return;
            }
        }

        recipeCompleted = false;
    }

    private bool RecipeMatches(SimpleRecipe recipe)
    {

      
       
        foreach (var requirement in recipe.requiredIngredients)
        {
            int count = 0;
            foreach (var ing in currentIngredients)
            {
                if (ing.ingredientData.ingredientName == requirement.ingredientName &&
                ing.currentState == requirement.requiredState)
                    count++;
            }

           
            if (count < requirement.quantity)
                return false;
        }

        return true;
    }



    private void OnTriggerEnter(Collider other)
    {

        IngredientInstance ingredient = other.GetComponent<IngredientInstance>();
        if (ingredient != null)
        {

            if (ingredient.wasAddedToPlate) return; // ⚠️ Ya fue agregado

          

            if (TryAddIngredient(ingredient))
            {
                //SOLO LE AÑADI ESTO PARA VR PERO ALCH NO SE SI SI AFECTE PERO SI FUNCIONA NO LE MUEVO
                ingredient.wasAddedToPlate = true;
                Debug.Log("Ingrediente agregado al plato exitosamente.");
            }
            else
            {
                Debug.Log("Ingrediente no compatible, no se agregó.");
            }
        }
    }

    // YO JULIOOOO

}
