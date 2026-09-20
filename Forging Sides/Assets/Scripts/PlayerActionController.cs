using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerActionController : MonoBehaviour
{
    
    public static bool GameIsPaused = false; 
    public GameObject pauseMenuUI; 

    public Transform playerCamera;
    public Transform holdPoint;

    public float interactionRange = 3f;
    public LayerMask grabbableLayer;
    public LayerMask interactableLayer;

    [Header("Cooldowns")]
    public float interactCooldown = 0.5f;
    private float lastInteractTime = 0f;

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
       
        if (context.started && heldObject == null)
        {
            if (Time.time - lastInteractTime >= interactCooldown)
            {
                lastInteractTime = Time.time;

                if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, interactableLayer))
                {
                    ItemDispenser dispenser = hit.collider.GetComponent<ItemDispenser>();
                    if (dispenser != null)
                    {
                        dispenser.DispenseItem();
                    }
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


    public void OnPause(InputAction.CallbackContext context)
    {
         pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; 


      
    }


    [Header("UI Feedback")]
    public TextMeshProUGUI itemNameText;

    private void Update()
    {
        if (heldObject == null)
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactionRange, grabbableLayer | interactableLayer))
            {
                CraftingPart part = hit.collider.GetComponent<CraftingPart>();
                ItemDispenser dispenser = hit.collider.GetComponent<ItemDispenser>();

                if (part != null && part.itemData != null)
                {
                    itemNameText.text = part.itemData.itemName;
                }
                else if (dispenser != null && dispenser.itemToDispense != null)
                {
                    itemNameText.text = dispenser.itemToDispense.itemName;
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
        else
        {
            itemNameText.text = "";
        }
    }
   



}