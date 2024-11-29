using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class NewGame : MonoBehaviour
{
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
    }

    public void OnNewGameButtonClick()
    {
        AddSave();
    }

    void AddSave()
    {
        string userName = UserSession.UserName;

        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Имя пользователя не задано. Сохранение невозможно.");
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

                    string userIdQuery = "SELECT id FROM users WHERE name = @name;";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = userIdQuery;

                        IDbDataParameter nameParam = command.CreateParameter();
                        nameParam.ParameterName = "@name";
                        nameParam.Value = userName;
                        command.Parameters.Add(nameParam);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            int userId = System.Convert.ToInt32(result);
                            Debug.Log($"ID пользователя {userName} найден: {userId}");

                         
                            string insertSaveQuery = "INSERT INTO saves (userId, currSave) VALUES (@userId, @currSave);";

                            using (IDbCommand insertSaveCommand = dbConnection.CreateCommand())
                            {
                                insertSaveCommand.CommandText = insertSaveQuery;

                                IDbDataParameter userIdParam = insertSaveCommand.CreateParameter();
                                userIdParam.ParameterName = "@userId";
                                userIdParam.Value = userId;
                                insertSaveCommand.Parameters.Add(userIdParam);

                                IDbDataParameter currSaveParam = insertSaveCommand.CreateParameter();
                                currSaveParam.ParameterName = "@currSave";
                                currSaveParam.Value = "cave,15,10,5"; 
                                insertSaveCommand.Parameters.Add(currSaveParam);

                                int rowsAffected = insertSaveCommand.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    Debug.Log($"Сохранение добавлено успешно для пользователя {userName}.");
                                }
                                else
                                {
                                    Debug.LogWarning("Не удалось добавить сохранение.");
                                }
                            }

                           
                            string checkStatsQuery = "SELECT COUNT(*) FROM stats WHERE userId = @userId;";

                            using (IDbCommand checkStatsCommand = dbConnection.CreateCommand())
                            {
                                checkStatsCommand.CommandText = checkStatsQuery;

                                IDbDataParameter userIdParam = checkStatsCommand.CreateParameter();
                                userIdParam.ParameterName = "@userId";
                                userIdParam.Value = userId;
                                checkStatsCommand.Parameters.Add(userIdParam);

                                int statsCount = System.Convert.ToInt32(checkStatsCommand.ExecuteScalar());

                                if (statsCount == 0)
                                {
                                  
                                    string insertStatsQuery = "INSERT INTO stats (userId, kills, lorenotes) VALUES (@userId, 0, 0);";

                                    using (IDbCommand insertStatsCommand = dbConnection.CreateCommand())
                                    {
                                        insertStatsCommand.CommandText = insertStatsQuery;

                                        IDbDataParameter statsUserIdParam = insertStatsCommand.CreateParameter();
                                        statsUserIdParam.ParameterName = "@userId";
                                        statsUserIdParam.Value = userId;
                                        insertStatsCommand.Parameters.Add(statsUserIdParam);

                                        int statsRowsAffected = insertStatsCommand.ExecuteNonQuery();
                                        if (statsRowsAffected > 0)
                                        {
                                            Debug.Log($"Запись в таблице stats добавлена для пользователя {userName}.");
                                        }
                                        else
                                        {
                                            Debug.LogWarning("Не удалось добавить запись в таблицу stats.");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log($"Запись в таблице stats уже существует для пользователя {userName}.");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Пользователь с именем {userName} не найден.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Не удалось подключиться к базе данных.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при добавлении сохранения: {ex.Message}");
        }
    }
}
