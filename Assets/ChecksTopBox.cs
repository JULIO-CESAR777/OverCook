using System;
using UnityEngine;

public class ChecksTopBox : MonoBehaviour
{
    
    public bool canSpawnIngredient;

    private void Awake()
    {
        canSpawnIngredient = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            Debug.Log("Ingredient entered box");
            canSpawnIngredient = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            Debug.Log("Ingredient left the box");
            canSpawnIngredient = true;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            canSpawnIngredient = false;
        }
    }
}
