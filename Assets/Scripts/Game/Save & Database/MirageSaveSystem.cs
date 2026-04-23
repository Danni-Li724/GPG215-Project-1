using System.IO;
using UnityEngine;

public class MirageSaveSystem : MonoBehaviour
{
    public static MirageSaveSystem Instance { get; private set; }

    private static string SettingsPath =>
        Path.Combine(Application.persistentDataPath, "mirage_settings.json");

    private static string BestRunPath =>
        Path.Combine(Application.persistentDataPath, "mirage_best_run.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveSettings(SettingsData data)
    {
        WriteJson(SettingsPath, JsonUtility.ToJson(data, true));
    }

    public SettingsData LoadSettingsOrDefault()
    {
        if (TryReadJson(SettingsPath, out string json))
        {
            try
            {
                SettingsData data = JsonUtility.FromJson<SettingsData>(json);
                if (data != null) return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"MirageSaveSystem: settings parse failed: {e.Message}");
            }
        }
        return new SettingsData();
    }


    public void SaveBestRun(RunResultData data)
    {
        WriteJson(BestRunPath, JsonUtility.ToJson(data, true));
    }

    public RunResultData LoadBestRunOrDefault()
    {
        if (TryReadJson(BestRunPath, out string json))
        {
            try
            {
                RunResultData data = JsonUtility.FromJson<RunResultData>(json);
                if (data != null) return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"MirageSaveSystem: best run parse failed: {e.Message}");
            }
        }
        return new RunResultData();
    }
    
    public bool TrySaveIfBest(RunResultData incoming)
    {
        RunResultData current = LoadBestRunOrDefault();
        if (incoming.totalMileage > current.totalMileage)
        {
            incoming.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveBestRun(incoming);
            return true;
        }
        return false;
    }

    private static void WriteJson(string path, string json)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
    }

    private static bool TryReadJson(string path, out string json)
    {
        json = string.Empty;
        if (!File.Exists(path)) return false;
        json = File.ReadAllText(path);
        return true;
    }
}
