using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    [Header("Cutting Settings")]
    public float cutSpeed = 100f; // Speed at which cutting progresses
    public Vector3 cutIngredientScale = new Vector3(1.3f, 1.6f, 1.3f); // Scale after cutting
    public float popUpForce = 2f; // Force applied when cutting is complete
    public float positionAdjustment = 0.1f; // Vertical adjustment after cutting

    [Header("References")]
    public Animator knifeAnimator;
    public IngredientInstance ingredientOnBoard;

    [Header("State")]
    [SerializeField] private float progress;
    [SerializeField] private bool isReadyToCut = false;

    private void Awake()
    {
        progress = 0;
    }

    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (ingredient == null || ingredient.currentState != "Crudo") return false;

        ingredientOnBoard = ingredient;
        isReadyToCut = true;

        // Prepare ingredient for cutting
        ingredient.transform.SetParent(transform);
        ingredient.transform.localPosition = Vector3.up * 0.2f;
        ingredient.transform.localRotation = Quaternion.identity;

        // Disable physics while on board
        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = ingredient.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        return true;
    }

    public void ProcessCutting()
    {
        if (!isReadyToCut || ingredientOnBoard == null) return;

        progress += cutSpeed * Time.deltaTime;
        PlayCutAnimation();

        if (progress >= 100)
        {
            CompleteCut();
        }
    }

    private void CompleteCut()
    {
        if (ingredientOnBoard == null) return;

        // Update visual mesh
        MeshFilter filter = ingredientOnBoard.GetComponent<MeshFilter>();
        if (filter != null && ingredientOnBoard.cutMesh != null)
        {
            filter.mesh = ingredientOnBoard.cutMesh;
        }

        // Update collider
        MeshCollider collider = ingredientOnBoard.GetComponent<MeshCollider>();
        if (collider != null && ingredientOnBoard.cutMesh != null)
        {
            collider.sharedMesh = ingredientOnBoard.cutMesh;
            collider.convex = true;
            collider.enabled = true;
        }

        // Adjust scale
        ingredientOnBoard.transform.localScale = Vector3.one; // Reset first
        ingredientOnBoard.transform.localScale = cutIngredientScale;

        // Refresh collider
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }

        // Enable physics
        Rigidbody rb = ingredientOnBoard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(Vector3.up * popUpForce, ForceMode.Impulse);
        }

        // Update ingredient state
        ingredientOnBoard.currentState = "Cortado";
        ingredientOnBoard.canBeCut = false;
        ingredientOnBoard.canBePickedUp = true;

        // Adjust position for better interaction
        ingredientOnBoard.transform.position += Vector3.up * positionAdjustment;

        // Reset board state
        ingredientOnBoard.transform.SetParent(null);
        ingredientOnBoard = null;
        progress = 0;
        isReadyToCut = false;
    }

    private void PlayCutAnimation()
    {
        if (knifeAnimator != null)
        {
            knifeAnimator.SetTrigger("Cut");
        }
    }

    // Visual feedback for when the player looks at the board
    public void ShowCuttingProgress()
    {
        // You could implement a UI progress bar here if needed
    }

    // Called when the player stops cutting
    public void InterruptCutting()
    {
        // Could add partial cutting effects here
    }
}