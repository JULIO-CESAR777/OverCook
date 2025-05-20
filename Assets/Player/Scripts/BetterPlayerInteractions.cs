using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class InteractionTagSettings
{
    public string ingredientBoxTag = "Spawner";
    public string ingredientTag = "Ingredient";
    public string plateTag = "Plate";
    public string cuttingBoardTag = "CuttingBoard";
    public string recipeTag = "Recipe";
    public string customerTag = "Customer";
    public string plateSpawnerTag = "SpawnerPlates";
    public string panTag = "Pan";
    public string stoveTag = "Stove";
}

public class BetterPlayerInteractions : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionReference interactAction;
    public InputActionReference secondaryAction;

    [Header("Interaction Settings")]
    public InteractionTagSettings tags;
    public float interactionDistance = 3f;
    public float sphereCastRadius = 0.3f;
    public float closeCheckRadius = 0.5f;
    public float minValidAngle = 45f;
    public float minInteractionScore = 0.25f;

    [Header("UI References")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;
    public GameObject handPosition;

    [Header("Debug")]
    public bool debugMode = false;
    public Color debugRayColor = Color.green;

    // State variables
    public GameObject currentInteractable;
    private GameObject heldObject;
    private bool isHoldingObject;
    private Camera playerCamera;
    private InteractionType currentInteractionType;

    private enum InteractionType
    {
        None,
        Grab,
        Place,
        Use,
        Give,
        Cook
    }

    private void Awake()
    {
        playerCamera = Camera.main;
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }

        if (secondaryAction != null)
        {
            secondaryAction.action.performed += OnSecondaryAction;
            secondaryAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        if (secondaryAction != null)
        {
            secondaryAction.action.performed -= OnSecondaryAction;
            secondaryAction.action.Disable();
        }
    }

    private void Update()
    {
        FindBestInteractable();
        HandleContinuousInteractions();
    }

    #region Interaction Detection
    private void FindBestInteractable()
    {
        GameObject bestTarget = null;
        float bestScore = 0f;

        // Check for direct raycast hits first
        RaycastHit[] hits = Physics.SphereCastAll(
            playerCamera.transform.position,
            sphereCastRadius,
            playerCamera.transform.forward,
            interactionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform)) continue;

            float score = CalculateInteractionScore(hit);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = GetRootInteractable(hit.collider.gameObject);
            }
        }

        // Fallback to close proximity check if nothing good found
        if (bestScore < minInteractionScore * 0.5f)
        {
            Collider[] closeColliders = Physics.OverlapSphere(
                playerCamera.transform.position,
                closeCheckRadius,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (var collider in closeColliders)
            {
                if (collider.transform.IsChildOf(transform)) continue;

                Vector3 direction = (collider.transform.position - playerCamera.transform.position).normalized;
                float angle = Vector3.Angle(playerCamera.transform.forward, direction);

                if (angle < minValidAngle)
                {
                    float distance = Vector3.Distance(playerCamera.transform.position, collider.transform.position);
                    float score = (1f - angle / minValidAngle) * (1f - Mathf.Clamp01(distance / closeCheckRadius));

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = GetRootInteractable(collider.gameObject);
                    }
                }
            }
        }

        // Update current interactable if we found something valid
        if (bestScore > minInteractionScore)
        {
            if (currentInteractable != bestTarget)
            {
                currentInteractable = bestTarget;
                UpdateInteractionUI();
            }
        }
        else
        {
            currentInteractable = null;
            interactionUI.SetActive(false);
        }

        if (debugMode) Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, debugRayColor);
    }

    private GameObject GetRootInteractable(GameObject obj)
    {
        Transform root = obj.transform;
        while (root.parent != null && !root.parent.IsChildOf(transform))
        {
            root = root.parent;
        }
        return root.gameObject;
    }

    private float CalculateInteractionScore(RaycastHit hit)
    {
        // Angle score (how centered the object is in view)
        Vector3 directionToTarget = (hit.point - playerCamera.transform.position).normalized;
        float angleScore = 1f - Vector3.Angle(playerCamera.transform.forward, directionToTarget) / 180f;

        // Distance score (closer is better)
        float distanceScore = 1f - Mathf.Clamp01(hit.distance / interactionDistance);

        // Screen position score (center of screen is better)
        Vector3 screenPoint = playerCamera.WorldToViewportPoint(hit.point);
        float screenScore = 1f - (Mathf.Abs(screenPoint.x - 0.5f) + Mathf.Abs(screenPoint.y - 0.5f));

        // Special bonuses for certain objects
        float bonusScore = 0f;

        // Bonus for ingredients that can be interacted with
        IngredientInstance ingredient = hit.collider.GetComponent<IngredientInstance>();
        if (ingredient != null)
        {
            if (ingredient.currentState == "Cortado") bonusScore += 0.2f;
            if (ingredient.canBeCut) bonusScore += 0.1f;
        }

        // Weighted final score
        return angleScore * 0.5f + distanceScore * 0.3f + screenScore * 0.2f + bonusScore;
    }
    #endregion

    #region Interaction Handling
    private void UpdateInteractionUI()
    {
        if (currentInteractable == null || interactionUI == null) return;

        interactionUI.SetActive(true);
        currentInteractionType = InteractionType.None;

        // Check interaction types and set appropriate UI text
        if (!isHoldingObject)
        {
            if (currentInteractable.CompareTag("Environment"))
            {

                interactionText.text = "";
               
            }
            
            if (currentInteractable.CompareTag(tags.ingredientBoxTag))
            {
                interactionText.text = "Press [F] to spawn ingredient";
                currentInteractionType = InteractionType.Use;
            }
            else if (currentInteractable.CompareTag(tags.ingredientTag))
            {
                interactionText.text = "Press [F] to grab";
                currentInteractionType = InteractionType.Grab;
            }
            else if (currentInteractable.CompareTag(tags.plateTag))
            {
                interactionText.text = "Press [F] to take plate";
                currentInteractionType = InteractionType.Grab;
            }
            else if (currentInteractable.CompareTag(tags.plateSpawnerTag))
            {
                interactionText.text = "Press [F] to spawn ";
                currentInteractionType = InteractionType.Use;
            }
            else if (currentInteractable.CompareTag(tags.panTag))
            {
                interactionText.text = "Press [F] to grab pan";
                currentInteractionType = InteractionType.Grab;
            }
            else if (currentInteractable.CompareTag(tags.recipeTag))
            {
                interactionText.text = "Press [F] to grab ";
                currentInteractionType = InteractionType.Grab;
            }
        }
        else // When holding an object
        {
            if (currentInteractable.CompareTag(tags.plateTag) && heldObject.CompareTag(tags.ingredientTag))
            {
                interactionText.text = "Press [F] to place on plate";
                currentInteractionType = InteractionType.Place;
            }
            else if (currentInteractable.CompareTag(tags.cuttingBoardTag) && heldObject.CompareTag(tags.ingredientTag))
            {
                IngredientInstance ingredient = heldObject.GetComponent<IngredientInstance>();
                if (ingredient != null && ingredient.canBeCut)
                {
                    interactionText.text = "Press [F] to place on board";
                    currentInteractionType = InteractionType.Place;
                }
            }
            else if (currentInteractable.CompareTag(tags.panTag) && heldObject.CompareTag(tags.ingredientTag))
            {
                interactionText.text = "Press [F] to place in pan";
                currentInteractionType = InteractionType.Place;
            }
            else if (currentInteractable.CompareTag(tags.stoveTag) && heldObject.CompareTag(tags.panTag))
            {
                interactionText.text = "Press [F] to cook";
                currentInteractionType = InteractionType.Cook;
            }
            else if (currentInteractable.CompareTag(tags.customerTag) && heldObject.CompareTag(tags.recipeTag))
            {
                interactionText.text = "Press [F] to serve";
                currentInteractionType = InteractionType.Give;
            }
        }

        // Special case for cutting board with ingredient
        if (currentInteractable.CompareTag(tags.cuttingBoardTag))
        {
            Debug.Log("Watcheando tabla con ingrediente");
            CuttingBoard board = currentInteractable.GetComponent<CuttingBoard>();
            if (board != null && board.ingredientOnBoard != null && !isHoldingObject)
            {
                interactionText.text = "Hold [F] to cut";
                currentInteractionType = InteractionType.Use;
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable == null) return;

        switch (currentInteractionType)
        {
            case InteractionType.Grab:
                GrabObject(currentInteractable);
                break;

            case InteractionType.Place:
                PlaceHeldObject(currentInteractable);
                break;

            case InteractionType.Use:
                UseInteractable(currentInteractable);
                break;

            case InteractionType.Give:
                GiveOrder();
                break;

            case InteractionType.Cook:
                StartCooking();
                break;
        }
    }

    private void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (isHoldingObject)
        {
            DropObject();
        }
    }

    private void HandleContinuousInteractions()
    {
        if (currentInteractable != null && currentInteractable.CompareTag(tags.cuttingBoardTag))
        {
            CuttingBoard board = currentInteractable.GetComponent<CuttingBoard>();
            if (board != null && board.ingredientOnBoard != null &&
                interactAction.action.IsPressed() && !isHoldingObject)
            {
                board.ProcessCutting();
            }
        }
    }
    #endregion

    #region Object Manipulation
    private void GrabObject(GameObject objToGrab)
    {
        if (isHoldingObject) return;

        StartCoroutine(MoveToHandCoroutine(objToGrab));
    }

    private IEnumerator MoveToHandCoroutine(GameObject obj)
    {
        isHoldingObject = true;
        float duration = 0.25f;
        float elapsed = 0f;

        Transform originalParent = obj.transform.parent;
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;

        // Configurar como hijo temporalmente para rotaciones consistentes
        obj.transform.SetParent(handPosition.transform, true);

        // Posición/rotación objetivo en espacio local
        Vector3 targetLocalPos = Vector3.zero;
        Quaternion targetLocalRot = GetTargetRotation(obj);

        // Disable physics during movement
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Collider col = obj.GetComponent<Collider>();
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            obj.transform.localPosition = Vector3.Lerp(
                handPosition.transform.InverseTransformPoint(startPos),
                targetLocalPos,
                t
            );

            obj.transform.localRotation = Quaternion.Slerp(
                Quaternion.Inverse(handPosition.transform.rotation) * startRot,
                targetLocalRot,
                t
            );

            yield return null;
        }

        // Asegurar posición/rotación final exacta
        obj.transform.localPosition = targetLocalPos;
        obj.transform.localRotation = targetLocalRot;

        heldObject = obj;
        interactionUI.SetActive(false);
    }

    private Quaternion GetTargetRotation(GameObject obj)
    {
        Quaternion baseRotation = Quaternion.identity;

        if (obj.CompareTag(tags.panTag))
        {
            return Quaternion.Euler(0f, 180f, 0f); // Rotación específica para sartenes
        }
        else if (obj.CompareTag(tags.ingredientTag))
        {
            return Quaternion.Euler(-90f, 0f, 0f); // Ejemplo para ingredientes
        }

        return baseRotation;
    }

    private void DropObject()
    {
        if (!isHoldingObject || heldObject == null) return;

        heldObject.transform.SetParent(null);

        // Enable physics
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        Collider col = heldObject.GetComponent<Collider>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(playerCamera.transform.forward * 2f, ForceMode.Impulse);
        }
        if (col != null) col.enabled = true;

        heldObject = null;
        isHoldingObject = false;
    }

    private void PlaceHeldObject(GameObject target)
    {
        if (!isHoldingObject || heldObject == null) return;

        bool placementSuccessful = false;

        if (target.CompareTag(tags.plateTag))
        {
            PlateController plate = target.GetComponent<PlateController>();
            IngredientInstance ingredient = heldObject.GetComponent<IngredientInstance>();
            if (plate != null && ingredient != null)
            {
                placementSuccessful = plate.TryAddIngredient(ingredient);
            }
        }
        else if (target.CompareTag(tags.cuttingBoardTag))
        {
            CuttingBoard board = target.GetComponent<CuttingBoard>();
            IngredientInstance ingredient = heldObject.GetComponent<IngredientInstance>();
            if (board != null && ingredient != null && ingredient.canBeCut)
            {
                placementSuccessful = board.TryAddIngredient(ingredient);
            }
        }
        else if (target.CompareTag(tags.panTag))
        {
            Sarten pan = target.GetComponent<Sarten>();
            IngredientInstance ingredient = heldObject.GetComponent<IngredientInstance>();
            if (pan != null && ingredient != null)
            {
                placementSuccessful = pan.TryAddIngredient(heldObject, ingredient);
            }
        }

        if (placementSuccessful)
        {
            heldObject = null;
            isHoldingObject = false;
            interactionUI.SetActive(false);
        }
    }
    #endregion

    #region Special Interactions
    private void UseInteractable(GameObject interactable)
    {
        Debug.Log(interactable);
        if (interactable.CompareTag(tags.ingredientBoxTag))
        {
            IngredientSpawner spawner = interactable.GetComponent<IngredientSpawner>();
            spawner?.SpawnIngredient();
        }
        else if (interactable.CompareTag(tags.plateSpawnerTag))
        {
            Debug.Log("Entranding Spawner");
            SpawnerPlates spawner = interactable.GetComponent<SpawnerPlates>();
            spawner?.SpawnPlate();
        }
    }

    private void GiveOrder()
    {
        if (!isHoldingObject || heldObject == null || currentInteractable == null) return;
        if (!heldObject.CompareTag(tags.recipeTag) || !currentInteractable.CompareTag(tags.customerTag)) return;

        Customer customer = currentInteractable.GetComponent<Customer>();
        if (customer != null && customer.ServeOrder(heldObject))
        {
            Destroy(heldObject);
            heldObject = null;
            isHoldingObject = false;
            interactionUI.SetActive(false);
        }
    }

    private void StartCooking()
    {
        if (!isHoldingObject || heldObject == null || currentInteractable == null) return;
        if (!heldObject.CompareTag(tags.panTag) || !currentInteractable.CompareTag(tags.stoveTag)) return;

        stove stoveScript = currentInteractable.GetComponent<stove>();
        if (stoveScript != null)
        {
            stoveScript.AddPanToStove(heldObject);
            heldObject = null;
            isHoldingObject = false;
            interactionUI.SetActive(false);
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (!debugMode) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerCamera.transform.position, closeCheckRadius);

        if (currentInteractable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerCamera.transform.position, currentInteractable.transform.position);
        }
    }
    #endregion
}