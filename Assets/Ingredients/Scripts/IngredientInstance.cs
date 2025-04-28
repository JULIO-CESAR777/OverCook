using UnityEngine;

public class IngredientInstance : MonoBehaviour
{
    public IngredientSO ingredientData;
    public string currentState;

    public void Setup(IngredientSO data, string state)
    {
        ingredientData = data;
        currentState = state;
    }
}
