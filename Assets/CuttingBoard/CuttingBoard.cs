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

        // Cambiar el estado del ingrediente a "Cortado"
        ingredientOnBoard.currentState = "Cortado";
        
        // Cambiar el MeshRenderer y MeshFilter para mostrar la versión cortada
        MeshRenderer renderer = ingredientOnBoard.GetComponent<MeshRenderer>();
        if (renderer != null && ingredientOnBoard.cutMesh != null)
        {
            // Aplicar la nueva malla (si está disponible)
            MeshFilter filter = ingredientOnBoard.GetComponent<MeshFilter>();
            if (filter != null)
            {
                filter.mesh = ingredientOnBoard.cutMesh;
            }
            
        }
        
        ingredientOnBoard.transform.localScale = new Vector3(1.3f, 1.6f, 1.3f);
        
        MeshCollider collider = ingredientOnBoard.GetComponent<MeshCollider>();
        if (collider != null && ingredientOnBoard.cutMesh != null)
        {
            // Actualizar el MeshCollider
            collider.enabled = false;
            collider.sharedMesh = null;

            collider.sharedMesh = ingredientOnBoard.cutMesh;
            collider.convex = true; // Si lo necesitas, especialmente si el objeto se mueve o colisiona
            collider.enabled = true;
        }
        
        ingredientOnBoard.transform.SetParent(null);
        
        
        // Aplicar fuerza para que salte un poquito (opcional)
        Rigidbody rb = ingredientOnBoard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * 4f, ForceMode.Impulse);
        }

        // IMPORTANTE: Bloqueamos recoger temporalmente
        StartCoroutine(EnablePickupAfterDelay(ingredientOnBoard.gameObject, 1f)); // medio segundo

        // 4. Limpiar referencia
        ingredientOnBoard = null;
        progress = 0;
        readyToCut = false;
        interactions.isDoingAnAction = false;
        
    }

    private IEnumerator EnablePickupAfterDelay(GameObject ingredient, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Al pasar el tiempo, permitimos recogerlo
        IngredientInstance instance = ingredient.GetComponent<IngredientInstance>();
        if (instance != null)
        {
            instance.canBePickedUp = true; // O cualquier sistema que uses para permitir agarrar
        }
    }

    public void AnimacionCortar()
    {
        knifeAnimator.SetTrigger("Cut");


    }

}
