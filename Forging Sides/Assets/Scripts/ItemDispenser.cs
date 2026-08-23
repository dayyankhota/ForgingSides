using UnityEngine;

public class ItemDispenser : MonoBehaviour
{
    public ItemData itemToDispense;
    public Transform dispensePoint;

    public void DispenseItem()
    {
        if(itemToDispense != null && itemToDispense.physicalPrefab != null)
        {
            Instantiate(itemToDispense.physicalPrefab, dispensePoint.position, dispensePoint.rotation);
        }
    }
}
