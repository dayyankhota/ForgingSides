using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Steampunk/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public List<ItemData> requiredParts;
    public ItemData craftedWeapon;
}
