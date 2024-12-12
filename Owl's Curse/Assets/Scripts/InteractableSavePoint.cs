using Mono.Data.Sqlite;
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
            // Увеличиваем максимальные значения
            playerScript.maxHealth += (int)healthBonus;
            playerScript.attackDamage += (int)damageBonus;

            // Восстанавливаем здоровье до максимума
            playerScript.currentHealth = playerScript.maxHealth;

            // Обновляем полосу здоровья
            if (playerScript.Bar != null)
            {
                playerScript.Bar.fillAmount = (float)playerScript.currentHealth / playerScript.maxHealth;
            }

            // Устанавливаем рассудок
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

                        string updateQuery = @"
                            INSERT OR REPLACE INTO saves (userId, currSave) 
                            VALUES (@userId, @currSave);";

                        using (IDbCommand updateCommand = dbConnection.CreateCommand())
                        {
                            updateCommand.CommandText = updateQuery;

                            var userIdParam = updateCommand.CreateParameter();
                            userIdParam.ParameterName = "@userId";
                            userIdParam.Value = userId;
                            updateCommand.Parameters.Add(userIdParam);

                            var saveParam = updateCommand.CreateParameter();
                            saveParam.ParameterName = "@currSave";
                            saveParam.Value = saveData;
                            updateCommand.Parameters.Add(saveParam);

                            int rowsAffected = updateCommand.ExecuteNonQuery();
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

                            HideUI();
                        }
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