using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Scriptable Objects/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public Sprite icon;
    public List<IngredientRequirement> ingredientsRequired;
}

[System.Serializable]
public class IngredientRequirement
{
    public IngredientSO ingredient;
    public string requiredState; // Ej: "Cocido"
    public int quantity;
}