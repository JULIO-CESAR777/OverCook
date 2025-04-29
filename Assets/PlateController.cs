using System.Collections.Generic;
using UnityEngine;

public class PlateController : MonoBehaviour
{
    public List<IngredientInstance> currentIngredients = new List<IngredientInstance>();
    public bool recipeCompleted;

    [Header("Apilado de ingredientes")]
    public Transform stackingPoint; // Punto donde empezar a apilar
    public float stackHeight = 0.05f; // Altura entre ingredientes apilados

    private void Start()
    {
        recipeCompleted = false;
    }

    public bool TryAddIngredient(IngredientInstance ingredientInstance)
    {
        if (ingredientInstance == null)
            return false;

        // Instanciar visualmente el ingrediente sobre el plato
        StackIngredient(ingredientInstance);

        // Agregar el ingrediente al plato
        currentIngredients.Add(ingredientInstance);

        // Verificar si al agregar este ingrediente, completamos una receta
        CheckRecipeCompletion();

        return true;
    }

    private void StackIngredient(IngredientInstance ingredientInstance)
    {
        // Acomoda el ingrediente como hijo del plato
        ingredientInstance.transform.SetParent(stackingPoint);

        // Calcula la posición apilada
        Vector3 newPosition = Vector3.up * (stackHeight * currentIngredients.Count);
        ingredientInstance.transform.localPosition = newPosition;

        // Opcional: Resetear rotación si quieres que siempre estén derechos
        ingredientInstance.transform.localRotation = Quaternion.identity;
    }

    public void CheckRecipeCompletion()
    {
        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            if (RecipeMatches(recipe))
            {
                Debug.Log("Receta completada: " + recipe.recipeName);
                recipeCompleted = true;
                return;
            }
        }
        recipeCompleted = false;
    }

    private bool RecipeMatches(RecipeSO recipe)
    {
        List<IngredientRequirement> requirements = new List<IngredientRequirement>(recipe.ingredientsRequired);
        List<IngredientInstance> availableIngredients = new List<IngredientInstance>(currentIngredients);

        foreach (var requirement in requirements)
        {
            int requiredAmount = requirement.quantity;

            for (int i = availableIngredients.Count - 1; i >= 0; i--)
            {
                var ing = availableIngredients[i];

                if (ing.ingredientData == requirement.ingredient && ing.currentState == requirement.requiredState)
                {
                    requiredAmount--;
                    availableIngredients.RemoveAt(i); // Usamos este ingrediente
                    if (requiredAmount == 0)
                        break;
                }
            }

            if (requiredAmount > 0)
            {
                return false; // Faltan ingredientes de este tipo
            }
        }

        return true; // Todos los requisitos cumplidos
    }

    private void OnTriggerEnter(Collider other)
    {
        IngredientInstance ingredient = other.GetComponent<IngredientInstance>();
        if (ingredient != null)
        {
            Debug.Log("Ingrediente tocó el plato: " + ingredient.ingredientData.ingredientName);

            // Intentar agregarlo automáticamente
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

}
