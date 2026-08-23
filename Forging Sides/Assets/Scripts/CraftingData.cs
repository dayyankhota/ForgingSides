using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Part", menuName = "Steampunk/Crafting Part")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject physicalPrefab; //This is what spawns when an item is crafted
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Steampunk/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<ItemData> requiredParts;
    public ItemData craftedWeapon;
}
