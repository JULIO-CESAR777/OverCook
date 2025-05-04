using UnityEngine;
using TMPro;

public class FloatingPoints : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float duration = 1.5f;
    public Vector3 floatDirection = Vector3.up;

    private TextMeshPro textMesh;

    private void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
    }

    public void SetPoints(int points)
    {
        if (textMesh != null)
        {
            textMesh.text = "+" + points.ToString();
        }
    }
}
