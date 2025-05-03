using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientNameRequirement
{
    public string ingredientName;
    public int quantity = 1;
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

      
       

    }

    public void CheckRecipeCompletion()
    {
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
                    Debug.Log($"✅ Receta completada: {recipe.recipeName}");
                    gameObject.GetComponent<Rigidbody>().useGravity=true;
                }

                ResetPlate();
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
                if (ing.ingredientData.ingredientName == requirement.ingredientName)
                    count++;
            }

            if (count < requirement.quantity)
                return false;
        }

        return true;
    }

    private void RemoveUsedIngredients(SimpleRecipe recipe)
    {
        List<IngredientInstance> toRemove = new List<IngredientInstance>();

        foreach (var requirement in recipe.requiredIngredients)
        {
            int needed = requirement.quantity;
            foreach (var ing in currentIngredients)
            {
                if (needed > 0 && ing.ingredientData.ingredientName == requirement.ingredientName)
                {
                    toRemove.Add(ing);
                    needed--;
                }
            }
        }

        foreach (var ing in toRemove)
        {
            currentIngredients.Remove(ing);
            Destroy(ing.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IngredientInstance ingredient = other.GetComponent<IngredientInstance>();
        if (ingredient != null)
        {
            Debug.Log("Ingrediente tocó el plato: " + ingredient.ingredientData.ingredientName);

            if (TryAddIngredient(ingredient))
            {
                Debug.Log("Ingrediente agregado al plato exitosamente.");
            }
            else
            {
                Debug.Log("Ingrediente no compatible, no se agregó.");
            }
        }
    }

    // YO JULIOOOO

    private void ResetPlate()
    {
        foreach (var ing in currentIngredients)
        {
            Destroy(ing.gameObject);
        }

        currentIngredients.Clear();
        recipeCompleted = false;
    }
}
