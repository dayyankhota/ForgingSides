using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CraftingQTE : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    [Header("Keys")]
   
    public Key[] possibleKeys = { Key.Q, Key.E, Key.R, Key.F, Key.Z, Key.X, Key.C };
    public bool failOnWrongKey = true;

    [Header("UI")]
    public GameObject qtePanel;
    public TextMeshProUGUI keyPromptText;
    public TextMeshProUGUI progressText;
    public Image timerFill;

    [Header("Feedback")]
    public Color normalColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float resultDisplayTime = 0.6f;

    private readonly List<Key> sequence = new List<Key>();
    private int index;
    private float timePerKey;
    private float keyTimer;
    private int startFrame;

    private Action onKeyHit;
    private Action onSuccess;
    private Action onFail;

    private bool showingResult;
    private float resultTimer;

    private void Awake()
    {
        IsActive = false;
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    private void OnDisable()
    {
        IsActive = false;
    }

    public void StartQTE(int length, float secondsPerKey, Action keyHit, Action success, Action fail)
    {
        if (IsActive || possibleKeys.Length == 0) return;

        sequence.Clear();
        Key last = Key.None;
        for (int i = 0; i < length; i++)
        {
            Key k;
            do { k = possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)]; }
            while (possibleKeys.Length > 1 && k == last);

            sequence.Add(k);
            last = k;
        }

        index = 0;
        timePerKey = secondsPerKey;
        onKeyHit = keyHit;
        onSuccess = success;
        onFail = fail;

        startFrame = Time.frameCount;

        showingResult = false;
        IsActive = true;
        if (qtePanel != null) qtePanel.SetActive(true);
        ShowCurrentKey();
    }

    private void Update()
    {
        if (showingResult)
        {
            resultTimer -= Time.unscaledDeltaTime;
            if (resultTimer <= 0f)
            {
                showingResult = false;
                if (qtePanel != null) qtePanel.SetActive(false);
            }
            return;
        }

        if (!IsActive || PauseMenuManager.isPaused) return;
        if (Time.frameCount == startFrame) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        keyTimer -= Time.deltaTime;
        if (timerFill != null) timerFill.fillAmount = Mathf.Clamp01(keyTimer / timePerKey);

        if (keyTimer <= 0f)
        {
            Finish(false);
            return;
        }

        Key expected = sequence[index];

        if (kb[expected].wasPressedThisFrame)
        {
            index++;
            onKeyHit?.Invoke();

            if (index >= sequence.Count) Finish(true);
            else ShowCurrentKey();
            return;
        }

        if (failOnWrongKey)
        {
            foreach (Key k in possibleKeys)
            {
                if (k != expected && kb[k].wasPressedThisFrame)
                {
                    Finish(false);
                    return;
                }
            }
        }
    }

    private void ShowCurrentKey()
    {
        keyTimer = timePerKey;

        if (keyPromptText != null)
        {
            keyPromptText.text = "[" + sequence[index].ToString() + "]";
            keyPromptText.color = normalColor;
        }
        if (progressText != null) progressText.text = (index + 1) + " / " + sequence.Count;
        if (timerFill != null) timerFill.fillAmount = 1f;
    }

    private void Finish(bool success)
    {
        IsActive = false;

        if (keyPromptText != null)
        {
            keyPromptText.text = success ? "Forged!" : "Failed!";
            keyPromptText.color = success ? successColor : failColor;
        }
        if (progressText != null) progressText.text = "";
        if (timerFill != null) timerFill.fillAmount = 0f;

        showingResult = true;
        resultTimer = resultDisplayTime;

        Action callback = success ? onSuccess : onFail;
        onKeyHit = onSuccess = onFail = null;
        callback?.Invoke();
    }
}