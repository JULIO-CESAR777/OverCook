using Unity.XR.CoreUtils;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject ingredient;
    public IngredientSO ingredientSO;
    public BoxCollider topOfTheBox;
    public Transform spawnPoint;

    private bool canSpawnIngredient;

    void Awake()
    {
        topOfTheBox = GetComponentInChildren<BoxCollider>();
    }

    public void SpawnIngredient()
    {
        Transform child = transform.GetChild(0);
        canSpawnIngredient = child.GetComponent<ChecksTopBox>().canSpawnIngredient;
        if (!canSpawnIngredient)
        {
            return;
        }

        //Instantiate(ingredient, spawnPoint.position, spawnPoint.rotation);
        IngredientState state = ingredientSO.GetStateByStep(PreparationStep.None);
        if (state != null && state.meshPrefab != null)
        {
            GameObject obj = Instantiate(state.meshPrefab, spawnPoint.position, spawnPoint.rotation);
            IngredientInstance instance = obj.GetComponent<IngredientInstance>();
            if (instance != null)
            {
                instance.Setup(ingredientSO, state.stateName);
            }
        }
    }
    
}
