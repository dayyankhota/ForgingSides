using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class CraftingPart : MonoBehaviour
{
    public ItemData itemData;


    [HideInInspector] public Workbench currentBench;
}