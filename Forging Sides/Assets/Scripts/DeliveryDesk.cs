using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeliveryDesk : MonoBehaviour
{
    [Header("Orders")]
    public List<RecipeData> possibleOrders;
    private RecipeData currentOrder;

    [Header("UI Feedback")]
    public TextMeshProUGUI orderTextUI;

    [Header("NPC Spawning")]
    public GameObject npcPrefab;
    public Transform npcSpawnPoint;
    private GameObject currentNPC;

    public float timeBetweenCustomers = 3f;
    public float successMessageDuration = 2f;

    private void Start()
    {
        StartCoroutine(SpawnNewCustomer());
    }

    private IEnumerator SpawnNewCustomer()
    {
        orderTextUI.text = "Waiting for customer...";
        yield return new WaitForSeconds(timeBetweenCustomers);

      
        if (npcPrefab != null)
        {
            currentNPC = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        }

        // Pick a random recipe
        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];
        orderTextUI.text = "Order: " + currentOrder.craftedWeapon.itemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentOrder == null) return;

        CraftingPart deliveredPart = other.GetComponent<CraftingPart>();

        if (deliveredPart != null)
        {
            // Check if the item dropped is the right weapon
            if (deliveredPart.itemData == currentOrder.craftedWeapon)
            {
                StartCoroutine(CompleteOrderSequence(deliveredPart.gameObject));
            }
            else
            {
    
                Debug.Log("Wrong item, They want a " + currentOrder.craftedWeapon.itemName);
            }
        }
    }

    private IEnumerator CompleteOrderSequence(GameObject deliveredWeapon)
    {
        Destroy(deliveredWeapon);
        orderTextUI.text = "Order Completed!";
        currentOrder = null; //Clear the active order

        if (currentNPC != null)
        {
            Destroy(currentNPC); //Despawn the NPC
        }

        
        yield return new WaitForSeconds(successMessageDuration);
        StartCoroutine(SpawnNewCustomer());
    }
}