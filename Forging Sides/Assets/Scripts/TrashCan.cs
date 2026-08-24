using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
      
        CraftingPart trash = other.GetComponent<CraftingPart>();

        if (trash != null)
        {
         
            Destroy(trash.gameObject);

           
            Debug.Log("Incinerated: " + trash.itemData.itemName);
        }
    }
}