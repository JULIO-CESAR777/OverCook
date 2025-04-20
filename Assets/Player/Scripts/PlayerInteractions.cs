using System;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public GameObject textInteractions;
    public float rayDistance = 3f;
    public string interactTag = "Interactable";

    private Camera mainCamera;

    private void Start()
    {
        textInteractions = GameObject.Find("InteractText");
        if (textInteractions != null)
        {
            textInteractions.SetActive(false);
        }

        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInteractionRaycast();
    }

    private void HandleInteractionRaycast()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, 0.3f, out hit, rayDistance)) // ← esto amplía el rango
        {
            if (hit.collider.CompareTag(interactTag))
            {
                textInteractions.SetActive(true);
                return;
            }
        }

        textInteractions.SetActive(false);
    }
}
