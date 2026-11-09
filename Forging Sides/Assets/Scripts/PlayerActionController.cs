using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    public Transform playerCamera;
    public Transform holdPoint;

    public float interactionRange = 3f;
    public LayerMask grabbableLayer;
    public LayerMask interactableLayer;

    [Header("Cooldowns")]
    public float interactCooldown = 0.5f;
    private float lastInteractTime = 0f;

    [Header("UI Feedback")]
    public TextMeshProUGUI itemNameText;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;



    public void OnGrab(InputAction.CallbackContext context)
    {
        if (PauseMenuManager.isPaused || CraftingQTE.IsActive) return;

        if (context.started)
        {
            if (heldObject == null) TryGrab();
            else Drop();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (PauseMenuManager.isPaused || CraftingQTE.IsActive) return;
        if (!context.started || heldObject != null) return;
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, interactableLayer))
        {
            ItemDispenser dispenser = hit.collider.GetComponentInParent<ItemDispenser>();
            if (dispenser != null)
            {
                dispenser.DispenseItem();
                return;
            }

            Workbench workbench = hit.collider.GetComponentInParent<Workbench>();
            if (workbench != null)
            {
                if (!workbench.TryCraft() && workbench.HasParts())
                {
              
                    Debug.Log("Those parts don't make anything.");
                }
            }
        }
    }



    private void TryGrab()
    {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, grabbableLayer))
        {
            heldObject = hit.collider.gameObject;
            heldObjectRb = heldObject.GetComponent<Rigidbody>();

            CraftingPart grabbedPart = heldObject.GetComponent<CraftingPart>();
            if (grabbedPart != null) TutorialEvents.Raise(TutorialEventType.ItemPickedUp, grabbedPart.itemData);


            CraftingPart part = heldObject.GetComponent<CraftingPart>();
            if (part != null && part.currentBench != null)
            {
                part.currentBench.RemovePart(part);
            }

            if (heldObjectRb != null)
            {
                heldObjectRb.isKinematic = true;
                heldObject.transform.position = holdPoint.position;
                heldObject.transform.parent = holdPoint;
            }
        }
    }

    private void Drop()
    {

        CraftingPart part = heldObject.GetComponent<CraftingPart>();
        if (part != null &&
            Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit benchHit, interactionRange, interactableLayer))
        {
            Workbench workbench = benchHit.collider.GetComponentInParent<Workbench>();
            if (workbench != null && workbench.TryPlacePart(part))
            {
                heldObject = null;
                heldObjectRb = null;
                return;
            }
        }


        heldObject.transform.parent = null;
        Vector3 rayDirection = holdPoint.position - playerCamera.position;
        float distance = Vector3.Distance(playerCamera.position, holdPoint.position);

        if (Physics.Raycast(playerCamera.position, rayDirection.normalized, out RaycastHit hit, distance, ~grabbableLayer))
        {
            heldObject.transform.position = hit.point - (rayDirection.normalized * 0.1f);
        }

        if (heldObjectRb != null) heldObjectRb.isKinematic = false;

        heldObject = null;
        heldObjectRb = null;
    }



    private void Update()
    {
        if (PauseMenuManager.isPaused) return;
        if (CraftingQTE.IsActive) { itemNameText.text = ""; return; }

        if (heldObject != null)
        {
 
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit holdHit, interactionRange, interactableLayer)
                && holdHit.collider.GetComponentInParent<Workbench>() != null)
            {
                itemNameText.text = "Click to place on Workbench";
            }
            else
            {
                itemNameText.text = "";
            }
            return;
        }

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, grabbableLayer | interactableLayer))
        {
            CraftingPart part = hit.collider.GetComponent<CraftingPart>();
            ItemDispenser dispenser = hit.collider.GetComponentInParent<ItemDispenser>();
            Workbench workbench = hit.collider.GetComponentInParent<Workbench>();
            Forge forge = hit.collider.GetComponentInParent<Forge>();

            if (part != null && part.itemData != null)
            {
                itemNameText.text = part.itemData.itemName;
            }
            else if (dispenser != null && dispenser.itemToDispense != null)
            {
                itemNameText.text = dispenser.itemToDispense.itemName;
            }
            else if (workbench != null)
            {
                RecipeData match = workbench.GetMatchingRecipe();
                if (match != null)
                    itemNameText.text = "Press E to craft: " + match.craftedWeapon.itemName;
                else if (workbench.HasParts())
                    itemNameText.text = "Workbench (no matching recipe)";
                else
                    itemNameText.text = "Workbench";
            }
            else if (forge != null)
            {
                itemNameText.text = forge.IsSmelting() ? "Smelting..." : "Forge";
            }
            else
            {
                itemNameText.text = "";
            }
        }
        else
        {
            itemNameText.text = "";
        }
    }
}