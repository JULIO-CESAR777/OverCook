using System.Collections.Generic;
using UnityEngine;
using Autohand;
using UnityEngine.SceneManagement;

public class PlateController : MonoBehaviour
{
    public List<IngredientInstance> currentIngredients = new List<IngredientInstance>();
    public bool recipeCompleted;

    [Header("Apilado de ingredientes")]
    public Transform stackingPoint;
    public float stackHeight = 0.05f;

    private bool flag;

    private void Awake()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Cooking") {
            flag = true;
        }
    }

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
        int ingredientIndex = stackingPoint.childCount;

        ingredientInstance.transform.SetParent(stackingPoint);
        Vector3 newPosition = Vector3.up * (stackHeight * ingredientIndex);

        ingredientInstance.transform.localPosition = newPosition;
        ingredientInstance.transform.localRotation = Quaternion.identity;

        Rigidbody rb = ingredientInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = ingredientInstance.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Grabbable grabbable = ingredientInstance.GetComponent<Grabbable>();
        if (grabbable != null)
            grabbable.enabled = false;

        ingredientInstance.canBePickedUp = false;
    }

    public void CheckRecipeCompletion()
    {
        if (currentIngredients.Count == 0) return;

        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            if (RecipeMatches(recipe))
            {
                recipeCompleted = true;

                if (recipe.specialResultPrefab != null && stackingPoint != null)
                {
                    Vector3 spawnPosition = stackingPoint.position + Vector3.up * 0.5f;
                    GameObject result = Instantiate(recipe.specialResultPrefab, spawnPosition, Quaternion.identity);
                    result.name = recipe.specialResultPrefab.name;
                }

                DestroyIngredients();
                Destroy(gameObject);
                return;
            }
        }

        recipeCompleted = false;
    }

    private bool RecipeMatches(RecipeSO recipe)
    {
        foreach (var requirement in recipe.ingredientsRequired)
        {
            int count = 0;
            foreach (var ing in currentIngredients)
            {
                if (ing.ingredientData == requirement.ingredient &&
                    ing.currentState == requirement.requiredState)
                {
                    count++;
                }
            }

            if (count < requirement.quantity)
                return false;
        }

        return true;
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


    private void DestroyIngredients()
    {
        foreach (var ingredient in currentIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient.gameObject);
        }

        currentIngredients.Clear();
    }
}
