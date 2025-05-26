using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            Vector3 targetPosition = Camera.main.transform.position;

            // Mantener la misma altura (Y) que el objeto, para que solo gire en Y
            targetPosition.y = transform.position.y;

            transform.LookAt(targetPosition);

            transform.Rotate(0, 180f, 0); // Corregir dirección si se ve al revés
        }
    }
}
