using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public string tag1 = "Ingredient";
    public string tag2 = "Plate";
    public string tag3 = "Recipe";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tag1) || other.CompareTag(tag2) || other.CompareTag(tag3))
        {
            Destroy(other.gameObject);
            Debug.Log($"{other.name} fue destruido por el bote de basura.");
        }
    }
}
