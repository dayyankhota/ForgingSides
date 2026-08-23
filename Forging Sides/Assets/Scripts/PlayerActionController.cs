using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    public Transform playerCamera;
    public Transform holdPoint;

    public float interactionRange = 3f;
    public LayerMask grabbableLayer;
    public LayerMask interactableLayer;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (heldObject == null) TryGrab();
            else Drop();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && heldObject == null) //Only allows pickup when hand is empty
            Debug.Log("Interact button was pressed and hands are empty!");
        {
            if(Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, interactableLayer))
            {
                ItemDispenser dispenser = hit.collider.GetComponent<ItemDispenser>();
                if (dispenser != null)
                {
                    dispenser.DispenseItem();
                }
            }
        }
    }

    private void TryGrab()
    {
        if(Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, grabbableLayer))
        {
            heldObject = hit.collider.gameObject;
            heldObjectRb = heldObject.GetComponent<Rigidbody>();
            
            if(heldObjectRb != null)
            {
                heldObjectRb.isKinematic = true;
                heldObject.transform.position = holdPoint.position;
                heldObject.transform.parent = holdPoint;
            }
        }
    }

    private void Drop()
    {
        heldObject.transform.parent = null;
        if (heldObjectRb != null) heldObjectRb.isKinematic = false;
        heldObject = null;
        heldObjectRb = null;
    }
}
