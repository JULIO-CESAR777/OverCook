using UnityEngine;
using System.Collections;

public class CuttingBoard : MonoBehaviour
{

    public IngredientInstance ingredientOnBoard;
    public float progress;
    public bool readyToCut = false;

    public Animator knifeAnimator;
    void Awake()
    {
        progress = 0;
    }
    public bool TryAddIngredient(IngredientInstance ingredient){
        if(ingredient.currentState == "Crudo"){
            ingredientOnBoard = ingredient;
            readyToCut = true;
            return true;
        }
        return false;
    }

    public void cutIngredient(PlayerInteractions interactions){

        if(!readyToCut) return;
        if (ingredientOnBoard == null) return;
        progress += 100f * Time.deltaTime;

        AnimacionCortar();
        if (progress >= 100)
        {
            
            CompleteCut(interactions);
        }
    }

    private void CompleteCut(PlayerInteractions interactions)
    {
        if (ingredientOnBoard == null) return;

        // 1. Cambiar la malla visual
        MeshFilter filter = ingredientOnBoard.GetComponent<MeshFilter>();
        if (filter != null && ingredientOnBoard.cutMesh != null)
        {
            filter.mesh = ingredientOnBoard.cutMesh;
        }

        // 2. Configurar el collider ANTES de cambiar la escala
        MeshCollider collider = ingredientOnBoard.GetComponent<MeshCollider>();
        if (collider != null && ingredientOnBoard.cutMesh != null)
        {
            collider.sharedMesh = ingredientOnBoard.cutMesh;
            collider.convex = true;
            collider.enabled = true;
        }

        // 3. Ajustar escala (opcional, pero si lo necesitas)
        ingredientOnBoard.transform.localScale = Vector3.one; // Resetear primero
        ingredientOnBoard.transform.localScale = new Vector3(1.3f, 1.6f, 1.3f);

        // 4. Recalcular bounds del collider
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }

        // 5. Configurar física
        Rigidbody rb = ingredientOnBoard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse); // Fuerza más suave
        }

        // 6. Configurar para poder agarrarlo
        ingredientOnBoard.currentState = "Cortado";
        ingredientOnBoard.canBeCut = false;
        ingredientOnBoard.canBePickedUp = true; // Permitir agarrar inmediatamente

        // 7. Ajustar posición para mejor detección
        ingredientOnBoard.transform.position += Vector3.up * 0.1f; // Pequeño ajuste vertical

        // 8. Limpiar referencia
        ingredientOnBoard = null;
        progress = 0;
        readyToCut = false;
        interactions.isDoingAnAction = false;

    }

    public void AnimacionCortar()
    {
        knifeAnimator.SetTrigger("Cut");
    }

}
