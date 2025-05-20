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
        if(interactAction != null){
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if(interactAction != null){
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
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
            if (cuttingBoard != null && cuttingBoard.ingredientOnBoard != null && cuttingBoard.ingredientOnBoard.canBeCut)
            {
                
                cuttingBoard.cutIngredient(this);
            }
        }
    }

    #region Obtencion de interactuables
    private GameObject GetBestInteractable()
    {
        const float sphereCastRadius = 0.3f;
        const float closeCheckRadius = 0.5f;
        const float maxDistance = 3f;
        const float minValidAngle = 45f;

        Transform cameraTransform = mainCamera.transform;
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraForward = cameraTransform.forward;

        // Buffers reutilizables
        RaycastHit[] hitBuffer = new RaycastHit[8];
        Collider[] colliderBuffer = new Collider[8];

        GameObject bestTarget = null;
        float bestScore = 0f;

        // 1. SphereCast (detección frontal)
        int hitCount = Physics.SphereCastNonAlloc(
            cameraPosition,
            sphereCastRadius,
            cameraForward,
            hitBuffer,
            maxDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            // Buscar específicamente la tabla de cortar
            CuttingBoard cuttingBoard = hitBuffer[i].collider.GetComponentInParent<CuttingBoard>();
            if (cuttingBoard != null && !cuttingBoard.transform.IsChildOf(transform))
            {
                float score = CalculateInteractableScore(hitBuffer[i], cameraPosition, cameraForward) + 0.5f; // Bonus
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = cuttingBoard.gameObject;
                    continue;
                }
            }

            // Lógica normal para otros objetos
            if (!hitBuffer[i].collider.transform.IsChildOf(transform))
            {
                Transform parent = hitBuffer[i].collider.transform;
                while (parent.parent != null && !parent.parent.IsChildOf(transform))
                {
                    parent = parent.parent;
                }

                float currentScore = CalculateInteractableScore(hitBuffer[i], cameraPosition, cameraForward);
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestTarget = parent.gameObject;
                }
            }
        }

        // 2. OverlapSphere (detección cercana)
        if (bestScore < 0.5f)
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(
                cameraPosition,
                closeCheckRadius,
                colliderBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < colliderCount; i++)
            {
                if (!colliderBuffer[i].transform.IsChildOf(transform))
                {
                    Vector3 direction = (colliderBuffer[i].transform.position - cameraPosition).normalized;
                    float angle = Vector3.Angle(cameraForward, direction);

                    if (angle < minValidAngle)
                    {
                        float distance = Vector3.Distance(cameraPosition, colliderBuffer[i].transform.position);
                        float score = (1f - angle / minValidAngle) * (1f - Mathf.Clamp01(distance / closeCheckRadius));

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestTarget = colliderBuffer[i].gameObject;
                        }
                    }
                }
            }
        }

        return bestScore > 0.25f ? bestTarget : null; // Umbral mínimo
    }

    private float CalculateInteractableScore(RaycastHit hit, Vector3 cameraPosition, Vector3 cameraForward)
    {

        // Bonus para ingredientes cortados
        IngredientInstance ingredient = hit.collider.GetComponent<IngredientInstance>();
        if (ingredient != null && ingredient.currentState == "Cortado")
        {
            float baseScore = 0.6f; // Score base alto para ingredientes listos
            float cut_distanceScore = 1f - Mathf.Clamp01(hit.distance / 5f);
            return baseScore + cut_distanceScore * 0.4f;
        }

        // Calculamos ángulo (0-1 donde 1 es mirando directamente)
        Vector3 directionToTarget = (hit.point - cameraPosition).normalized;
        float angleScore = 1f - Vector3.Angle(cameraForward, directionToTarget) / 90f;

        // Calculamos distancia (0-1 donde 1 es más cerca)
        float distanceScore = 1f - Mathf.Clamp01(hit.distance / 5f);

        // Priorizamos objetos más centrados en la pantalla
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(hit.point);
        float screenScore = 1f - (Mathf.Abs(screenPoint.x - 0.5f) + Mathf.Abs(screenPoint.y - 0.5f));

        // Combinamos los scores (ajusta los pesos según necesidad)
        return angleScore * 0.5f + distanceScore * 0.3f + screenScore * 0.2f;
    }
    #endregion

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

        // Agarrar ingrediente, plato, o receta
        if ((bestTarget.CompareTag(interactTag[1]) || bestTarget.CompareTag(interactTag[4]) || bestTarget.CompareTag(interactTag[2])) && !isDoingAnAction)
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to grab";
            textInteractions.SetActive(true);
            return;
        }

        // Colocar un ingrediente
        if (bestTarget.CompareTag(interactTag[2]) && isDoingAnAction)
        {
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = currentInteractable.GetComponent<PlateController>().recipeCompleted
                ? ""
                : "Press F to place ingredient";
            textInteractions.SetActive(true);
            return;
        }

        // Para la tabla de cortar
        if (bestTarget.CompareTag(interactTag[3]))
        {
            currentInteractable = bestTarget.gameObject;

            // Comparaciones cuando se tiene el ingrediente en la mano
            if ((isDoingAnAction && grabObject.CompareTag(interactTag[1]) && grabObject.GetComponent<IngredientInstance>().canBeCut) ||
                (!isDoingAnAction && bestTarget.gameObject.GetComponent<CuttingBoard>().ingredientOnBoard != null && grabObject == null))
            {
            
                interfaceText.text = "Press F to cut";
                textInteractions.SetActive(true);
                return;
            }
            
        }

        if (bestTarget.CompareTag(interactTag[5]) && isDoingAnAction && grabObject.CompareTag(interactTag[4]))
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

        //Detecta la sarten y no tiene nada en la mano
        if(bestTarget.CompareTag(interactTag[7]) && !isDoingAnAction){
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to grab";
            textInteractions.SetActive(true);
            return;
        }

        if(bestTarget.CompareTag(interactTag[7]) && isDoingAnAction && grabObject != null && grabObject.CompareTag(interactTag[1])){
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to put ingredient";
            textInteractions.SetActive(true);
            return;
        }

        if(bestTarget.CompareTag(interactTag[8]) && isDoingAnAction && grabObject != null && grabObject.CompareTag(interactTag[7])){
            currentInteractable = bestTarget.gameObject;
            interfaceText.text = "Press F to cook";
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
                grabbingObject(currentInteractable);
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

                if (isDoingAnAction && cuttingBoard != null && cuttingBoard.ingredientOnBoard == null &&
                    grabObject.GetComponent<IngredientInstance>().canBeCut)
                {
                    // Si estamos agarrando algo Y no hay ingrediente en la tabla → colocar ingrediente
                    PlaceIngredientOnCuttingBoard(currentInteractable);
                }
                
            }

            if(currentInteractable != null && currentInteractable.CompareTag(interactTag[4]) && !isDoingAnAction)
            {
                grabbingObject(currentInteractable);
            }

            if(currentInteractable != null  && grabObject != null && currentInteractable.CompareTag(interactTag[5]) && grabObject.CompareTag(interactTag[4]) && isDoingAnAction)
            {
                giveOrder();
            }

            if (currentInteractable != null && currentInteractable.CompareTag(interactTag[6]))
            {
                if (isDoingAnAction) return;
                var interactable = currentInteractable.GetComponent<SpawnerPlates>();
                interactable?.SpawnPlate();
            }

            // SARTEN
            if (currentInteractable != null && currentInteractable.CompareTag(interactTag[7]) && !isDoingAnAction)
            { 
                grabbingObject(currentInteractable);
            }

            if (currentInteractable != null && grabObject != null && currentInteractable.CompareTag(interactTag[7]) && isDoingAnAction  && grabObject.CompareTag(interactTag[1]))
            {
                PlaceIngredientOnPan();
            }

            // Horno
            if(currentInteractable != null && currentInteractable.CompareTag(interactTag[8]) && isDoingAnAction && grabObject.CompareTag(interactTag[7])){
                stove stoveScript = currentInteractable.GetComponent<stove>();
                if (stoveScript != null)
                {
                    stoveScript.AddPanToStove(grabObject);
                    grabObject = null;
                    isDoingAnAction = false;
                    textInteractions.SetActive(false);
                }
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
        
        // Si estaba sobre un quemador, liberar el slot
        Collider[] overlapped = Physics.OverlapSphere(grabbedObject.transform.position, 0.2f);
        foreach (Collider col in overlapped)
        {
            Burner burner = col.GetComponent<Burner>();
            if (burner != null && burner.currentPan == grabbedObject)
            {
                burner.currentPan = null;
                burner.isOccupied = false;
                Debug.Log("Sartén retirada manualmente del quemador.");
                break;
            }
        }

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

        // Configurar rotación objetivo especial para la sartén
        Quaternion panTargetRotation = handPosition.transform.rotation * Quaternion.Euler(0f, -180f, 0f);

        // Desactivar físicas y colisiones mientras se mueve
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (obj.CompareTag("Pan"))
            {
                obj.transform.rotation = Quaternion.Slerp(startRotation, panTargetRotation, t);
            }
            else
            {
                obj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            }

            obj.transform.localScale = originalScale; // Mantener escala

            yield return null;
        }

        // Finalizar movimiento y parenting
        obj.transform.SetParent(handPosition.transform);
        obj.transform.localPosition = Vector3.zero;

        if (obj.CompareTag("Pan"))
        {
            // Rotación absoluta respecto al padre para evitar errores
            obj.transform.localRotation = Quaternion.Euler(0f, -180f, 0f);
        }
        else
        {
            obj.transform.localRotation = Quaternion.identity;
        }

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

        // Mantener el collider del ingrediente deshabilitado
        Collider ingredientCol = grabObject.GetComponent<Collider>();
        if (ingredientCol != null) ingredientCol.enabled = false;

        // Asegurarse que el collider de la tabla esté habilitado
        Collider boardCol = cuttingBoard.GetComponent<Collider>();
        if (boardCol != null) boardCol.enabled = true;

        // Posicionamiento
        grabObject.transform.SetParent(cuttingBoard.transform);
        grabObject.transform.localPosition = Vector3.up * 0.2f; // Usar localPosition
        grabObject.transform.localRotation = Quaternion.identity;

        grabObject = null;
        ingredientInstance.canBePickedUp = false;

        // Restablecer el estado de la acción después de colocar el ingrediente en el plato
        isDoingAnAction = false;

        currentInteractable = cuttingBoard;
         
    }

    public void PlaceIngredientOnPan()
    {
        IngredientInstance ingredientInstance = grabObject.GetComponent<IngredientInstance>();

        if (grabObject == null || ingredientInstance == null) return;

        Sarten panScript = currentInteractable.GetComponent<Sarten>();
        if (panScript == null) return;

        // Intenta agregarlo al sartén
        if (panScript.TryAddIngredient(grabObject, ingredientInstance))
        {

            grabObject = null;
            isDoingAnAction = false;

            currentInteractable = null;
        }
    }

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

        currentInteractable = null;
    }


    void OnDrawGizmosSelected()
    {
        if (currentInteractable != null && currentInteractable.CompareTag(interactTag[3]))
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(currentInteractable.transform.position, currentInteractable.GetComponent<Collider>().bounds.size);
        }
    }

}
