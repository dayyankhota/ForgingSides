using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    public Transform playerCamera;
    public Transform holdPoint;

    [Header("Reach")]
    public float interactionRange = 4f;
    public float grabRadius = 0.2f;

    [Header("Layers")]
    public LayerMask grabbableLayer;


    [Header("Cooldowns")]
    public float interactCooldown = 0.5f;
    private float lastInteractTime = 0f;

    [Header("UI Feedback")]
    public TextMeshProUGUI itemNameText;

    [Header("Debug")]
    public bool debugRays = false;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;

    private bool RaycastGrabbable(out RaycastHit hit)
    {
        return Physics.SphereCast(playerCamera.position, grabRadius, playerCamera.forward, out hit,
                                  interactionRange, grabbableLayer, QueryTriggerInteraction.Ignore);
    }

    private bool RaycastWorld(out RaycastHit hit)
    {
        return Physics.Raycast(playerCamera.position, playerCamera.forward, out hit,
                               interactionRange, ~grabbableLayer, QueryTriggerInteraction.Collide);
    }

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

        if (RaycastWorld(out RaycastHit hit))
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
        if (!RaycastGrabbable(out RaycastHit hit))
        {
            if (debugRays) Debug.Log("[Grab] Nothing grabbable within " + interactionRange + "m.");
            return;
        }

        Rigidbody rb = hit.rigidbody;
        if (rb == null)
        {
            if (debugRays) Debug.LogWarning("[Grab] Hit '" + hit.collider.name + "' on the grabbable layer, but it has no Rigidbody.");
            return;
        }

        heldObject = rb.gameObject;
        heldObjectRb = rb;

        CraftingPart part = heldObject.GetComponent<CraftingPart>();
        if (part != null)
        {
            if (part.currentBench != null) part.currentBench.RemovePart(part);
            if (part.currentForge != null) part.currentForge.RemovePart(part);

            TutorialEvents.Raise(TutorialEventType.ItemPickedUp, part.itemData);
        }

        heldObjectRb.isKinematic = true;
        heldObject.transform.position = holdPoint.position;
        heldObject.transform.parent = holdPoint;
    }

    private void Drop()
    {
        CraftingPart part = heldObject.GetComponent<CraftingPart>();
        if (part != null && RaycastWorld(out RaycastHit stationHit))
        {
            Workbench workbench = stationHit.collider.GetComponentInParent<Workbench>();
            Forge forge = stationHit.collider.GetComponentInParent<Forge>();

            if (debugRays)
                Debug.Log("[Drop] Ray hit '" + stationHit.collider.name + "', workbench: " + (workbench != null) + ", forge: " + (forge != null));

            bool snapped = (workbench != null && workbench.TryPlacePart(part))
                        || (forge != null && forge.TryPlacePart(part));

            if (snapped)
            {
                heldObject = null;
                heldObjectRb = null;
                return;
            }
        }

        heldObject.transform.parent = null;
        Vector3 rayDirection = holdPoint.position - playerCamera.position;
        float distance = Vector3.Distance(playerCamera.position, holdPoint.position);

        if (Physics.Raycast(playerCamera.position, rayDirection.normalized, out RaycastHit hit, distance, ~grabbableLayer, QueryTriggerInteraction.Ignore))
        {
            heldObject.transform.position = hit.point - (rayDirection.normalized * 0.1f);
        }

        heldObjectRb.isKinematic = false;

        heldObject = null;
        heldObjectRb = null;
    }

    private void Update()
    {
        if (PauseMenuManager.isPaused) return;
        if (CraftingQTE.IsActive) { itemNameText.text = ""; return; }

        if (debugRays)
            Debug.DrawRay(playerCamera.position, playerCamera.forward * interactionRange, Color.yellow);

        if (heldObject != null)
        {
            itemNameText.text = GetHoldingPrompt();
            return;
        }

        if (RaycastGrabbable(out RaycastHit itemHit))
        {
            CraftingPart part = itemHit.collider.GetComponentInParent<CraftingPart>();
            if (part != null && part.itemData != null)
            {
                string text = part.itemData.itemName;
                if (part.currentForge != null)
                {
                    float progress = part.currentForge.GetSmeltProgress(part);
                    if (progress >= 0f) text += "  (Smelting " + Mathf.RoundToInt(progress * 100f) + "%)";
                    else text += "  (Ready)";
                }

                itemNameText.text = text;
                return;
            }
        }

        if (RaycastWorld(out RaycastHit hit))
        {
            ItemDispenser dispenser = hit.collider.GetComponentInParent<ItemDispenser>();
            Workbench workbench = hit.collider.GetComponentInParent<Workbench>();
            Forge forge = hit.collider.GetComponentInParent<Forge>();

            if (dispenser != null && dispenser.itemToDispense != null)
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
                itemNameText.text = forge.IsSmelting() ? "Forge (Smelting...)" : "Forge";
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

    private string GetHoldingPrompt()
    {
        if (!RaycastWorld(out RaycastHit hit)) return "";

        CraftingPart held = heldObject.GetComponent<CraftingPart>();
        if (held == null) return "";

        Workbench workbench = hit.collider.GetComponentInParent<Workbench>();
        if (workbench != null)
            return workbench.HasFreeSlot() ? "Click to place on Workbench" : "Workbench full";

        Forge forge = hit.collider.GetComponentInParent<Forge>();
        if (forge != null)
        {
            if (!forge.CanAccept(held)) return "Can't smelt this";
            return forge.HasFreeSlot() ? "Click to place in Forge" : "Forge full";
        }

        return "";
    }
}