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

    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (ingredient.currentState == "Crudo" && ingredient.canCook)
        {
            ingredient.transform.SetParent(stackingPoint);
            ingredient.transform.localPosition = Vector3.zero;
            ingredientOnPan = ingredient;
            readyToCook = true;
            return true;
        }
        return false;
    }

    public void CompleteCook()
    {
        if (ingredientOnPan == null || ingredientOnPan.cutMesh == null) return;

        ingredientOnPan.currentState = "Cocinado";

        MeshFilter filter = ingredientOnPan.GetComponent<MeshFilter>();
        if (filter != null) filter.mesh = ingredientOnPan.cutMesh;

        MeshRenderer renderer = ingredientOnPan.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.material.color = Color.gray;

        MeshCollider col = ingredientOnPan.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = ingredientOnPan.cutMesh;
            col.enabled = true;
        }

        ingredientOnPan.transform.localScale = new Vector3(1.3f, 1.6f, 1.3f);
        ingredientOnPan.transform.SetParent(null);

        Rigidbody rb = ingredientOnPan.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        StartCoroutine(EnablePickupAfterDelay(ingredientOnPan.gameObject, 1f));

        ingredientOnPan = null;
        readyToCook = false;
        progress = 0;
    }

    private IEnumerator EnablePickupAfterDelay(GameObject ingredient, float delay)
    {
        yield return new WaitForSeconds(delay);
        IngredientInstance instance = ingredient.GetComponent<IngredientInstance>();
        if (instance != null) instance.canBePickedUp = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        IngredientInstance ingredient = other.gameObject.GetComponent<IngredientInstance>();
        if (ingredient != null && !ingredient.wasAddedToPlate)
        {
            if (TryAddIngredient(ingredient))
            {
                ingredient.wasAddedToPlate = true;
            }
        }
    }
}
