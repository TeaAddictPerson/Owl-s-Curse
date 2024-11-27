using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using TMPro;

public class CheckUser : MonoBehaviour
{
    public TMP_InputField inputField; 
    private string dbPath;           

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
    }

    public void Check()
    {
        string userName = inputField.text.Trim(); 

        if (string.IsNullOrWhiteSpace(userName))
        {
            Debug.LogWarning("Имя пользователя не может быть пустым или состоять только из пробелов!");
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
                    Debug.Log("Подключение к базе данных успешно!");

                    string query = "SELECT COUNT(*) FROM users WHERE name = @name;";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = query;

                        IDbDataParameter param = command.CreateParameter();
                        param.ParameterName = "@name";
                        param.Value = userName; 
                        command.Parameters.Add(param);

                        int userCount = Convert.ToInt32(command.ExecuteScalar());
                        
                        if (userCount > 0)
                        {
                            Debug.Log("Пользователь найден в базе данных.");
                        }
                        else
                        {
                            Debug.Log("Пользователь не найден в базе данных.");
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
            Debug.LogError($"Ошибка при проверке пользователя: {ex.Message}");
        }
    }
}
