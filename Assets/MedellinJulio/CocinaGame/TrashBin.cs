using UnityEngine;

public class TrashBin : MonoBehaviour
{
   
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Debug.Log($"{other.name} fue destruido por el bote de basura.");
        }
    }
}
