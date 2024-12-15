using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;

public class FinalRoomTrigger : MonoBehaviour
{
    public Collider2D leftWallCollider;
    public Collider2D rightWallCollider;

    private bool isTriggered = false;
    private string dbPath;
    private bool isLastSaveInSkies = false;

    private void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");


        if (leftWallCollider == null || rightWallCollider == null)
        {
            Debug.LogError("Один или оба коллайдера стен не установлены!");
            return;
        }

    
        leftWallCollider.isTrigger = true;
        rightWallCollider.isTrigger = true;

        CheckLastSavePosition();
    }

    private void CheckLastSavePosition()
    {
        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                Debug.Log("Подключение к БД установлено");

                string query = "SELECT currSave FROM saves ORDER BY id DESC LIMIT 1;"; 

                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = query;

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string currSave = result.ToString();
                        Debug.Log($"Последнее сохранение: {currSave}");

                        if (currSave.Contains("skies"))
                        {
                            isLastSaveInSkies = true;
                        }
                        else
                        {
                            isLastSaveInSkies = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Последнее сохранение не найдено.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при чтении из БД: {ex.Message}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошел в комнату. Ждем 2 секунды перед отключением триггеров.");

            isTriggered = true;
            StartCoroutine(DisableTriggersAfterDelay(2f));
        }
    }

    private IEnumerator DisableTriggersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);


        if (isLastSaveInSkies)
        {
            leftWallCollider.isTrigger = false;
            rightWallCollider.isTrigger = false; 
            Debug.Log("Сейф в позиции 'skies'. Коллайдеры стали твердыми.");
        }
        else
        {
            leftWallCollider.isTrigger = false;
            rightWallCollider.isTrigger = false;
            Debug.Log("Триггеры отключены после задержки.");
        }
    }
}
