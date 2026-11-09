using System;

public enum TutorialEventType
{
    Continue,
    Sprint,
    ItemDispensed,
    ItemPickedUp,
    ItemPlacedOnBench,
    SmeltingStarted,
    ItemSmelted,
    ItemCrafted,
    OrderDelivered,
    WrongDelivery,
    ItemTrashed
}


public static class TutorialEvents
{
    public static event Action<TutorialEventType, ItemData> OnEvent;

    public static void Raise(TutorialEventType type, ItemData item = null)
    {
        OnEvent?.Invoke(type, item);
    }
}