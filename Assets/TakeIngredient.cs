using UnityEngine;

public class TakeIngredient : MonoBehaviour
{
    public void BeingGrab(GameObject handPosition)
    {
        // Optional: disable physics so the object stays in hand
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Ignore collision with player layer
        GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Player");

        // Snap to hand position & set as child
        transform.SetParent(handPosition.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    
    public void Drop()
    {
        // Unparent the object
        transform.SetParent(null);

        // Re-enable physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Reset collision with the player
        GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Nothing");
        
    }

    
}
