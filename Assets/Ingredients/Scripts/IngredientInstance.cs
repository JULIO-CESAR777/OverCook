using System.Collections.Generic;
using UnityEngine;

public class IngredientInstance : MonoBehaviour
{
    public IngredientSO ingredientData;
    public string currentState;
    public bool canBePickedUp = true;
    public bool wasAddedToPlate = false;
    [Header("Corte")]
    public Mesh cutMesh;

    [Header("Cocinado")]
    public Mesh cookMesh;
    //public List<MeshCollider> meshCollider;

    [Header("Quemado")]
    public Mesh burnedMesh;

    public void Setup(IngredientSO data, string state)
    {
        ingredientData = data;
        currentState = state;
    }

}
