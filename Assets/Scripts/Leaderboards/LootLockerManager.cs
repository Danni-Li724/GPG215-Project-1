using System;
using System.Collections;
using UnityEngine;
using LootLocker.Requests;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }

    [Header("Leaderboard")]
    [SerializeField] private string leaderboardKey = "mileage_leaderboard";
    [SerializeField] private int topScoresCount = 10;

    private bool sessionStarted = false;
    private string playerName = "Unknown";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartGuestSession());
    }

    private IEnumerator StartGuestSession()
    {
        bool done = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                sessionStarted = true;
                Debug.Log("lootLocker guest session started. Player ID: " + response.player_id);
            }
            else
            {
                Debug.LogWarning("lootLocker session failed: " + response.errorData);
            }
            done = true;
        });

        yield return new WaitUntil(() => done);
    }

    public void SetPlayerName(string name, Action onDone = null)
    {
        if (!sessionStarted) { onDone?.Invoke(); return; }
        playerName = name;

        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            if (!response.success)
                Debug.LogWarning("couldn't set player name: " + response.errorData);
            onDone?.Invoke();
        });
    }

    public void SubmitScore(int score, Action onSuccess = null, Action onFail = null)
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("lootLocker: no active session.");
            onFail?.Invoke();
            return;
        }

        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log("score submitted. Rank: " + response.rank);
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogWarning("score submit failed: " + response.errorData);
                onFail?.Invoke();
            }
        });
    }

    public void GetTopScores(Action<LootLockerLeaderboardMember[]> onResult)
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("lootLocker: no active session.");
            onResult?.Invoke(null);
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, topScoresCount, 0, (response) =>
        {
            if (response.success)
                onResult?.Invoke(response.items);
            else
            {
                Debug.LogWarning("failed to show leaderboard: " + response.errorData);
                onResult?.Invoke(null);
            }
        });
    }

    public bool IsSessionReady => sessionStarted;
}