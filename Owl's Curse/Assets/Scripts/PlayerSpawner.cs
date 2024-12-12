using UnityEngine;
using System.Linq;

public class PlayerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointData
    {
        public string savepointName;
        public Transform spawnPoint;
        public GameObject frame;
    }

    [SerializeField] private SpawnPointData[] spawnPoints; // Только фреймы с точками спавна
    [SerializeField] private SpawnPointData defaultSpawnPoint;
    [SerializeField] private GameObject[] allFrames; // Все фреймы на сцене

    void Start()
    {
        // Проверяем настройки
        if (allFrames == null || allFrames.Length == 0)
        {
            Debug.LogError("Не заданы фреймы сцены!");
            return;
        }

        if (defaultSpawnPoint == null || defaultSpawnPoint.spawnPoint == null || defaultSpawnPoint.frame == null)
        {
            Debug.LogError("Не настроена стартовая точка спавна!");
            return;
        }

        // Деактивируем все фреймы
        DeactivateAllFrames();

        string saveData = PlayerPrefs.GetString("LastSave", null);
        if (!string.IsNullOrEmpty(saveData))
        {
            LoadPlayerFromSave(saveData);
        }
        else
        {
            Debug.Log("Сохранение не найдено, используем стартовую точку спавна");
            SpawnPlayerAtDefault();
        }
    }

    private void DeactivateAllFrames()
    {
        foreach (var frame in allFrames)
        {
            if (frame != null)
            {
                frame.SetActive(false);
                Debug.Log($"Деактивирован фрейм: {frame.name}");
            }
        }
    }

    private void LoadPlayerFromSave(string saveData)
    {
        try
        {
            string[] saveParams = saveData.Split(',');
            if (saveParams.Length >= 4)
            {
                string savepointName = saveParams[0];
                int maxHealth = int.Parse(saveParams[1]);
                int attackDamage = int.Parse(saveParams[2]);
                float sanity = float.Parse(saveParams[3]);

                // Находим данные точки спавна
                SpawnPointData spawnData = FindSpawnPoint(savepointName);
                if (spawnData != null && spawnData.frame != null)
                {
                    // Активируем нужный фрейм
                    spawnData.frame.SetActive(true);
                    Debug.Log($"Активирован фрейм для точки сохранения: {savepointName}");

                    SpawnAndInitializePlayer(spawnData.spawnPoint, maxHealth, attackDamage, sanity);
                }
                else
                {
                    Debug.LogWarning($"Точка спавна {savepointName} не найдена или её фрейм не задан, используем стартовую точку");
                    SpawnPlayerAtDefault();
                }
            }
            else
            {
                Debug.LogError("Некорректный формат данных сохранения");
                SpawnPlayerAtDefault();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при загрузке сохранения: {e.Message}");
            SpawnPlayerAtDefault();
        }
    }

    private void SpawnPlayerAtDefault()
    {
        defaultSpawnPoint.frame.SetActive(true);
        Debug.Log($"Активирован стартовый фрейм: {defaultSpawnPoint.frame.name}");
        SpawnAndInitializePlayer(defaultSpawnPoint.spawnPoint);
    }

    public void RespawnPlayerAtLastSave()
    {
        string saveData = PlayerPrefs.GetString("LastSave");
        if (!string.IsNullOrEmpty(saveData))
        {
            string[] saveParams = saveData.Split(',');
            if (saveParams.Length >= 1)
            {
                string savepointName = saveParams[0];
                SpawnPointData spawnData = FindSpawnPoint(savepointName);

                if (spawnData != null)
                {
                    // Деактивируем все фреймы
                    DeactivateAllFrames();

                    // Активируем нужный фрейм
                    spawnData.frame.SetActive(true);

                    // Перемещаем игрока
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = spawnData.spawnPoint.position;
                        Debug.Log($"Игрок возрожден в точке: {savepointName}");
                    }
                }
                else
                {
                    Debug.LogError($"Точка возрождения {savepointName} не найдена");
                }
            }
        }
    }

    private void SpawnAndInitializePlayer(Transform spawnPoint, int? maxHealth = null, int? attackDamage = null, float? sanity = null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPoint.position;
            PlayerScript playerScript = player.GetComponent<PlayerScript>();

            if (playerScript != null)
            {
                if (maxHealth.HasValue) playerScript.maxHealth = maxHealth.Value;
                if (maxHealth.HasValue) playerScript.currentHealth = maxHealth.Value;
                if (attackDamage.HasValue) playerScript.attackDamage = attackDamage.Value;
                if (sanity.HasValue) playerScript.currentSanity = sanity.Value;

                // Обновляем UI
                if (playerScript.Bar != null)
                {
                    playerScript.Bar.fillAmount = 1f;
                }
                if (playerScript.sanityBar != null)
                {
                    playerScript.sanityBar.fillAmount = sanity.HasValue ?
                        sanity.Value / playerScript.maxSanity : 1f;
                }

                Debug.Log($"Игрок размещен в позиции: {spawnPoint.position}");
            }
        }
        else
        {
            Debug.LogError("Игрок не найден на сцене!");
        }
    }

    private SpawnPointData FindSpawnPoint(string savepointName)
    {
        return spawnPoints.FirstOrDefault(sp => sp.savepointName == savepointName);
    }
}