using UnityEngine;

public class DatabaseTriggerBridge : MonoBehaviour
{
    [Header("Настройки для добавления в инвентарь")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private int currentStack = 1;
    [SerializeField] private string surfaceName;

    [Header("Настройки для записи штрафа")]
    [SerializeField] private string errorMessage;
    [SerializeField] private int penaltyPoints;

    // Этот метод появится в UnityEvent, так как у него 0 параметров!
    public void TriggerAddInventoryItem()
    {
        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.AddInventoryItem(itemId, displayName, currentStack, surfaceName);
        }
    }

    // Этот метод тоже появится в UnityEvent
    public void TriggerRecordOmpPenalty()
    {
        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.RecordOmpPenalty(errorMessage, penaltyPoints);
        }
    }
}
