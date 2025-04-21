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
        canSpawnIngredient = true;
    }

    public void SpawnIngredient()
    {
        if (!canSpawnIngredient)
        {
            Debug.Log("Box is already full. Can't spawn.");
            return;
        }

        Instantiate(ingredient, spawnPoint.position, spawnPoint.rotation);
        canSpawnIngredient = false;
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



}
