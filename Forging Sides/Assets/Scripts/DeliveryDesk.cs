using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DeliveryDesk : MonoBehaviour
{
    private bool isGameOver = false;
    [Header("Orders")]
    public List<RecipeData> possibleOrders;
    private RecipeData currentOrder;

    [Header("UI Feedback")]
    public TextMeshProUGUI orderTextUI;

    [Header("NPC Spawning")]
    public GameObject npcPrefab;
    public Transform npcSpawnPoint;
    private GameObject currentNPC;

    [Header("Lose Condition")]
    public TextMeshProUGUI strikeTextUI;
    private int strikes = 0;
    public int maxStrikes = 3;


    public float timeBetweenCustomers = 3f;
    public float successMessageDuration = 2f;

    [Header("Timer Setup")]
    public TextMeshProUGUI timerTextUI;
    public float timePerOrder = 20f;
    private float timer = 0f;
    private bool isTimerRunning = false;

    [Header("Win Condition")]
    public int ordersToWin = 5;
    private int completedOrders = 0;

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
        timer = timePerOrder;
        isTimerRunning = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (currentOrder == null) return;

        CraftingPart deliveredPart = other.GetComponent<CraftingPart>();

        if (deliveredPart != null)
        {
            if (deliveredPart.itemData == currentOrder.craftedWeapon)
            {
               
                StartCoroutine(CompleteOrderSequence(deliveredPart.gameObject));
            }
            else
            {
                
                Destroy(deliveredPart.gameObject);
                HandleStrike();
            }
        }
    }

    private IEnumerator CompleteOrderSequence(GameObject deliveredWeapon)
    {
        Destroy(deliveredWeapon);
        currentOrder = null;
        isTimerRunning = false; 

        if (currentNPC != null)
        {
            Destroy(currentNPC); 
        }
        completedOrders++;

        if (completedOrders >= ordersToWin)
        {
            orderTextUI.text = "YOU WIN!\nPress R to Restart";
            orderTextUI.color = Color.green;

            Time.timeScale = 0f;
            isGameOver = true; 

            yield break; 
        }
        else
        {
            orderTextUI.text = "Order Completed! (" + completedOrders + "/" + ordersToWin + ")";

            yield return new WaitForSeconds(successMessageDuration);

            StartCoroutine(SpawnNewCustomer());
        }
    }

    private void HandleStrike()
    {
        strikes++;
        strikeTextUI.text = "Strikes: " + strikes + "/" + maxStrikes;

        if (strikes >= maxStrikes)
        {
            orderTextUI.text = "GAME OVER\nPress R to Restart";
            orderTextUI.color = Color.red;
            Time.timeScale = 0f;
            isGameOver = true;
        }
        else
        {
            
            if (timer <= 0)
            {
                if (currentNPC != null) Destroy(currentNPC);
                currentOrder = null;
                StartCoroutine(SpawnNewCustomer());
            }
        }
    }

    private void Update()
    {
       
        if (isGameOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (isTimerRunning && !isGameOver)
        {
            timer -= Time.deltaTime;
            timerTextUI.text = "Time: " + Mathf.Ceil(timer).ToString() + "s";

            if (timer <= 0)
            {
                isTimerRunning = false;
                HandleStrike();
            }
        }
    }
}