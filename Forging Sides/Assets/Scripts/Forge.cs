using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forge : MonoBehaviour
{
    [System.Serializable]
    public class SmeltingRecipe
    {
        public ItemData rawMetal;
        public ItemData smeltedMetal;
        public float timeToSmelt = 3f;
    }

    [Header("Forge Setup")]

    public List<SmeltingRecipe> recipes;
    public Transform spawnPoint;


    // Tracks which items are currently being smelted
    private Dictionary<CraftingPart, Coroutine> smeltingProcesses = new Dictionary<CraftingPart, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();

        // If an item entered the forge and isn't already being processed
        if (part != null && !smeltingProcesses.ContainsKey(part))
        {
            SmeltingRecipe matchedRecipe = GetRecipe(part.itemData);

            if (matchedRecipe != null)
            {

                Coroutine process = StartCoroutine(SmeltRoutine(part, matchedRecipe));
                smeltingProcesses.Add(part, process);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();

        if (part != null && smeltingProcesses.ContainsKey(part))
        {
            StopCoroutine(smeltingProcesses[part]);
            smeltingProcesses.Remove(part);

        }
    }

    private IEnumerator SmeltRoutine(CraftingPart part, SmeltingRecipe recipe)
    {

        yield return new WaitForSeconds(recipe.timeToSmelt);

        smeltingProcesses.Remove(part);

        Destroy(part.gameObject);
        Instantiate(recipe.smeltedMetal.physicalPrefab, spawnPoint.position, spawnPoint.rotation);

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
        return smeltingProcesses.Count > 0;
    }
}