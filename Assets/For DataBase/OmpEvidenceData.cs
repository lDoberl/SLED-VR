using UnityEngine;

public class OmpEvidenceData : MonoBehaviour
{
    [Header("Идентификатор следа")]
    [Tooltip("Уникальный ID, например: fingerprint_safe_01")]
    [SerializeField] private string evidenceId;

    /// <summary>
    /// Вызывать, когда игрок обнаружил след (напылил порошок, посветил УФ-лампой)
    /// </summary>
    public void TriggerEvidenceDiscovered()
    {
        if (DatabaseManager.Instance == null) return;

        string timeNow = System.DateTime.Now.ToString("HH:mm:ss");
        
        // Передаем ID, статус "обнаружено = true" и время обнаружения
        DatabaseManager.Instance.SaveEvidenceState(evidenceId, true, timeNow);
        Debug.Log($"[Улика] След '{evidenceId}' обнаружен и зафиксирован в БД.");
    }

    /// <summary>
    /// Вызывать, если игрок отдельно фотографирует этот след на виртуальную камеру
    /// </summary>
    public void TriggerEvidencePhotographed()
    {
        if (DatabaseManager.Instance == null) return;

        string timeNow = System.DateTime.Now.ToString("HH:mm:ss");
        
        // Благодаря ON CONFLICT в БД, этот вызов просто обновит время фотосъемки, не создавая дубликатов
        DatabaseManager.Instance.SaveEvidenceState(evidenceId, true, timeNow);
        Debug.Log($"[Улика] След '{evidenceId}' успешно сфотографирован.");
    }
}
