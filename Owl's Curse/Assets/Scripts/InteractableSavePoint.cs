﻿using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableSavePoint : InteractableBase
{
    [SerializeField] private GameObject savePanel;
    [SerializeField] private TextMeshProUGUI saveText;
    [SerializeField] private string savepointName;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private string savePromptText;

    public float healthBonus = 0f;
    public float damageBonus = 0f;

    private const float BASE_SANITY = 5f;
    private string dbPath;
    private Transform player;
    private PlayerScript playerScript;

    private void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
        }

        if (savePanel == null || saveText == null || confirmButton == null || declineButton == null)
        {
            Debug.LogError("Необходимые компоненты не назначены!");
            return;
        }

        confirmButton.onClick.AddListener(OnSaveConfirm);
        declineButton.onClick.AddListener(OnSaveDecline);

        savePanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        saveText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnSaveConfirm);
        if (declineButton != null) declineButton.onClick.RemoveListener(OnSaveDecline);
    }

    public override void Interact()
    {
        Debug.Log("Открываем панель сохранения");
        savePanel.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);
        saveText.gameObject.SetActive(true);
        saveText.text = savePromptText;
    }

    public void OnSaveConfirm()
    {
        Debug.Log("Начинаем процесс сохранения");
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
        
            playerScript.maxHealth += (int)healthBonus;
            playerScript.attackDamage += (int)damageBonus;

         
            playerScript.currentHealth = playerScript.maxHealth;

       
            if (playerScript.Bar != null)
            {
                playerScript.Bar.fillAmount = (float)playerScript.currentHealth / playerScript.maxHealth;
            }

            playerScript.currentSanity = BASE_SANITY;
            if (playerScript.sanityBar != null)
            {
                playerScript.sanityBar.fillAmount = playerScript.currentSanity / playerScript.maxSanity;
            }

            Debug.Log($"Обновлены характеристики игрока: MaxHP={playerScript.maxHealth}, DMG={playerScript.attackDamage}, CurrentHP={playerScript.currentHealth}, Sanity={playerScript.currentSanity}");

            SaveGame(playerScript);
        }
        else
        {
            Debug.LogError("Не удалось получить компонент PlayerScript");
            HideUI();
        }
    }

    public void OnSaveDecline()
    {
        Debug.Log("Отмена сохранения");
        HideUI();
    }

    private void HideUI()
    {
        Debug.Log("Скрываем UI сохранения");
        savePanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        saveText.gameObject.SetActive(false);
    }

    private void SaveGame(PlayerScript stats)
    {
        string userName = UserSession.UserName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("Имя пользователя не найдено");
            HideUI();
            return;
        }

        Debug.Log($"Начинаем сохранение для пользователя: {userName}");
        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                Debug.Log("Подключение к БД установлено");

                string userIdQuery = "SELECT id FROM users WHERE name = @name;";
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = userIdQuery;
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    param.Value = userName;
                    command.Parameters.Add(param);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        int userId = Convert.ToInt32(result);
                        Debug.Log($"Найден ID пользователя: {userId}");

                      
                        string saveData = $"{savepointName},{stats.maxHealth},{stats.attackDamage},{stats.currentSanity}";
                        Debug.Log($"Подготовлены данные для сохранения: {saveData}");

                   
                        string updateSaveQuery = @"
                        INSERT OR REPLACE INTO saves (userId, currSave) 
                        VALUES (@userId, @currSave);";

                        using (IDbCommand updateSaveCommand = dbConnection.CreateCommand())
                        {
                            updateSaveCommand.CommandText = updateSaveQuery;

                            var userIdParam = updateSaveCommand.CreateParameter();
                            userIdParam.ParameterName = "@userId";
                            userIdParam.Value = userId;
                            updateSaveCommand.Parameters.Add(userIdParam);

                            var saveParam = updateSaveCommand.CreateParameter();
                            saveParam.ParameterName = "@currSave";
                            saveParam.Value = saveData;
                            updateSaveCommand.Parameters.Add(saveParam);

                            int rowsAffected = updateSaveCommand.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Debug.Log($"Игра успешно сохранена в точке: {savepointName}");
                                PlayerPrefs.SetString("LastSave", saveData);
                                PlayerPrefs.Save();
                                Debug.Log("Данные сохранены в PlayerPrefs");
                            }
                            else
                            {
                                Debug.LogError("Не удалось сохранить данные в БД");
                            }
                        }

                      
                        string updateStatsQuery = @"
                        INSERT OR REPLACE INTO stats (userId, kills, lorenotes) 
                        VALUES (@userId, @kills, @lorenotes);";

                        using (IDbCommand updateStatsCommand = dbConnection.CreateCommand())
                        {
                            updateStatsCommand.CommandText = updateStatsQuery;

                            var userIdParam = updateStatsCommand.CreateParameter();
                            userIdParam.ParameterName = "@userId";
                            userIdParam.Value = userId;
                            updateStatsCommand.Parameters.Add(userIdParam);

                            var killsParam = updateStatsCommand.CreateParameter();
                            killsParam.ParameterName = "@kills";
                            killsParam.Value = stats.killCount; 
                            updateStatsCommand.Parameters.Add(killsParam);

                            var lorenotesParam = updateStatsCommand.CreateParameter();
                            lorenotesParam.ParameterName = "@lorenotes";
                            lorenotesParam.Value = stats.noteCount; 
                            updateStatsCommand.Parameters.Add(lorenotesParam);

                            int statsRowsAffected = updateStatsCommand.ExecuteNonQuery();
                            if (statsRowsAffected > 0)
                            {
                                Debug.Log("Статистика (убийства и записки) успешно сохранена");
                            }
                            else
                            {
                                Debug.LogError("Не удалось сохранить статистику в таблице stats");
                            }
                        }

                        HideUI();
                    }
                    else
                    {
                        Debug.LogError($"Пользователь {userName} не найден в БД");
                        HideUI();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка сохранения: {ex.Message}\nStackTrace: {ex.StackTrace}");
            HideUI();
        }
    }

}