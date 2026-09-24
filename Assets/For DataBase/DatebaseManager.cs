using UnityEngine;
using System;
using Npgsql;

public class DatabaseManager : MonoBehaviour
{
    // Статический доступ, чтобы писать DatabaseManager.Instance из любого скрипта
    public static DatabaseManager Instance { get; private set; }

    [Header("Настройки подключения")]
    [SerializeField] private string dbHost = "localhost";
    [SerializeField] private string dbUser = "postgres";
    [SerializeField] private string dbPassword = "1";
    [SerializeField] private string dbName = "unity_omp_db";

    private string connectionString;
    
    // Сюда запишется ID сессии из PostgreSQL после старта игры
    private int currentSessionId = -1; 

    void Awake()
    {
        // Настройка Синглтона, чтобы менеджер не уничтожался при перезагрузке сцен
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Собираем строку подключения
            connectionString = $"Host={dbHost};Username={dbUser};Password={dbPassword};Database={dbName};Port=5432";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 1. старый тест
        StartNewSession("Следователь первой миссии");
        
        
        // 4. ПРИНУДИТЕЛЬНО ВЫВОДИМ РЕЗУЛЬТАТ В КОНСОЛЬ UNITY
        //    Debug.Log($"<color=yellow><b>[ИТОГИ МИССИИ ИЗ БД]</b>\n" +
        //            $"Игрок: {stats.playerName}\n" +
        //            $"Финальный балл: {stats.finalScore} / 100\n" +
        //            $"Статус завершения: {stats.isCompleted}\n" +
        //            $"Список нарушений:\n{stats.penaltyLog}</color>");
        }


    // ==========================================
    // ЛОГИКА 1: СОЗДАНИЕ ИГРОВОЙ СЕССИИ
    // ==========================================
    public void StartNewSession(string playerName)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                
                // Запрос вставляет запись и возвращает сгенерированный базой session_id
                string sql = "INSERT INTO game_sessions (player_name) VALUES (@name) RETURNING session_id;";
                
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("name", playerName);
                    
