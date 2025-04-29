using UnityEngine;
using System.Collections.Generic;

public enum PreparationStep
{
    None,
    Raw,
    Cut,
    Cook,
    Boil,
    Fry,
    Blend
}

[System.Serializable]
public class IngredientState
{
    public string stateName; // Ej: "Crudo", "Cortado", "Cocido"
    public PreparationStep requiredStep; // Ej: Cut, Cook, etc.
    public GameObject meshPrefab; // Prefab 3D de este estado
}

[CreateAssetMenu(fileName = "IngredientSO", menuName = "Scriptable Objects/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string ingredientName;
    public List<IngredientState> states;

    public IngredientState GetStateByStep(PreparationStep step)
    {
        return states.Find(s => s.requiredStep == step);
    }

    public GameObject GetMeshForState(string stateName)
    {
        var state = states.Find(s => s.stateName == stateName);
        return state != null ? state.meshPrefab : null;
    }
}
