using UnityEngine;
using TMPro; // Обязательно подключаем пространство имен TextMeshPro

public class EndScreenControllerTMP : MonoBehaviour
{
    [Header("Элементы UI (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI scoreText;       // Для вывода итоговой оценки
    [SerializeField] private TextMeshProUGUI penaltyLogText;  // Для вывода списка нарушений

    /// <summary>
    /// Вызывается для завершения миссии и отображения результатов на экране
    /// </summary>
    public void DisplayResults()
    {
        // 1. Помечаем в PostgreSQL, что сессия успешно завершена
        DatabaseManager.Instance.CompleteSession();

        // 2. Вытягиваем финальные агрегированные данные из базы
        DatabaseManager.FinalResult stats = DatabaseManager.Instance.GetCurrentSessionResult();

        // 3. Форматируем и выводим итоговый балл с динамическим цветом
        if (scoreText != null)
        {
            string scoreColor = "#FF0000"; // По умолчанию красный (провал)
            
            if (stats.finalScore >= 80) scoreColor = "#00FF00";      // Зеленый (отлично)
            else if (stats.finalScore >= 50) scoreColor = "#FFFF00"; // Желтый (удовлетворительно)

            scoreText.text = $"Результат осмотра: <color={scoreColor}>{stats.finalScore}</color> / 100 баллов";
        }

        // 4. Выводим красивый структурированный список нарушений
        if (penaltyLogText != null)
        {
            // Модифицируем вывод штрафов, подкрашивая "минусы" в красный цвет с помощью TMP-тегов
            string formattedLog = stats.penaltyLog.Replace("(-", " (<color=#FF5555>-");
            formattedLog = formattedLog.Replace(" б.)", " б.</color>)");

            penaltyLogText.text = $"<b >ПРОТОКОЛ НАРУШЕНИЙ ПРИ ОМП:</b>\n\n{formattedLog}";
        }
    }
}
