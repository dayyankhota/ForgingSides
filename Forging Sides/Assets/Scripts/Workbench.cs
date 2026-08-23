using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour
{
    public List<RecipeData> knownRecipes;
    public Transform spawnPoint;
    private List<CraftingPart> partsOnTable = new List<CraftingPart>();

    private void OnTriggerEnter(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();
        if (part != null && !partsOnTable.Contains(part))
        {
            partsOnTable.Add(part);
            TryCrafting();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CraftingPart part = other.GetComponent<CraftingPart>();
        if (part != null && partsOnTable.Contains(part)) partsOnTable.Remove(part);
    }

    private void TryCrafting()
    {
        List<ItemData> currentData = new List<ItemData>();
        foreach (var part in partsOnTable) currentData.Add(part.itemData);

        foreach (var recipe in knownRecipes)
        {
            if (CheckRecipeMatch(currentData, recipe.requiredParts))
            {
                foreach (var part in partsOnTable) Destroy(part.gameObject);
                partsOnTable.Clear();
                Instantiate(recipe.craftedWeapon.physicalPrefab, spawnPoint.position, spawnPoint.rotation);
                return;
            }
        }
    }

    private bool CheckRecipeMatch(List<ItemData> current, List<ItemData> required)
    {
        if (current.Count != required.Count) return false;
        List<ItemData> tempRequired = new List<ItemData>(required);
        foreach (var item in current)
        {
            if (tempRequired.Contains(item)) tempRequired.Remove(item);
            else return false;
        }
        return true;
    }
}