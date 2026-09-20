using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Part", menuName = "Steampunk/Crafting Part")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject physicalPrefab; //This is what spawns when an item is crafted
}
