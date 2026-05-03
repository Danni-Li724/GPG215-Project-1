using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;

public class LeaderboardPanelUI : MonoBehaviour
{
    [Header("Entry")]
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject entryPrefab;

    [Header("Name Input")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button setNameButton;

    [Header("Buttons")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button closeButton;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    {
        gameObject.SetActive(false);
        refreshButton.onClick.AddListener(FetchAndDisplay);
        if (setNameButton != null)
            setNameButton.onClick.AddListener(OnSetNamePressed);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    public void FetchAndDisplay()
    {
        if (LootLockerManager.Instance == null || !LootLockerManager.Instance.IsSessionReady)
        {
            SetStatus("Not connected.");
            return;
        }

        SetStatus("Loading...");

        LootLockerManager.Instance.GetTopScores((entries) =>
        {
            ClearEntries();

            if (entries == null || entries.Length == 0)
            {
                SetStatus("No scores yet. Be the first!");
                return;
            }

            SetStatus("");
            foreach (var entry in entries)
                CreateEntry(entry.rank, entry.player.name, entry.score);
        });
    }

    private void OnSetNamePressed()
    {
        string name = nameInputField != null ? nameInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) return;

        SetStatus("Saving name...");
        LootLockerManager.Instance.SetPlayerName(name, () =>
        {
            SetStatus("Name saved!");
        });
    }

    private void CreateEntry(int rank, string playerName, int score)
    {
        GameObject go = Instantiate(entryPrefab, entryContainer);
        LeaderboardEntryUI ui = go.GetComponent<LeaderboardEntryUI>();
        if (ui != null)
            ui.Set(rank, playerName, score);
    }

    private void ClearEntries()
    {
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);
    }
}