                    // ExecuteScalar выполняет запрос и возвращает одно первое значение (наш ID)
                    currentSessionId = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    Debug.Log($"<color=cyan>[БД] Новая сессия зарегистрирована! ID в базе данных = {currentSessionId}</color>");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка при старте сессии: {ex.Message}");
            }
        }
    }
        // ==========================================
    // ЛОГИКА 2: ЗАПИСЬ ШТРАФОВ (ОШИБОК) ИГРОКА
    // ==========================================
    
    /// <summary>
    /// Записывает штраф в базу данных и автоматически уменьшает общий балл сессии.
    /// </summary>
    /// <param name="errorMessage">Описание нарушения</param>
    /// <param name="penaltyPoints">Сколько баллов снять (целое число)</param>
    public void RecordOmpPenalty(string errorMessage, int penaltyPoints)
    {
        // Если сессия почему-то не создалась, не ломаем игру ошибками запроса
        if (currentSessionId == -1)
        {
            Debug.LogWarning("[БД] Попытка записать штраф без активной сессии!");
            return;
        }

        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                
                // 1. Запрос на добавление строки в таблицу штрафов
                string insertSql = @"INSERT INTO omp_penalties (session_id, error_message, penalty_points) 
                                     VALUES (@sid, @msg, @pts);";

                using (var cmd = new NpgsqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    cmd.Parameters.AddWithValue("msg", errorMessage);
                    cmd.Parameters.AddWithValue("pts", penaltyPoints);

                    cmd.ExecuteNonQuery();
                    Debug.Log($"<color=orange>[БД] Штраф записан в базу: \"{errorMessage}\" (-{penaltyPoints} баллов)</color>");
                }

                // 2. Сразу же обновляем итоговый балл в таблице сессий
                UpdateSessionScore(penaltyPoints);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка при записи штрафа: {ex.Message}");
            }
        }
    }

    // Вспомогательный метод для обновления общего счета текущей сессии
    private void UpdateSessionScore(int penaltyPoints)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                
                // Вычитаем штрафные очки из общего балла (изначально там 100)
                string updateSql = @"UPDATE game_sessions 
                                     SET total_score = total_score - @pts 
                                     WHERE session_id = @sid;";

                using (var cmd = new NpgsqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("pts", penaltyPoints);
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка обновления счета сессии: {ex.Message}");
            }
        }
    }
    // ==========================================
    // ЛОГИКА 3: СОХРАНЕНИЕ СОСТОЯНИЯ УЛИК И ИНВЕНТАРЯ
    // ==========================================

    /// <summary>
    /// Фиксирует статус обнаружения скрытой улики (например, отпечатка пальца)
    /// </summary>
    // public void SaveEvidenceState(string evidenceId, bool isDiscovered, string timeOfPhoto = null)
    // {
    //     if (currentSessionId == -1) return;

    //     using (var conn = new NpgsqlConnection(connectionString))
    //     {
    //         try
    //         {
    //             conn.Open();
                
    //             // Используем ON CONFLICT, чтобы при повторном вызове данные обновлялись, а не дублировались
    //             string sql = @"INSERT INTO evidence_states (session_id, evidence_id, is_discovered, time_of_photo) 
    //                            VALUES (@sid, @eid, @disc, @time) 
    //                            ON CONFLICT (session_id, evidence_id) 
    //                            DO UPDATE SET is_discovered = @disc, time_of_photo = @time;";

    //             using (var cmd = new NpgsqlCommand(sql, conn))
    //             {
    //                 cmd.Parameters.AddWithValue("sid", currentSessionId);
    //                 cmd.Parameters.AddWithValue("eid", evidenceId);
    //                 cmd.Parameters.AddWithValue("disc", isDiscovered);
    //                 // Если время не передано, записываем в базу SQL-значение NULL
    //                 cmd.Parameters.AddWithValue("time", (object)timeOfPhoto ?? DBNull.Value);

    //                 cmd.ExecuteNonQuery();
    //                 Debug.Log($"<color=blue>[БД] Статус улики '{evidenceId}' обновлен в базе.</color>");
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             Debug.LogError($"[БД] Ошибка сохранения статуса улики {evidenceId}: {ex.Message}");
    //         }
    //     }
    // }

    /// <summary>
    /// Добавляет предмет в инвентарь текущей сессии с метаданными ОМП
    /// </summary>
    public void AddInventoryItem(string itemId, string displayName, int currentStack, string surfaceName = null, string timeOfPhoto = null, bool inEnvelope = false)
    {
        if (currentSessionId == -1) return;

        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                
                string sql = @"INSERT INTO inventory_items (session_id, item_id, display_name, current_stack, surface_name, time_of_photo, in_envelope) 
                               VALUES (@sid, @iid, @name, @stack, @surf, @time, @env);";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    cmd.Parameters.AddWithValue("iid", itemId);
                    cmd.Parameters.AddWithValue("name", displayName);
                    cmd.Parameters.AddWithValue("stack", currentStack);
                    cmd.Parameters.AddWithValue("surf", (object)surfaceName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("time", (object)timeOfPhoto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("env", inEnvelope);

                    cmd.ExecuteNonQuery();
                    Debug.Log($"<color=magenta>[БД] Предмет '{displayName}' добавлен в инвентарь сессии.</color>");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка добавления предмета {itemId} в инвентарь: {ex.Message}");
            }
        }
    }
        // ==========================================
    // ЛОГИКА 4: ПОЛУЧЕНИЕ ФИНАЛЬНЫХ РЕЗУЛЬТАТОВ
    // ==========================================

    // Структура для удобной передачи финальных итогов в интерфейс (UI)
    public struct FinalResult
    {
        public int sessionId;
        public string playerName;
        public int finalScore;       // Итоговый балл (из 100)
        public bool isCompleted;     // Статус завершения
        public string penaltyLog;    // Список всех ошибок в виде одного текста
    }

    /// <summary>
    /// Финализирует сессию в базе данных (ставит статус завершено)
    /// </summary>
    public void CompleteSession()
    {
        if (currentSessionId == -1) return;

        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string sql = "UPDATE game_sessions SET is_completed = true WHERE session_id = @sid;";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    cmd.ExecuteNonQuery();
                    Debug.Log($"[БД] Сессия {currentSessionId} официально завершена.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка при финализации сессии: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Получает из базы данных полные итоги по текущей сессии
    /// </summary>
    public FinalResult GetCurrentSessionResult()
    {
        FinalResult result = new FinalResult();
        result.sessionId = currentSessionId;
        result.penaltyLog = "";

        if (currentSessionId == -1) return result;

        using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // 1. Получаем имя и итоговый балл из таблицы сессий
                string sessionSql = "SELECT player_name, total_score, is_completed FROM game_sessions WHERE session_id = @sid;";
                using (var cmd = new NpgsqlCommand(sessionSql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.playerName = reader.GetString(0);
                            result.finalScore = reader.GetInt32(1);
                            result.isCompleted = reader.GetBoolean(2);
                        }
                    }
                }

                // 2. Получаем список всех штрафов для вывода на экран итогов
                string penaltySql = "SELECT error_message, penalty_points FROM omp_penalties WHERE session_id = @sid ORDER BY penalty_id ASC;";
                using (var cmd = new NpgsqlCommand(penaltySql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int counter = 1;
                        while (reader.Read())
                        {
                            string msg = reader.GetString(0);
                            int pts = reader.GetInt32(1);
                            
                            // Формируем красивый построчный список ошибок
                            result.penaltyLog += $"{counter}. {msg} (-{pts} б.)\n";
                            counter++;
                        }
                    }
                }

                if (string.IsNullOrEmpty(result.penaltyLog))
                {
                    result.penaltyLog = "Ошибок не допущено! Идеальный осмотр места происшествия.";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[БД] Ошибка при получении итогов: {ex.Message}");
            }
        }

        return result;
    }
    public void SaveEvidenceState(string evidenceId, bool isDiscovered, string timeOfPhoto = null)
    {
    if (currentSessionId == -1) return;

    using (var conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                // Запрос использует ON CONFLICT, чтобы не дублировать строки при повторных вызовах
                string sql = @"
                    INSERT INTO evidence_states (session_id, evidence_id, is_discovered, time_of_photo) 
                    VALUES (@sid, @eid, @disc, @time)
                    ON CONFLICT (session_id, evidence_id) 
                    DO UPDATE SET is_discovered = EXCLUDED.is_discovered, time_of_photo = COALESCE(EXCLUDED.time_of_photo, evidence_states.time_of_photo);";
                    
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("sid", currentSessionId);
                    cmd.Parameters.AddWithValue("eid", evidenceId);
                    cmd.Parameters.AddWithValue("disc", isDiscovered);
                    cmd.Parameters.AddWithValue("time", (object)timeOfPhoto ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    Debug.Log($"[БД] Состояние улики '{evidenceId}' обновлено.");
                }
            }
            catch (Exception ex) { Debug.LogError($"[БД] Ошибка состояния улики: {ex.Message}"); }
        }
    }
}
