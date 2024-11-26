using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using TMPro;

public class AddUser : MonoBehaviour
{
    public TMP_InputField inputField; 
    private string dbPath;           

    void Start()
    {
       
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
    }

    public void Add()
    {
        string userName = inputField.text.Trim(); 

        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Имя пользователя не может быть пустым!");
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

                  
                    string query = "INSERT INTO users (name) VALUES (@name);";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = query;

                        
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@name";
                        parameter.Value = userName;
                        command.Parameters.Add(parameter);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Debug.Log($"Пользователь '{userName}' успешно добавлен в базу данных.");
                        }
                        else
                        {
                            Debug.LogWarning("Не удалось добавить пользователя в базу данных.");
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
            Debug.LogError($"Ошибка при добавлении пользователя: {ex.Message}");
        }
    }
}
