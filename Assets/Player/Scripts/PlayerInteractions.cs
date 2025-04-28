using System;
using System.Collections;
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

    public GameObject currentInteractable;
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
            
            if (hit.collider.CompareTag(interactTag[2]) && isDoingAnAction)
            {
                currentInteractable = hit.collider.gameObject;

                interfaceText.text = currentInteractable.GetComponent<PlateController>().recipeCompleted
                    ? ""
                    : "Press F to place ingredient";
                
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
                grabbingObject(currentInteractable);
                currentInteractable = null;
            }
            
            // Acción cuando es un plato y el jugador tiene un ingrediente en mano
            if (currentInteractable!=null && currentInteractable.CompareTag(interactTag[2]) && isDoingAnAction)
            {
                PlaceIngredientOnPlate(currentInteractable);
            }
            
            //Agregar el agarre de un plato con una receta completa
            
        }
        //Comprobacion cuando se quiere soltar un objeto
        else if(currentInteractable == null && isDoingAnAction )
        {
            droppingObject(grabObject);
        }
        
    }

    
    public void grabbingObject(GameObject grabbedObject)
    {
        
        if (grabObject != null) return; // Ya tienes algo agarrado

        StartCoroutine(MoveToHand(grabbedObject));
        
    }

    private IEnumerator MoveToHand(GameObject obj)
    {
        float duration = 0.25f; // Tiempo de la animación
        float elapsed = 0f;

        Vector3 startPosition = obj.transform.position;
        Quaternion startRotation = obj.transform.rotation;

        Vector3 targetPosition = handPosition.transform.position;
        Quaternion targetRotation = handPosition.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            obj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Desactiva física
            rb.detectCollisions = false;
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // Evita colisiones molestas mientras se mueve
        }

        // Al final del movimiento
        obj.transform.SetParent(handPosition.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        grabObject = obj;
    }

    public void droppingObject(GameObject grabbedObject)
    {
        if (grabObject == null) return;

        grabObject.transform.SetParent(null); // Lo separa de la mano

        Rigidbody rb = grabObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Vuelve a activar física
            rb.detectCollisions = true;

            // Aplica una pequeña fuerza hacia adelante
            rb.AddForce(mainCamera.transform.forward * 2f, ForceMode.Impulse);
        }

        Collider col = grabObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true; // Reactiva colisiones
        }

        grabObject = null;
        isDoingAnAction = false;
    }

    private void PlaceIngredientOnPlate(GameObject plate)
    {
        if (grabObject == null) return;

        // Obtener el IngredientSO desde el objeto
        IngredientInstance ingredientInstance = grabObject.GetComponent<IngredientInstance>();
        if (ingredientInstance == null) return;

        IngredientSO ingredientSO = ingredientInstance.ingredientData;

        // Intentar añadirlo al plato
        PlateController plateController = plate.GetComponent<PlateController>();
        if (plateController == null || !plateController.TryAddIngredient(ingredientSO))
        {
            return;
        }

        // Si pasó la validación, colocarlo sobre el plato
        grabObject.transform.SetParent(null);
        Vector3 plateTop = plate.transform.position + Vector3.up * 0.1f;
        grabObject.transform.position = plateTop;

        Rigidbody rb = grabObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = grabObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        grabObject.transform.SetParent(plate.transform); // para que se quede con el plato
        grabObject = null;

        // Restablecer el estado de la acción después de colocar el ingrediente en el plato
        isDoingAnAction = false;

        // Verifica si la receta está completa
        plateController.CheckRecipeCompletion(); // Esto ya lo tienes para validar si el plato se completó
    }
    

}
