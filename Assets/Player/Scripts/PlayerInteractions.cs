using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class DebugExtension
{
    public static void DebugWireSphere(Vector3 position, Color color, float radius = 1.0f)
    {
        float angle = 10.0f;
        Vector3 lastPoint = Vector3.zero;
        Vector3 nextPoint = Vector3.zero;
        for (int i = 0; i <= 36; i++)
        {
            float theta = i * angle * Mathf.Deg2Rad;

            // Draw circle on XZ plane
            lastPoint = new Vector3(Mathf.Cos(theta - angle * Mathf.Deg2Rad), 0, Mathf.Sin(theta - angle * Mathf.Deg2Rad)) * radius + position;
            nextPoint = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * radius + position;
            Debug.DrawLine(lastPoint, nextPoint, color);

            // Draw circle on XY plane
            lastPoint = new Vector3(Mathf.Cos(theta - angle * Mathf.Deg2Rad), Mathf.Sin(theta - angle * Mathf.Deg2Rad), 0) * radius + position;
            nextPoint = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0) * radius + position;
            Debug.DrawLine(lastPoint, nextPoint, color);

            // Draw circle on YZ plane
            lastPoint = new Vector3(0, Mathf.Cos(theta - angle * Mathf.Deg2Rad), Mathf.Sin(theta - angle * Mathf.Deg2Rad)) * radius + position;
            nextPoint = new Vector3(0, Mathf.Cos(theta), Mathf.Sin(theta)) * radius + position;
            Debug.DrawLine(lastPoint, nextPoint, color);
        }
    }
}


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
        if (textInteractions != null)
        {
            interfaceText = textInteractions.GetComponent<TextMeshProUGUI>();
            textInteractions.SetActive(false);
        }
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

        // Detectar si está cortando
        if (interactAction.action.IsPressed() && currentInteractable != null && currentInteractable.CompareTag(interactTag[3]))
        {
            CuttingBoard cuttingBoard = currentInteractable.GetComponent<CuttingBoard>();
            if (cuttingBoard != null && cuttingBoard.ingredientOnBoard != null)
            {
                
                cuttingBoard.cutIngredient(this);
            }
        }
    }

    private GameObject GetBestInteractable()
    {
        // Opción 1: Chequear objetos en el rayo principal
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.3f, rayDistance);

        // Opción 2: Chequear objetos cercanos con OverlapSphere
        Collider[] closeColliders = Physics.OverlapSphere(mainCamera.transform.position, 0.5f);

        // Convertir todos los resultados a GameObjects
        List<GameObject> potentialTargets = new List<GameObject>();

        // Añadir objetos del SphereCast
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(this.transform))
            {
                potentialTargets.Add(hit.collider.gameObject);
            }
        }

        // Añadir objetos del OverlapSphere
        foreach (Collider col in closeColliders)
        {
            if (!col.transform.IsChildOf(this.transform) && !potentialTargets.Contains(col.gameObject))
            {
                potentialTargets.Add(col.gameObject);
            }
        }

        if (potentialTargets.Count == 0)
            return null;

        // Encontrar el objeto más "frontal"
        GameObject bestTarget = null;
        float smallestAngle = Mathf.Infinity;

        foreach (GameObject target in potentialTargets)
        {
            Vector3 targetDir = (target.transform.position - mainCamera.transform.position).normalized;
            float angle = Vector3.Angle(mainCamera.transform.forward, targetDir);

            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    private void HandleInteractionRaycast()
    {
        GameObject bestTarget = GetBestInteractable();

        if (bestTarget == null)
        {
            currentInteractable = null;
            textInteractions.SetActive(false);
            return;
        }

        // Resto de tu lógica de interacción usando bestTarget
        if (bestTarget.CompareTag(interactTag[0]) && !isDoingAnAction)
        {
            currentInteractable = bestTarget;
            interfaceText.text = "Press F to interact";
            textInteractions.SetActive(true);
            return;
        }


        if ((bestTarget.CompareTag(interactTag[1]) || bestTarget.CompareTag(interactTag[4]) || bestTarget.CompareTag(interactTag[2])) && !isDoingAnAction)
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to grab";
            textInteractions.SetActive(true);
            return;
        }

        if (bestTarget.CompareTag(interactTag[2]) && isDoingAnAction)
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = currentInteractable.GetComponent<PlateController>().recipeCompleted
                ? ""
                : "Press F to place ingredient";
            textInteractions.SetActive(true);
            return;
        }

        if (bestTarget.CompareTag(interactTag[3]))
        {
            if (isDoingAnAction || bestTarget.gameObject.GetComponent<CuttingBoard>().ingredientOnBoard != null)
            {
                currentInteractable = bestTarget.gameObject;
                interfaceText.text = "Press F to cut";
                textInteractions.SetActive(true);
                return;
            }
        }

        if (bestTarget.CompareTag(interactTag[5]) && isDoingAnAction && grabObject.CompareTag("Recipe"))
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to give";
            textInteractions.SetActive(true);
            return;
        }

        if (bestTarget.CompareTag(interactTag[6]) && !isDoingAnAction)
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to interact";
            textInteractions.SetActive(true);
            return;
        }

        // Si nada coincide
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
            if(currentInteractable.CompareTag(interactTag[1]) && !isDoingAnAction)
            {
                //Grab Ingredient
                grabbingObject(currentInteractable);
            }

            //Accion cuando es un plato JULIOOOO
            if (currentInteractable.CompareTag(interactTag[2]) && !isDoingAnAction)
            {
                //Grab Ingredient
                grabbingPlate(currentInteractable);
            }

            // Acción cuando es un plato y el jugador tiene un ingrediente en mano
            if (currentInteractable!=null && currentInteractable.CompareTag(interactTag[2]) && isDoingAnAction)
            {
                PlaceIngredientOnPlate(currentInteractable);
            }
            
            // Acción en una tabla de cortar
            if (currentInteractable != null && currentInteractable.CompareTag(interactTag[3]))
            {
                var cuttingBoard = currentInteractable.GetComponent<CuttingBoard>();

                if (isDoingAnAction && cuttingBoard != null && cuttingBoard.ingredientOnBoard == null)
                {
                    // Si estamos agarrando algo Y no hay ingrediente en la tabla → colocar ingrediente
                    PlaceIngredientOnCuttingBoard(currentInteractable);
                }
                
            }

            if(currentInteractable != null && currentInteractable.CompareTag(interactTag[4]) && !isDoingAnAction)
            {

                grabbingPlate(currentInteractable);
                currentInteractable = null;
            }

            if(currentInteractable != null && currentInteractable.CompareTag(interactTag[5]) && grabObject.CompareTag("Recipe") && isDoingAnAction)
            {
                giveOrder();
                currentInteractable = null;


            }

            if (currentInteractable != null && currentInteractable.CompareTag(interactTag[6]))
            {
                if (isDoingAnAction) return;
                var interactable = currentInteractable.GetComponent<SpawnerPlates>();
                interactable?.SpawnPlate();
            }

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

        IngredientInstance ingredient = grabbedObject.GetComponent<IngredientInstance>();
        if (ingredient != null && !ingredient.canBePickedUp)
        {
            // Todavía no se puede recoger
            return;
        }
        

        StartCoroutine(MoveToHand(grabbedObject));
        
    }

    public void grabbingPlate(GameObject grabbedObject)
    {
        if (grabObject != null) return;
       

        StartCoroutine(MoveToHand(grabbedObject));

    }
    private IEnumerator MoveToHand(GameObject obj)
    {

        isDoingAnAction = true;
        float duration = 0.25f; // Tiempo de la animación
        float elapsed = 0f;

        Vector3 startPosition = obj.transform.position;
        Quaternion startRotation = obj.transform.rotation;

        Vector3 targetPosition = handPosition.transform.position;
        Quaternion targetRotation = handPosition.transform.rotation;

        Vector3 originalScale = obj.transform.localScale; // Guardar la escala original

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            obj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
        
            // Asegurarse de que la escala no cambie
            obj.transform.localScale = originalScale;

            yield return null;
        }

        // Asegurarse de que el objeto conserve la escala original después de moverlo
        obj.transform.localScale = originalScale;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Desactiva la física mientras se mueve
            rb.detectCollisions = false;
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // Evita colisiones mientras se mueve
        }

        // Final del movimiento
        obj.transform.SetParent(handPosition.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        grabObject = obj;
        currentInteractable = null;
    }

    public void droppingObject(GameObject grabbedObject)
    {
        if (grabObject == null) return;

        isDoingAnAction = false;
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
        
    }

    private void PlaceIngredientOnPlate(GameObject plate)
    {
        if (grabObject == null) return;

        // Obtener el IngredientInstance desde el objeto
        IngredientInstance ingredientInstance = grabObject.GetComponent<IngredientInstance>();
        if (ingredientInstance == null) return;

        // Verificar el estado del ingrediente antes de agregarlo
        //Debug.Log("Intentando agregar: " + ingredientInstance.ingredientData.ingredientName + " Estado: " + ingredientInstance.currentState);
        
        // Intentar añadirlo al plato, pasando el estado actual
        PlateController plateController = plate.GetComponent<PlateController>();
        if (plateController == null || !plateController.TryAddIngredient(ingredientInstance))
        {
            return;
        }

        grabObject = null;

        // Restablecer el estado de la acción después de colocar el ingrediente en el plato
        isDoingAnAction = false;

        // Verifica si la receta está completa
        plateController.CheckRecipeCompletion(); // Esto ya lo tienes para validar si el plato se completó
    }
    

    public void PlaceIngredientOnCuttingBoard(GameObject cuttingBoard){
        if (grabObject == null) return;

        // Obtener el IngredientSO desde el objeto
        IngredientInstance ingredientInstance = grabObject.GetComponent<IngredientInstance>();
        if (ingredientInstance == null) return;

        // Intentar añadirlo a la tabla
        CuttingBoard cuttingboard = cuttingBoard.GetComponent<CuttingBoard>();
        if(cuttingboard == null || !cuttingboard.TryAddIngredient(ingredientInstance)) return;
        
        // Si pasó la validación, colocarlo sobre la tabla
        grabObject.transform.SetParent(null);
        Vector3 top = cuttingBoard.transform.position + Vector3.up * 0.2f;
        grabObject.transform.position = top;
        grabObject.transform.rotation = cuttingBoard.transform.rotation;

        Collider col = grabObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        grabObject.transform.SetParent(cuttingBoard.transform); // para que se quede con el plato
        grabObject = null;
        ingredientInstance.canBePickedUp = false;

        // Restablecer el estado de la acción después de colocar el ingrediente en el plato
        isDoingAnAction = false;
         
    }

    //Julio

    public void giveOrder()
    {
        if (grabObject == null && currentInteractable == null) return;

        bool flag;

        

        Customer cliente = currentInteractable.GetComponent<Customer>();

        flag = cliente.ServeOrder(grabObject);

        if (flag)
        {

            grabObject.transform.SetParent(null);
            Destroy(grabObject.gameObject);
            grabObject = null;
           
            isDoingAnAction = false;
          
            Debug.Log("Borrado objetos");
            
        }

    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        // Esfera de alcance lejano (SphereCast)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * rayDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mainCamera.transform.position + mainCamera.transform.forward * rayDistance, 0.3f);
        
        // Esfera de alcance cercano (OverlapSphere)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(mainCamera.transform.position, 0.5f);
    }



}
