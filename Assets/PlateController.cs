using System;
using UnityEngine;
using System.Collections.Generic;
public class PlateController : MonoBehaviour
{
    public List<IngredientSO> currentIngredients = new List<IngredientSO>();
    public bool recipeCompleted;

    private void Start()
    {
        recipeCompleted = false;
    }

    public bool TryAddIngredient(IngredientSO newIngredient)
    {
        
        // Verificar si la receta ya está completa
        if (IsRecipeComplete())
        {
            Debug.Log("La receta ya está completa. No se pueden agregar más ingredientes.");
            return false;
        }
        
        // Si el plato está vacío, acepta cualquier ingrediente
        if (currentIngredients.Count == 0)
        {
            currentIngredients.Add(newIngredient);
            CheckRecipeCompletion(); // Verificar si la receta está completa
            return true;
        }

        // Intentar formar una receta con los ingredientes actuales + el nuevo
        List<IngredientSO> tempIngredients = new List<IngredientSO>(currentIngredients);
        tempIngredients.Add(newIngredient);

        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            if (RecipeMatches(recipe, tempIngredients))
            {
                currentIngredients.Add(newIngredient);
                CheckRecipeCompletion(); // Verificar si la receta está completa
                return true;
            }
        }

        // Si ninguna receta es válida con este conjunto, no se acepta
        return false;
    }


    private bool RecipeMatches(RecipeSO recipe, List<IngredientSO> ingredients)
    {
        // Copiar los requerimientos de la receta
        List<IngredientRequirement> reqs = new List<IngredientRequirement>(recipe.ingredientsRequired);

        // Iterar sobre los requerimientos de ingredientes
        foreach (var req in reqs)
        {
            int quantityFound = 0;

            // Contar cuántos de este ingrediente están en el plato
            foreach (var ingredient in ingredients)
            {
                if (ingredient == req.ingredient)
                {
                    quantityFound++;
                }
            }

            // Verificar si la cantidad encontrada es suficiente
            if (quantityFound < req.quantity)
            {
                return false; // No hay suficientes de este ingrediente
            }
        }

        return true; // Todos los ingredientes y cantidades están correctos
    }
    
    public void CheckRecipeCompletion()
    {
        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            if (RecipeMatches(recipe, currentIngredients))
            {
                // Si la receta es válida, muestra un mensaje o retroalimentación
                // Puedes hacer más acciones aquí, como reproducir un sonido o mostrar una UI
                recipeCompleted = true;
            }
        }
    }

    private bool IsRecipeComplete()
    {
        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            // Verificar si la receta está completa
            if (RecipeMatches(recipe, currentIngredients))
            {
                return true; // La receta está completa
            }
        }

        return false; // La receta no está completa
    }

}
