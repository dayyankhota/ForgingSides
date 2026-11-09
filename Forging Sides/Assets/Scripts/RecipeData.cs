using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Steampunk/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<ItemData> requiredParts;
    public ItemData craftedWeapon;

    [Header("Crafting QTE")]
    [Tooltip("Number of key prompts when crafting. 0 = no QTE, crafts instantly.")]
    public int qteKeyCount = 3;
    [Tooltip("Seconds the player has to press each key.")]
    public float qteTimePerKey = 1.2f;
}