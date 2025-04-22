using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference interactAction;

    [Header("Interaction Settings")]
    public List<string> interactTag;
    public float rayDistance = 3f;
    public GameObject textInteractions;
    public TextMeshProUGUI interfaceText;

    public bool isDoingAnAction;

    [Header("References")]
    public Camera mainCamera;

    private GameObject currentInteractable;
    public GameObject grabObject;

    public GameObject handPosition;

    private void OnEnable()
    {
        interactAction.action.performed += OnInteract;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();
    }

    private void Start()
    {
        textInteractions = GameObject.Find("InteractText");
        interfaceText = GameObject.Find("InteractText").GetComponent<TextMeshProUGUI>();
        handPosition = GameObject.Find("HandHolder");
        
        if (textInteractions != null)
        {
            textInteractions.SetActive(false);
        }

        mainCamera = Camera.main;

        isDoingAnAction = false;
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
            if (hit.collider.CompareTag(interactTag[0]) && !isDoingAnAction)
            {
                currentInteractable = hit.collider.gameObject;
                interfaceText.text = "Press F to interact";
                textInteractions.SetActive(true);

                return;
            }

            if(hit.collider.CompareTag(interactTag[1]) && !isDoingAnAction){
                currentInteractable = hit.collider.gameObject;
                interfaceText.text = "Press F to grab";
                textInteractions.SetActive(true);
                return;
            }
        }

        currentInteractable = null;
        textInteractions.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        //Interacciones cuando un objeto es detectado enfrente
        if (currentInteractable != null)
        {
            //Accion cuando es una caja de ingredientes
            if(currentInteractable.CompareTag(interactTag[0])){
                if(isDoingAnAction) return;

                var interactable = currentInteractable.GetComponent<IngredientSpawner>();
                interactable?.SpawnIngredient();
            }
            //Accion cuando es un ingrediente
            if(currentInteractable.CompareTag(interactTag[1])){
                isDoingAnAction = true;
                //Grab Ingredient
                var interactable = currentInteractable.GetComponent<TakeIngredient>();
                grabObject = interactable.gameObject;
                interactable?.BeingGrab(handPosition);
                
                currentInteractable = null;
            }
        }
        //Comprobacion cuando se quiere soltar un objeto
        else if(currentInteractable == null && isDoingAnAction )
        {
            grabObject.GetComponent<TakeIngredient>().Drop();
            grabObject = null;
            isDoingAnAction = false;
        }
    }
}
