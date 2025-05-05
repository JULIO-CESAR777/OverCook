using UnityEngine;

public class TablaCortar : MonoBehaviour
{
    public IngredientInstance ingredientOnBoard;
    public float progress;
    public bool readyToCut = false;
    public bool dentro;
    public Transform stackingPoint;


    void Awake()
    {
        progress = 0;
        dentro = false;
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


    public void OnTriggerEnter(Collider other)
    {
       

        if (other.CompareTag("Ingredient")) 
        {
            dentro = true;
            Debug.Log(dentro);
            IngredientInstance instance = other.GetComponent<IngredientInstance>();

            TryAddIngredient(instance );
        }
    }

}
