using UnityEngine;

public class ItemDispenser : MonoBehaviour
{
    public ItemData itemToDispense;
    public Transform dispensePoint;
    AudioManager audioManager;
     private void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    public void DispenseItem()
    {
        if (itemToDispense != null && itemToDispense.physicalPrefab != null)
        {
            Instantiate(itemToDispense.physicalPrefab, dispensePoint.position, dispensePoint.rotation);
            TutorialEvents.Raise(TutorialEventType.ItemDispensed, itemToDispense);
            audioManager.PlaySFX(audioManager.BoxInteract);
        }
    }
}