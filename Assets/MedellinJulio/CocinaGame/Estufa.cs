using UnityEngine;

public class Estufa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient")) 
        {
            IngredientInstance ingredient = other.gameObject.GetComponent<IngredientInstance>();
          

        }
    }
}
