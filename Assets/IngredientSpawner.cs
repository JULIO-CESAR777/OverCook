using Unity.XR.CoreUtils;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ingredient;
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
            Debug.Log("Box is already full. Can't spawn.");
            return;
        }

        Instantiate(ingredient, spawnPoint.position, spawnPoint.rotation);
    }
    
}
