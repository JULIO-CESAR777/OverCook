using UnityEngine;

public class CortarIngrediente : MonoBehaviour
{


    TablaCortar tablacortar;
    public ParticleSystem particulasCortar;

    private void Start()
    {
        tablacortar = GetComponentInParent<TablaCortar>();
    }
    public void cutIngredient( )
    {
        
       
        if (tablacortar.ingredientOnBoard == null) return;

        ParticleSystem pscortar = Instantiate(particulasCortar, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
        pscortar.Play();
        Destroy(pscortar.gameObject, pscortar.main.duration);

        tablacortar.progress += 30f;
        Debug.Log("Progreso de corte: " + tablacortar.progress);
        if (tablacortar.progress >= 90)
        {

            CompleteCut();
        }
    }

    private void CompleteCut( )
    {
        if (tablacortar.ingredientOnBoard == null) return;

        // Cambiar el estado del ingrediente a "Cortado"
        tablacortar.ingredientOnBoard.currentState = "Cortado";

        // Cambiar el MeshRenderer y MeshFilter para mostrar la versión cortada
        MeshRenderer renderer = tablacortar.ingredientOnBoard.GetComponent<MeshRenderer>();
        if (renderer != null && tablacortar.ingredientOnBoard.cutMesh != null)
        {
            // Aplicar la nueva malla (si está disponible)
            MeshFilter filter = tablacortar.ingredientOnBoard.GetComponent<MeshFilter>();
            if (filter != null)
            {
                filter.mesh = tablacortar.ingredientOnBoard.cutMesh;
            }

        }

        tablacortar.ingredientOnBoard.transform.localScale = new Vector3(1.3f, 1.6f, 1.3f);

        MeshCollider collider = tablacortar.ingredientOnBoard.GetComponent<MeshCollider>();
        if (collider != null && tablacortar.ingredientOnBoard.cutMesh != null)
        {
            // Actualizar el MeshCollider
            collider.sharedMesh = tablacortar.ingredientOnBoard.cutMesh;
            collider.enabled = true;
            collider.providesContacts = true;
        }

        tablacortar.ingredientOnBoard.transform.SetParent(null);


        // Aplicar fuerza para que salte un poquito (opcional)
        Rigidbody rb = tablacortar.ingredientOnBoard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }

        // IMPORTANTE: Bloqueamos recoger temporalmente

        // 4. Limpiar referencia
        tablacortar.ingredientOnBoard = null;
        tablacortar.progress = 0;
        tablacortar.readyToCut = false;
      
        Debug.Log("Se cambio la accion en la tabla");


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
 
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            cutIngredient();
        }
    }
}
