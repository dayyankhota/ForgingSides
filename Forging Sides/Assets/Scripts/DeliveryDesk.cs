using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DeliveryDesk : MonoBehaviour
{
    private SceneController levelCheck;

    private Animator animator;
    AudioManager audioManager;
    public Transform destinationSpot;

    [SerializeField] ParticleSystem coinParticle = null;
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
    public int ordersToWin = 2;
    private int completedOrders = 0;

    [Header("Tutorial")]
    [Tooltip("If true, customers don't arrive until StartShift() is called (by the tutorial).")]
    public bool waitForTutorial = false;
    private bool tutorialOrderActive = false;
    private bool shiftStarted = false;

    private void Start()
    {
        if (waitForTutorial)
        {
            audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
            orderTextUI.text = "";
            if (timerTextUI != null) timerTextUI.text = "";
        }
        else
        {
            StartShift();
        }
    }

    public void StartShift()
    {
        if (shiftStarted) return;
        shiftStarted = true;

        if (tutorialOrderActive)
        {
            tutorialOrderActive = false;
            currentOrder = null;
            if (currentNPC != null) Destroy(currentNPC);
        }

        StartCoroutine(SpawnNewCustomer());
    }

    public void StartTutorialOrder(RecipeData recipe)
    {
        if (shiftStarted || recipe == null) return;

        tutorialOrderActive = true;

        if (npcPrefab != null && currentNPC == null)
            currentNPC = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);

        currentOrder = recipe;
        orderTextUI.text = "Order: " + recipe.craftedWeapon.itemName;
        if (timerTextUI != null) timerTextUI.text = "No rush!";
        isTimerRunning = false;
    }


    private IEnumerator SpawnNewCustomer()
    {
        orderTextUI.text = "Waiting for customer...";
        yield return new WaitForSeconds(timeBetweenCustomers);

        if (npcPrefab != null)
        {
            currentNPC = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        }
               MoveToSpot movement = currentNPC.GetComponent<MoveToSpot>();
 
        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];
        orderTextUI.text = "Order: " + currentOrder.craftedWeapon.itemName;
        timer = timePerOrder;
        isTimerRunning = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentOrder == null) return;

        CraftingPart deliveredPart = other.GetComponent<CraftingPart>();
        if (deliveredPart == null) return;

        bool correct = deliveredPart.itemData == currentOrder.craftedWeapon;

        if (tutorialOrderActive)
        {
            Destroy(deliveredPart.gameObject);

            if (correct)
            {
                
                tutorialOrderActive = false;
                currentOrder = null;
                if (currentNPC != null) Destroy(currentNPC);
                audioManager.PlaySFX(audioManager.CustomerSuccess);
                if (coinParticle != null) coinParticle.Play();
                orderTextUI.text = "Order Completed!";
                if (timerTextUI != null) timerTextUI.text = "";
                TutorialEvents.Raise(TutorialEventType.OrderDelivered, deliveredPart.itemData);
            }
            else
            {
                orderTextUI.text = "That's not what they ordered!\nOrder: " + currentOrder.craftedWeapon.itemName;
                 audioManager.PlaySFX(audioManager.CustomerFail);
                TutorialEvents.Raise(TutorialEventType.WrongDelivery, deliveredPart.itemData);
            }
            return;
        }

        if (correct)
        {
            TutorialEvents.Raise(TutorialEventType.OrderDelivered, deliveredPart.itemData);
            audioManager.PlaySFX(audioManager.CustomerSuccess);
            StartCoroutine(CompleteOrderSequence(deliveredPart.gameObject));
        }
        else
        {
            TutorialEvents.Raise(TutorialEventType.WrongDelivery, deliveredPart.itemData);
            audioManager.PlaySFX(audioManager.CustomerFail);
            Destroy(deliveredPart.gameObject);
            HandleStrike();
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
            if (SceneController.instance.IsLastScene())
            {
                orderTextUI.color = Color.green;
                orderTextUI.text = "You Win \nPress R to Restart";
                Time.timeScale = 0f;
                isGameOver = true;
                yield break;
            }
            else
            {
                SceneController.instance.NextLevel();
            }

        }
        else
        {
            orderTextUI.text = "Order Completed! (" + completedOrders + "/" + ordersToWin + ")";
            if (coinParticle != null) coinParticle.Play();
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