using UnityEngine;

public class TrashBin : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient") || other.CompareTag("Recipe") || other.CompareTag("Plate") || other.CompareTag("Pan"))
        {
            Destroy(other.gameObject);
          
        }
    }
}
