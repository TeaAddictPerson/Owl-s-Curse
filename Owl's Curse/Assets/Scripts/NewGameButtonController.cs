using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class NewGameButtonController : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;

    private Color normalTextColor;
    private Color disabledTextColor;
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
        normalTextColor = buttonText.color;
        disabledTextColor = new Color(normalTextColor.r, normalTextColor.g, normalTextColor.b, 0.5f);

      
        SetButtonState(false);

        
        CheckSaveDataForUser();
    }

    void CheckSaveDataForUser()
    {
     
        string userName = UserSession.UserName;

  
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Имя пользователя не задано!");
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
                    
                    string query = "SELECT userId, currSave FROM saves WHERE userId = (SELECT id FROM users WHERE name = @name);";

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
                               
                                int userId = reader.GetInt32(0);
                                string currSave = reader.GetString(1);

                            
                                if (!string.IsNullOrEmpty(currSave))
                                {
                                    SetButtonState(true);
                                }
                                else
                                {
                                    SetButtonState(false); 
                                }
                            }
                            else
                            {
                                SetButtonState(false); 
                            }
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
            Debug.LogError($"Ошибка при проверке данных пользователя: {ex.Message}");
        }
    }


    void SetButtonState(bool isActive)
    {
        button.interactable = isActive;
        buttonText.color = isActive ? normalTextColor : disabledTextColor;
    }
}
