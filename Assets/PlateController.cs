using UnityEngine;
using System.Collections.Generic;
public class PlateController : MonoBehaviour
{
    public List<IngredientSO> currentIngredients = new List<IngredientSO>();
    
    public bool TryAddIngredient(IngredientSO newIngredient)
    {
        // Si el plato está vacío, acepta cualquier ingrediente
        if (currentIngredients.Count == 0)
        {
            currentIngredients.Add(newIngredient);
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
                return true;
            }
        }

        // Si ninguna receta es válida con este conjunto, no se acepta
        return false;
    }

    private bool RecipeMatches(RecipeSO recipe, List<IngredientSO> ingredients)
    {
        // Copia los requerimientos
        List<IngredientRequirement> reqs = new List<IngredientRequirement>(recipe.ingredientsRequired);

        foreach (var ingredient in ingredients)
        {
            var match = reqs.Find(r => r.ingredient == ingredient);
            if (match != null)
            {
                match.quantity--;
                if (match.quantity == 0)
                    reqs.Remove(match);
            }
            else
            {
                return false; // hay un ingrediente que no está en la receta
            }
        }

        return true; // todos los ingredientes están dentro del requerimiento
    }
}
