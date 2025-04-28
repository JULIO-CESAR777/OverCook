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
    public IngredientSO ingredient; // El ingrediente en sí
    public string requiredState; // El nombre del estado requerido (por ejemplo, "Crudo", "Cortado", etc.)
    public int quantity; // Cantidad del ingrediente necesario
}