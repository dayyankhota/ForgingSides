using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour
{
    public enum FailPenalty { KeepParts, DestroyParts }

    [Header("Recipes")]
    public List<RecipeData> knownRecipes;
    public Transform spawnPoint;

    [Header("Snap Slots")]
    public Transform[] slots;

    [Header("Quick-Time Event")]
    public CraftingQTE qte;
    public FailPenalty failPenalty = FailPenalty.KeepParts;
    public AudioClip qteHitClip;
    public AudioClip qteFailClip;

    [Header("Feedback")]
    [SerializeField] ParticleSystem sparkParticle = null;

    private CraftingPart[] occupants;
    private AudioManager audioManager;

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("[Workbench] '" + name + "' has no snap slots assigned – items can't be placed on it.");
            slots = new Transform[0];
        }
        occupants = new CraftingPart[slots.Length];
    }

    private void Start()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();
    }

    public bool TryPlacePart(CraftingPart part)
    {
        int index = GetFreeSlotIndex();
        if (index == -1)
        {
            Debug.Log("[Workbench] No free slot for '" + part.name + "'.");
            return false;
        }

        Transform slot = slots[index];
        occupants[index] = part;
        part.currentBench = this;

        part.transform.SetParent(null);
        part.transform.SetPositionAndRotation(slot.position, slot.rotation);

        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        TutorialEvents.Raise(TutorialEventType.ItemPlacedOnBench, part.itemData);
        return true;
    }


    public void RemovePart(CraftingPart part)
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == part) occupants[i] = null;
        }
        part.currentBench = null;
    }

    private int GetFreeSlotIndex()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == null) return i;
        }
        return -1;
    }


    public RecipeData GetMatchingRecipe()
    {
        List<ItemData> current = new List<ItemData>();
        foreach (CraftingPart part in occupants)
        {
            if (part != null) current.Add(part.itemData);
        }

        if (current.Count == 0) return null;

        foreach (RecipeData recipe in knownRecipes)
        {
            if (CheckRecipeMatch(current, recipe.requiredParts)) return recipe;
        }
        return null;
    }


    public bool TryCraft()
    {
        if (CraftingQTE.IsActive) return false;

        RecipeData recipe = GetMatchingRecipe();
        if (recipe == null) return false;

        if (qte != null && recipe.qteKeyCount > 0)
        {
            qte.StartQTE(recipe.qteKeyCount, recipe.qteTimePerKey,
                         OnQTEKeyHit,
                         () => CompleteCraft(recipe),
                         OnQTEFail);
        }
        else
        {
            CompleteCraft(recipe);
        }
        return true;
    }

    private void CompleteCraft(RecipeData recipe)
    {
        if (GetMatchingRecipe() != recipe) return;

        DestroyAllParts();

        Instantiate(recipe.craftedWeapon.physicalPrefab, spawnPoint.position, spawnPoint.rotation);

        if (sparkParticle != null) sparkParticle.Play();
        PlayClip(audioManager != null ? audioManager.metalClang : null);

        TutorialEvents.Raise(TutorialEventType.ItemCrafted, recipe.craftedWeapon);
    }

    private void OnQTEKeyHit()
    {
        if (sparkParticle != null) sparkParticle.Play();
        PlayClip(qteHitClip);
    }

    private void OnQTEFail()
    {
        PlayClip(qteFailClip);
        if (failPenalty == FailPenalty.DestroyParts) DestroyAllParts();
    }

    private void DestroyAllParts()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] != null)
            {
                Destroy(occupants[i].gameObject);
                occupants[i] = null;
            }
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioManager != null && clip != null) audioManager.PlaySFX(clip);
    }

    public bool HasParts()
    {
        foreach (CraftingPart part in occupants)
        {
            if (part != null) return true;
        }
        return false;
    }

    private bool CheckRecipeMatch(List<ItemData> current, List<ItemData> required)
    {
        if (current.Count != required.Count) return false;

        List<ItemData> tempRequired = new List<ItemData>(required);
        foreach (ItemData item in current)
        {
            if (tempRequired.Contains(item)) tempRequired.Remove(item);
            else return false;
        }
        return true;
    }
}