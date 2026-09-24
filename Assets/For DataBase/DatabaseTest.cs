using UnityEngine;
using System;
using Npgsql; // Если здесь горит красным — драйвер не подключился. Но у нас он должен работать!

public class DatabaseTest : MonoBehaviour
{
    // Строка подключения к вашей созданной локальной базе
    private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1;Database=unity_omp_db";

    void Start()
    {
        Debug.Log("[БД] Попытка установить соединение...");
        CheckConnection();
    }

    private void CheckConnection()
    {
        // Блок using автоматически закроет соединение после проверки, даже если произойдет ошибка
        using (var connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                // Пробуем открыть соединение с базой
                connection.Open();
                
                // Если код дошел досюда, значит всё отлично!
                Debug.Log("<color=green>[БД] Успех! Проект Unity успешно подключился к PostgreSQL.</color>");
            }
            catch (Exception ex)
            {
                // Если что-то пошло не так (неверный пароль, выключена служба БД и т.д.)
                Debug.LogError($"[БД] Ошибка подключения: {ex.Message}");
            }
        }
    }
}
