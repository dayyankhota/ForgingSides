using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Forge : MonoBehaviour
{
    AudioManager audioManager;
    
    [SerializeField] ParticleSystem forgeSmoke = null;
    [System.Serializable]
    public class SmeltingRecipe
    {

        public ItemData rawMetal;
        public ItemData smeltedMetal;
        public float timeToSmelt = 3f;
    }

    [Header("Forge Setup")]
    public List<SmeltingRecipe> recipes;
    public bool keepOutputInSlot = true;
    public Transform spawnPoint;

    [Header("Snap Slots")]
    public Transform[] slots;

    private CraftingPart[] occupants;

    private class SmeltJob
    {
        public Coroutine routine;
        public float startTime;
        public float duration;
    }

    private readonly Dictionary<CraftingPart, SmeltJob> jobs = new Dictionary<CraftingPart, SmeltJob>();

    private void Awake()
    {
         audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("[Forge] '" + name + "' has no snap slots assigned – items can't be placed in it.");
            slots = new Transform[0];
        }
        occupants = new CraftingPart[slots.Length];
    }


    public bool CanAccept(CraftingPart part)
    {
        return part != null && GetRecipe(part.itemData) != null;
    }

    public bool TryPlacePart(CraftingPart part)
    {
        if (!CanAccept(part)) return false;

        int index = GetFreeSlotIndex();
        if (index == -1) return false;
       

        PlaceInSlot(part, index);
        
        SmeltingRecipe recipe = GetRecipe(part.itemData);
        SmeltJob job = new SmeltJob { startTime = Time.time, duration = recipe.timeToSmelt };
        jobs[part] = job;
        job.routine = StartCoroutine(SmeltRoutine(part, recipe));

        TutorialEvents.Raise(TutorialEventType.SmeltingStarted, part.itemData);
        return true;
    }

    public void RemovePart(CraftingPart part)
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == part) occupants[i] = null;
        }
        part.currentForge = null;

        if (jobs.TryGetValue(part, out SmeltJob job))
        {
            if (job.routine != null) StopCoroutine(job.routine);
            jobs.Remove(part);
        }
    }

    private void PlaceInSlot(CraftingPart part, int index)
    {
        Transform slot = slots[index];
        occupants[index] = part;
        part.currentForge = this;
         audioManager.PlaySFX(audioManager.Furnace);
         if (forgeSmoke != null) forgeSmoke.Play();
        part.transform.SetParent(null);
        part.transform.SetPositionAndRotation(slot.position, slot.rotation);

        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public bool HasFreeSlot()
    {
        return GetFreeSlotIndex() != -1;
    }

    private int GetFreeSlotIndex()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == null) return i;
        }
        return -1;
    }

    private int GetSlotIndex(CraftingPart part)
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == part) return i;
        }
        return -1;
    }

    private IEnumerator SmeltRoutine(CraftingPart part, SmeltingRecipe recipe)
    {
        yield return new WaitForSeconds(recipe.timeToSmelt);

        jobs.Remove(part);

        int slotIndex = GetSlotIndex(part);
        if (slotIndex != -1) occupants[slotIndex] = null;
        Destroy(part.gameObject);

        bool toSlot = keepOutputInSlot && slotIndex != -1;
        Transform target = toSlot ? slots[slotIndex] : spawnPoint;

        GameObject output = Instantiate(recipe.smeltedMetal.physicalPrefab, target.position, target.rotation);

        if (toSlot)
        {
            CraftingPart outputPart = output.GetComponent<CraftingPart>();
            if (outputPart != null) PlaceInSlot(outputPart, slotIndex);
        }

        TutorialEvents.Raise(TutorialEventType.ItemSmelted, recipe.smeltedMetal);
    }

    private SmeltingRecipe GetRecipe(ItemData input)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.rawMetal == input) return recipe;
        }
        return null;
    }

    public bool IsSmelting()
    {
        return jobs.Count > 0;
    }

    public float GetSmeltProgress(CraftingPart part)
    {
        if (part != null && jobs.TryGetValue(part, out SmeltJob job))
            return Mathf.Clamp01((Time.time - job.startTime) / job.duration);
        return -1f;
    }
}