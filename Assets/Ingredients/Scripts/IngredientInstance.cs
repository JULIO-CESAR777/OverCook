using System.Collections.Generic;
using UnityEngine;

public class IngredientInstance : MonoBehaviour
{
    public IngredientSO ingredientData;
    public string currentState;
    public bool canBePickedUp = true;

    [Header("Corte")]
    public List<Mesh> cutMesh;
    //public List<MeshCollider> meshCollider;

    public void Setup(IngredientSO data, string state)
    {
        ingredientData = data;
        currentState = state;
    }

}
