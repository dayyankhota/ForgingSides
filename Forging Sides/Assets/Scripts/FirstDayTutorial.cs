using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FirstDayTutorial : MonoBehaviour
{
    [Serializable]
    public class TutorialStep
    {
        [TextArea(2, 4)] public string message;

        [Tooltip("What the player must do to finish this step.")]
        public TutorialEventType completeWhen = TutorialEventType.Continue;

        [Tooltip("Only count events involving this item. Leave empty for any item.")]
        public ItemData requiredItem;

        [Min(1)] public int requiredCount = 1;

        [Tooltip("Optional: the marker floats above this while the step is active.")]
        public Transform highlightTarget;

        [Tooltip("Optional: a customer arrives with this order when the step starts (no timer, no strikes).")]
        public RecipeData startTutorialOrder;

        [Tooltip("Optional: anything extra to trigger when the step starts (enable objects, play sounds...).")]
        public UnityEvent onStepStart;
    }

    [Header("Steps")]
    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI messageText;
    [Tooltip("Small line under the message: '[Space] Continue', '(1/2)', 'Done!'")]
    public TextMeshProUGUI hintText;

    [Header("Marker (optional)")]
    [Tooltip("A scene object (e.g. glowing sphere/arrow) WITHOUT a collider.")]
    public GameObject marker;
    public float markerHeight = 1.5f;
    public float bobAmount = 0.15f;
    public float bobSpeed = 3f;

    [Header("References")]
    public DeliveryDesk deliveryDesk;
    public FPController player;

    [Header("Options")]
    public Key continueKey = Key.Space;
    public Key skipKey = Key.Backspace;
    [Tooltip("If false, the tutorial won't replay when the scene is restarted (R after game over).")]
    public bool replayOnRestart = false;
    public float delayBetweenSteps = 0.6f;
    public AudioClip stepCompleteClip;

    [Header("Events")]
    public UnityEvent onTutorialComplete;

    private static bool completedThisSession = false;

    private int currentIndex = -1;
    private int progress;
    private bool waitingForNext;
    private float nextStepTimer;
    private bool finished;
    private AudioManager audioManager;

    private TutorialStep Current =>
        (currentIndex >= 0 && currentIndex < steps.Count) ? steps[currentIndex] : null;

    private void OnEnable() { TutorialEvents.OnEvent += HandleEvent; }
    private void OnDisable() { TutorialEvents.OnEvent -= HandleEvent; }

    private void Start()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();

        if (completedThisSession && !replayOnRestart)
        {
            EndTutorial();
            return;
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        NextStep();
    }

    private void Update()
    {
        if (finished || PauseMenuManager.isPaused) return;

        Keyboard kb = Keyboard.current;

        if (kb != null && kb[skipKey].wasPressedThisFrame)
        {
            EndTutorial();
            return;
        }

        UpdateMarker();


        if (waitingForNext)
        {
            nextStepTimer -= Time.deltaTime;
            if (nextStepTimer <= 0f) NextStep();
            return;
        }

        TutorialStep step = Current;
        if (step == null) return;

        switch (step.completeWhen)
        {
            case TutorialEventType.Continue:
                if (kb != null && kb[continueKey].wasPressedThisFrame) CompleteStep();
                break;

            case TutorialEventType.Sprint:
                if (player != null && player.IsSprinting) CompleteStep();
                break;
        }
    }


    private void HandleEvent(TutorialEventType type, ItemData item)
    {
        if (finished || waitingForNext) return;

        TutorialStep step = Current;
        if (step == null || type != step.completeWhen) return;
        if (step.requiredItem != null && item != step.requiredItem) return;

        progress++;
        UpdateHint();

        if (progress >= step.requiredCount) CompleteStep();
    }

    private void CompleteStep()
    {
        waitingForNext = true;
        nextStepTimer = delayBetweenSteps;

        if (hintText != null) hintText.text = "Done!";
        if (audioManager != null && stepCompleteClip != null) audioManager.PlaySFX(stepCompleteClip);
    }

    private void NextStep()
    {
        currentIndex++;
        waitingForNext = false;
        progress = 0;

        TutorialStep step = Current;
        if (step == null)
        {
            EndTutorial();
            return;
        }

        if (messageText != null) messageText.text = step.message;
        UpdateHint();

        if (marker != null) marker.SetActive(step.highlightTarget != null);

        if (step.startTutorialOrder != null && deliveryDesk != null)
            deliveryDesk.StartTutorialOrder(step.startTutorialOrder);

        step.onStepStart?.Invoke();
    }

    private void EndTutorial()
    {
        if (finished) return;
        finished = true;
        completedThisSession = true;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (marker != null) marker.SetActive(false);

        if (deliveryDesk != null) deliveryDesk.StartShift();
        onTutorialComplete?.Invoke();
    }

    private void UpdateHint()
    {
        if (hintText == null) return;

        TutorialStep step = Current;
        if (step == null) { hintText.text = ""; return; }

        string skip = "   <size=70%>[" + skipKey + "] Skip tutorial</size>";

        if (step.completeWhen == TutorialEventType.Continue)
            hintText.text = "[" + continueKey + "] Continue" + skip;
        else if (step.completeWhen == TutorialEventType.Sprint)
            hintText.text = "Hold [Shift]" + skip;
        else if (step.requiredCount > 1)
            hintText.text = "(" + progress + "/" + step.requiredCount + ")" + skip;
        else
            hintText.text = skip.TrimStart();
    }

    private void UpdateMarker()
    {
        TutorialStep step = Current;
        if (marker == null || step == null || step.highlightTarget == null) return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        marker.transform.position = step.highlightTarget.position + Vector3.up * (markerHeight + bob);
    }
}