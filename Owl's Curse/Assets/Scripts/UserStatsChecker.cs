using TMPro;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class UserStatsChecker : MonoBehaviour
{
    public TMP_Text killsText; 
    public TMP_Text lorenotesText; 

    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");

       
        CheckUserStats();
    }

    void CheckUserStats()
    {
        string userName = UserSession.UserName;

     
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Имя пользователя не задано. Выводим нули.");
            DisplayZeros();
            return;
        }

        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                if (dbConnection.State == ConnectionState.Open)
                {
                    Debug.Log("Подключение к базе данных успешно.");

                   
                    string query = @"
                        SELECT kills, lorenotes 
                        FROM stats 
                        WHERE userId = (SELECT id FROM users WHERE name = @name);";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = query;

                        IDbDataParameter nameParam = command.CreateParameter();
                        nameParam.ParameterName = "@name";
                        nameParam.Value = userName;
                        command.Parameters.Add(nameParam);

                        using (IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                               
                                int kills = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int lorenotes = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                                killsText.text = kills.ToString();
                                lorenotesText.text = lorenotes.ToString();

                                Debug.Log($"Для пользователя {userName} получены данные: kills = {kills}, lorenotes = {lorenotes}");
                            }
                            else
                            {
                                Debug.LogWarning("Записей для пользователя не найдено. Выводим нули.");
                                DisplayZeros();
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Не удалось подключиться к базе данных.");
                    DisplayZeros();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при проверке данных: {ex.Message}");
            DisplayZeros();
        }
    }
    void DisplayZeros()
    {
        killsText.text = "0";
        lorenotesText.text = "0";
    }
}
