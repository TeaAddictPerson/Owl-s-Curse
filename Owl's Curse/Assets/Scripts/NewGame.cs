using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using UnityEngine.SceneManagement;

public class NewGame : MonoBehaviour
{
    private string dbPath;
    public int gameSceneNumber;

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

                            // Удаляем существующее сохранение
                            string deleteSaveQuery = "DELETE FROM saves WHERE userId = @userId;";
                            using (IDbCommand deleteSaveCommand = dbConnection.CreateCommand())
                            {
                                deleteSaveCommand.CommandText = deleteSaveQuery;

                                IDbDataParameter userIdParam = deleteSaveCommand.CreateParameter();
                                userIdParam.ParameterName = "@userId";
                                userIdParam.Value = userId;
                                deleteSaveCommand.Parameters.Add(userIdParam);

                                deleteSaveCommand.ExecuteNonQuery();
                                Debug.Log("Существующее сохранение удалено.");
                            }

                            // Создаем новое сохранение
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
                                    Debug.Log($"Новое сохранение создано для пользователя {userName}.");
                                }
                                else
                                {
                                    Debug.LogWarning("Не удалось создать новое сохранение.");
                                    return;
                                }
                            }

                            // Обновляем статистику
                            string updateStatsQuery = "UPDATE stats SET kills = 0, lorenotes = 0 WHERE userId = @userId;";
                            using (IDbCommand updateStatsCommand = dbConnection.CreateCommand())
                            {
                                updateStatsCommand.CommandText = updateStatsQuery;

                                IDbDataParameter userIdParam = updateStatsCommand.CreateParameter();
                                userIdParam.ParameterName = "@userId";
                                userIdParam.Value = userId;
                                updateStatsCommand.Parameters.Add(userIdParam);

                                int statsRowsAffected = updateStatsCommand.ExecuteNonQuery();

                                // Если записи статистики не существует, создаем новую
                                if (statsRowsAffected == 0)
                                {
                                    string insertStatsQuery = "INSERT INTO stats (userId, kills, lorenotes) VALUES (@userId, 0, 0);";
                                    using (IDbCommand insertStatsCommand = dbConnection.CreateCommand())
                                    {
                                        insertStatsCommand.CommandText = insertStatsQuery;

                                        IDbDataParameter statsUserIdParam = insertStatsCommand.CreateParameter();
                                        statsUserIdParam.ParameterName = "@userId";
                                        statsUserIdParam.Value = userId;
                                        insertStatsCommand.Parameters.Add(statsUserIdParam);

                                        int insertStatsResult = insertStatsCommand.ExecuteNonQuery();
                                        if (insertStatsResult > 0)
                                        {
                                            Debug.Log($"Создана новая запись статистики для пользователя {userName}.");
                                        }
                                        else
                                        {
                                            Debug.LogWarning("Не удалось создать запись статистики.");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log($"Статистика сброшена для пользователя {userName}.");
                                }
                            }

                            // Сохраняем начальные данные в PlayerPrefs и переходим на игровую сцену
                            PlayerPrefs.SetString("LastSave", "cave,15,10,5");
                            PlayerPrefs.Save();
                            SceneManager.LoadScene(gameSceneNumber);
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
            Debug.LogError($"Ошибка при создании новой игры: {ex.Message}");
        }
    }
}