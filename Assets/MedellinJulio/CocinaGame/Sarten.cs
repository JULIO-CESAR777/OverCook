using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sarten : MonoBehaviour
{
    public Transform stackingPoint;
    public IngredientInstance ingredientOnPan;
    public bool readyToCook;
    public float progress;

    public bool TryAddIngredient(GameObject grabObject, IngredientInstance ingredient)
    {
        if (ingredient.currentState == "Crudo" && ingredient.canCook)
        {
            ingredient.transform.SetParent(stackingPoint);
            ingredient.transform.localPosition = Vector3.zero;
            ingredient.transform.rotation = Quaternion.identity;

            Collider col = grabObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = grabObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            //ingredient.wasAddedToPlate = true;

            ingredientOnPan = ingredient;
            readyToCook = true;

            return true;
        }
        return false;
    }

    public void CompleteCook(Sarten pan)
    {
        GameObject ingredientGO = pan.ingredientOnPan.gameObject;
        IngredientInstance ingredient = pan.ingredientOnPan;

        ingredientGO.GetComponent<MeshFilter>().mesh = ingredient.cookMesh;

        // Quitar como hijo de la sartén
        ingredientGO.transform.SetParent(null);

        // Reactivar físicas
        Collider col = ingredientGO.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = ingredientGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;

            // Aplicar una fuerza hacia arriba y hacia adelante
            Vector3 throwDirection = pan.transform.up + pan.transform.forward;
            rb.AddForce(throwDirection.normalized * 10f, ForceMode.Impulse); // Ajusta la fuerza según necesidad
        }

        // Limpiar referencias en la sartén
        pan.ingredientOnPan = null;
        pan.readyToCook = false;


    }

    private void OnCollisionEnter(Collision other)
    {
        IngredientInstance ingredient = other.gameObject.GetComponent<IngredientInstance>();
        if (ingredient != null && !ingredient.wasAddedToPlate)
        {
            if (TryAddIngredient(other.gameObject, ingredient))
            {
                ingredient.wasAddedToPlate = true;
            }
        }
    }
}
