using UnityEngine;
using System.Collections;

public class CuttingBoard : MonoBehaviour
{

    public IngredientInstance ingredientOnBoard;
    public float progress;
    private bool canPickUpAfterCut = false;
    public bool readyToCut = false;

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

    public void cutIngredient(){

        if(!readyToCut) return;
        if (ingredientOnBoard == null) return;
        progress += 100f * Time.deltaTime;
 

        if (progress >= 100)
        {
            Debug.Log("se termino de cortar");
            CompleteCut();
        }
    }

    private void CompleteCut()
    {
        if (ingredientOnBoard == null) return;

        // 1. Guardar posición
        Vector3 spawnPosition = ingredientOnBoard.transform.position + Vector3.up * 0.1f;
        Quaternion spawnRotation = ingredientOnBoard.transform.rotation;

        GameObject cutIngredient = null;

        // 2. Spawnear nuevo objeto cortado
        if (ingredientOnBoard.cutVersionPrefab != null)
        {
            cutIngredient = Instantiate(ingredientOnBoard.cutVersionPrefab, spawnPosition, spawnRotation);

            // Aplicar fuerza para que salte un poquito (opcional)
            Rigidbody rb = cutIngredient.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("No hay prefab de versión cortada asignado al ingrediente.");
        }

         // 3. Destruir el ingrediente crudo original
        Destroy(ingredientOnBoard.gameObject);

        // IMPORTANTE: Bloqueamos recoger temporalmente
        canPickUpAfterCut = false;
        StartCoroutine(EnablePickupAfterDelay(ingredientOnBoard.gameObject, 0.5f)); // medio segundo

        // 4. Limpiar referencia
        ingredientOnBoard = null;
        progress = 0;
        readyToCut = false;

        // 5. Iniciar el delay para permitir agarrar
        if (cutIngredient != null)
        {
            StartCoroutine(EnablePickupAfterDelay(cutIngredient, 0.5f)); // 0.5 segundos de espera
        }
    }

    private IEnumerator EnablePickupAfterDelay(GameObject ingredient, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Al pasar el tiempo, permitimos recogerlo
        IngredientInstance instance = ingredient.GetComponent<IngredientInstance>();
        if (instance != null)
        {
            Debug.Log("ya lo puedes agarrar");
            instance.canBePickedUp = true; // O cualquier sistema que uses para permitir agarrar
        }
    }

}
