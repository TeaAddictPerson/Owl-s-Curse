using UnityEngine;
using UnityEngine.SceneManagement;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;

public class LoadGameManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene"; // Имя вашей игровой сцены
    private string dbPath;

    private void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
    }

    public void LoadLastSave()
    {
        string userName = UserSession.UserName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("Имя пользователя не найдено");
            return;
        }

        Debug.Log($"Загружаем сохранение для пользователя: {userName}");
        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                Debug.Log("Подключение к БД установлено");

                string saveQuery = @"
                    SELECT s.currSave 
                    FROM saves s 
                    JOIN users u ON s.userId = u.id 
                    WHERE u.name = @name;";

                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = saveQuery;
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    param.Value = userName;
                    command.Parameters.Add(param);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string saveData = result.ToString();
                        Debug.Log($"Загружены данные сохранения: {saveData}");

                        // Сохраняем данные для использования после загрузки сцены
                        PlayerPrefs.SetString("LastSave", saveData);
                        PlayerPrefs.Save();

                        // Загружаем игровую сцену
                        Debug.Log($"Загружаем сцену: {gameSceneName}");
                        SceneManager.LoadScene(gameSceneName);
                    }
                    else
                    {
                        Debug.LogError("Сохранение не найдено");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка загрузки: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }
}