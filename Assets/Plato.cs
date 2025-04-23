using UnityEngine;
using System.Collections.Generic;
public class Plato : MonoBehaviour
{
    public List<IngredientInstance> ingredientsOnPlate = new List<IngredientInstance>();

    private void OnTriggerEnter(Collider other)
    {
        var ingredient = other.GetComponent<IngredientInstance>();
        if (ingredient != null && !ingredientsOnPlate.Contains(ingredient))
        {
            ingredientsOnPlate.Add(ingredient);
            Debug.Log($"Se colocó {ingredient.ingredientData.ingredientName} en el plato");
        }
    }
}
