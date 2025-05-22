using UnityEngine;

public class OutlineController : MonoBehaviour
{
    public Material outlineMaterial;

    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;
    private bool outlineActive = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterials = meshRenderer.materials;
    }

    public void SetOutline(bool enabled)
    {
        if (meshRenderer == null || outlineMaterial == null) return;

        if (enabled && !outlineActive)
        {
            originalMaterials = meshRenderer.materials; // Guarda los materiales actuales antes de aplicar el outline

            // Agrega el material de outline al inicio
            Material[] newMats = new Material[originalMaterials.Length + 1];
            newMats[0] = outlineMaterial;
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                newMats[i + 1] = originalMaterials[i];
            }
            meshRenderer.materials = newMats;
            outlineActive = true;
        }
        else if (!enabled && outlineActive)
        {
            meshRenderer.materials = originalMaterials;
            outlineActive = false;
        }
    }
}
