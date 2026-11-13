using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DeliveryDesk : MonoBehaviour
{
    private AudioManager audioManager;
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
    public GameObject npcPrefabVariant;
    public Transform npcSpawnPoint;
    private GameObject currentNPC;
    [Range(0f, 1f)]
    public float variantSpawnChance = 0.5f;

    [Header("NPC Arrival")]
    public float maxArrivalWait = 15f;

    [Header("Lose Condition")]
    public TextMeshProUGUI strikeTextUI;
    private int strikes = 0;
    public int maxStrikes = 3;
    public GameObject gameOverPanel;

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

    [Header("Win Screen (last level only)")]
    public GameObject winPanel;
    public string restartSceneName = "Level1";
    public bool quitToMainMenu = false;
    public string mainMenuSceneName = "MainMenu";
    private bool hasWon = false;
    private bool isLeaving = false;

    public static bool GameEnded { get; private set; }

    [Header("Tutorial")]
    public bool waitForTutorial = false;
    private bool tutorialOrderActive = false;
    private bool shiftStarted = false;
    private Coroutine tutorialOrderRoutine;

 
    private readonly List<CraftingPart> partsOnDesk = new List<CraftingPart>();

    private void Awake()
    {
        GameEnded = false;
    }

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();

        if (waitForTutorial)
        {
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

        if (tutorialOrderRoutine != null)
        {
            StopCoroutine(tutorialOrderRoutine);
            tutorialOrderRoutine = null;
        }

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

        if (tutorialOrderRoutine != null) StopCoroutine(tutorialOrderRoutine);
        tutorialOrderRoutine = StartCoroutine(TutorialOrderRoutine(recipe));
    }

    private IEnumerator TutorialOrderRoutine(RecipeData recipe)
    {
        currentOrder = null;
        isTimerRunning = false;
        if (timerTextUI != null) timerTextUI.text = "";

        if (npcPrefab != null && currentNPC == null)
            currentNPC = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);

        orderTextUI.text = "A customer is approaching...";
        yield return WaitForNPCArrival();

        currentOrder = recipe;
        orderTextUI.text = "Order: " + recipe.craftedWeapon.itemName;
        if (timerTextUI != null) timerTextUI.text = "No rush!";
        tutorialOrderRoutine = null;

        CheckItemsAlreadyOnDesk();
    }

    private IEnumerator SpawnNewCustomer()
    {
        orderTextUI.text = "Waiting for customer...";
        if (timerTextUI != null) timerTextUI.text = "";
        yield return new WaitForSeconds(timeBetweenCustomers);

        GameObject prefabToSpawn = npcPrefab;
        if (npcPrefabVariant != null && Random.value <= variantSpawnChance)
            prefabToSpawn = npcPrefabVariant;

        if (prefabToSpawn != null)
        {
            currentNPC = Instantiate(prefabToSpawn, npcSpawnPoint.position, npcSpawnPoint.rotation);
            orderTextUI.text = "A customer is approaching...";
            yield return WaitForNPCArrival();
        }

        if (isGameOver) yield break;

    
        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];
        orderTextUI.text = "Order: " + currentOrder.craftedWeapon.itemName;
        timer = timePerOrder;
        isTimerRunning = true;

        CheckItemsAlreadyOnDesk();
    }

    private IEnumerator WaitForNPCArrival()
    {
        if (currentNPC == null) yield break;

        MoveToSpot mover = currentNPC.GetComponent<MoveToSpot>();
        if (mover == null) yield break; 

        float waited = 0f;
        while (currentNPC != null && !mover.HasArrived && waited < maxArrivalWait)
        {
            waited += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();
        if (part == null) return;

        if (!partsOnDesk.Contains(part)) partsOnDesk.Add(part);
        TryDeliver(part);
    }

    private void OnTriggerExit(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();
        if (part != null) partsOnDesk.Remove(part);
    }

    private void CheckItemsAlreadyOnDesk()
    {
        partsOnDesk.RemoveAll(p => p == null);

        foreach (CraftingPart part in new List<CraftingPart>(partsOnDesk))
        {
            if (currentOrder == null) break;
            TryDeliver(part);
        }
    }

    private void TryDeliver(CraftingPart deliveredPart)
    {
        if (currentOrder == null || deliveredPart == null) return;

        bool correct = deliveredPart.itemData == currentOrder.craftedWeapon;
        partsOnDesk.Remove(deliveredPart);

        if (tutorialOrderActive)
        {
            Destroy(deliveredPart.gameObject);

            if (correct)
            {
                tutorialOrderActive = false;
                currentOrder = null;
                if (currentNPC != null) Destroy(currentNPC);
                PlaySFX(audioManager != null ? audioManager.CustomerSuccess : null);
                if (coinParticle != null) coinParticle.Play();
                orderTextUI.text = "Order Completed!";
                if (timerTextUI != null) timerTextUI.text = "";
                TutorialEvents.Raise(TutorialEventType.OrderDelivered, deliveredPart.itemData);
            }
            else
            {
                orderTextUI.text = "That's not what they ordered!\nOrder: " + currentOrder.craftedWeapon.itemName;
                PlaySFX(audioManager != null ? audioManager.CustomerFail : null);
                TutorialEvents.Raise(TutorialEventType.WrongDelivery, deliveredPart.itemData);
            }
            return;
        }

        if (correct)
        {
            TutorialEvents.Raise(TutorialEventType.OrderDelivered, deliveredPart.itemData);
            PlaySFX(audioManager != null ? audioManager.CustomerSuccess : null);
            StartCoroutine(CompleteOrderSequence(deliveredPart.gameObject));
        }
        else
        {
            TutorialEvents.Raise(TutorialEventType.WrongDelivery, deliveredPart.itemData);
            PlaySFX(audioManager != null ? audioManager.CustomerFail : null);
            Destroy(deliveredPart.gameObject);
            HandleStrike();
        }
    }

    private IEnumerator CompleteOrderSequence(GameObject deliveredWeapon)
    {
        Destroy(deliveredWeapon);
        currentOrder = null;
        isTimerRunning = false;

        if (currentNPC != null) Destroy(currentNPC);
        completedOrders++;

        if (completedOrders >= ordersToWin)
        {
            if (SceneController.instance == null || SceneController.instance.IsLastScene())
            {
                ShowWinScreen();
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
            ShowGameOverScreen();
        }
        else if (timer <= 0)
        {
            if (currentNPC != null) Destroy(currentNPC);
            currentOrder = null;
            StartCoroutine(SpawnNewCustomer());
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioManager != null && clip != null) audioManager.PlaySFX(clip);
    }

    private void ShowGameOverScreen()
    {
        isGameOver = true;
        GameEnded = true;
        isTimerRunning = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            orderTextUI.text = "";
            if (timerTextUI != null) timerTextUI.text = "";
        }
        else
        {
            orderTextUI.text = "GAME OVER\nPress R to Restart";
            orderTextUI.color = Color.red;
        }

        Time.timeScale = 0f;
    }

    private void ShowWinScreen()
    {
        hasWon = true;
        isGameOver = true;
        GameEnded = true;
        isTimerRunning = false;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            orderTextUI.text = "";
            if (timerTextUI != null) timerTextUI.text = "";
        }
        else
        {
            orderTextUI.color = Color.green;
            orderTextUI.text = "You Win!\nPress R to Restart or Q to Quit";
        }

        Time.timeScale = 0f;
    }

    private void LoadSceneByName(string sceneName)
    {
        isLeaving = true;
        Time.timeScale = 1f;

        if (SceneController.instance != null) SceneController.instance.LoadScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    private void QuitGame()
    {
        if (quitToMainMenu)
        {
            LoadSceneByName(mainMenuSceneName);
            return;
        }

        isLeaving = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (isGameOver && !isLeaving && kb != null)
        {
            if (kb.rKey.wasPressedThisFrame)
            {
                LoadSceneByName(hasWon ? restartSceneName : SceneManager.GetActiveScene().name);
            }
            else if (kb.qKey.wasPressedThisFrame)
            {
                QuitGame();
            }
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