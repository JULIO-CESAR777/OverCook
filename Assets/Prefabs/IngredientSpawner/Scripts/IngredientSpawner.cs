using Autohand;
using Unity.XR.CoreUtils;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject ingredient;
    public IngredientSO ingredientSO;
    public Transform spawnPoint;

    public void SpawnIngredient()
    {
        Transform child = transform.GetChild(0);

        // Aquí se busca el estado y el prefab correspondiente basado en `stateName`
        foreach (var state in ingredientSO.states)
        {
            if (state != null && state.meshPrefab != null)
            {
                GameObject obj = Instantiate(state.meshPrefab, spawnPoint.position, spawnPoint.rotation);
                IngredientInstance instance = obj.GetComponent<IngredientInstance>();
                if (instance != null)
                {
                    instance.Setup(ingredientSO, state.stateName); // Asignar estado a la instancia
                }
            }
        }
    }
    
}
