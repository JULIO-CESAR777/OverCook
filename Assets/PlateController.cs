using System;
using UnityEngine;
using System.Collections.Generic;
public class PlateController : MonoBehaviour
{
    public List<IngredientInstance> currentIngredients = new List<IngredientInstance>();
    public bool recipeCompleted;

    private void Start()
    {
        recipeCompleted = false;
    }

    // Modificamos TryAddIngredient para que acepte un ingrediente con estado diferente
    public bool TryAddIngredient(IngredientInstance ingredientInstance)
    {
        // Si el plato está vacío, acepta cualquier ingrediente
        if (currentIngredients.Count == 0)
        {
            currentIngredients.Add(ingredientInstance);
            Debug.Log("Agregado primer ingrediente: " + ingredientInstance.ingredientData.ingredientName + " Estado: " + ingredientInstance.currentState);
            CheckRecipeCompletion(); // Verificar si la receta está completa
            return true;
        }

        // Intentar formar una receta con los ingredientes actuales + el nuevo
        List<IngredientInstance> tempIngredients = new List<IngredientInstance>(currentIngredients);
        tempIngredients.Add(ingredientInstance);

        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            if (RecipeMatches(recipe, tempIngredients)) // Pasamos el estado del ingrediente
            {
                currentIngredients.Add(ingredientInstance);
                Debug.Log("Ingrediente agregado correctamente: " + ingredientInstance.ingredientData.ingredientName + " Estado: " + ingredientInstance.currentState);
                CheckRecipeCompletion(); // Verificar si la receta está completa
                return true;
            }
        }

        // Si ninguna receta es válida con este conjunto, no se acepta
        return false;
    }


    
    // Comparamos la receta con los ingredientes, incluyendo el estado
    private bool RecipeMatches(RecipeSO recipe, List<IngredientInstance> ingredients)
    {
        List<IngredientRequirement> reqs = new List<IngredientRequirement>(recipe.ingredientsRequired);

        foreach (var req in reqs)
        {
            Debug.Log("Requisito de receta: " + req.ingredient.ingredientName + " Estado requerido: " + req.requiredState);
            int quantityFound = 0;

            // Iteramos sobre los ingredientes que tenemos en el plato (currentIngredients)
            foreach (var ingredientInstance in ingredients)
            {
                // Comparamos el IngredientSO del plato con el IngredientSO de la receta
                if (ingredientInstance.ingredientData == req.ingredient)
                {
                    // Comprobamos si el estado del ingrediente coincide con el estado requerido por la receta
                    if (ingredientInstance.currentState == req.requiredState)
                    {
                        quantityFound++;
                    }
                    else
                    {
                        Debug.Log("El estado no coincide. Se esperaba: " + req.requiredState + ", pero se encontró: " + ingredientInstance.currentState);
                    }
                }
            }

            // Verificamos si la cantidad encontrada es suficiente
            if (quantityFound < req.quantity)
            {
                Debug.Log("No hay suficientes ingredientes con el estado correcto. Requiere: " + req.quantity + " Encontrados: " + quantityFound);
                return false; // No hay suficientes ingredientes con el estado correcto
            }
        }

        return true; // Todos los ingredientes y estados están correctos
    }

    
    // Comprobamos si la receta está completa, considerando el estado de los ingredientes
    public void CheckRecipeCompletion()
    {
        foreach (var recipe in GameManager.Instance.GetAllRecipes())
        {
            bool recipeMatches = true;

            foreach (var ingredientReq in recipe.ingredientsRequired)
            {
                bool foundIngredientWithCorrectState = false;

                // Buscar si el plato tiene los ingredientes requeridos con el estado correcto
                foreach (var currentIngredient in currentIngredients)
                {
                    if (currentIngredient.ingredientData == ingredientReq.ingredient)
                    {
                        if (currentIngredient.currentState == ingredientReq.requiredState)
                        {
                            foundIngredientWithCorrectState = true;
                            break; // Encontramos el ingrediente con el estado correcto
                        }
                    }
                }

                if (!foundIngredientWithCorrectState)
                {
                    recipeMatches = false;
                    break; // Si no encontramos el ingrediente con el estado correcto, salimos
                }
            }

            if (recipeMatches)
            {
                Debug.Log("Receta completada: " + recipe.recipeName);
                recipeCompleted = true;
                return; // Si la receta está completada, no hace falta continuar revisando
            }
        }

        recipeCompleted = false; // Si no se completó ninguna receta
    }
}
