using UnityEngine;

public class TablaCortar : MonoBehaviour
{
    public IngredientInstance ingredientOnBoard;
    public float progress;
    public bool readyToCut = false;

    public Transform stackingPoint;


    void Awake()
    {
        progress = 0;
    }
    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (ingredient.currentState == "Crudo")
        {
            ingredient.transform.SetParent(stackingPoint.transform);
            ingredientOnBoard = ingredient;
            readyToCut = true;
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        
    }

}
