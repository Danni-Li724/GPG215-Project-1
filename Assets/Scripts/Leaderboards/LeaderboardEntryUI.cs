using UnityEngine;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    public void Set(int rank, string playerName, int score)
    {
        if (rankText != null)  rankText.text  = "#" + rank;
        if (nameText != null)  nameText.text  = string.IsNullOrEmpty(playerName) ? "Unknown" : playerName;
        if (scoreText != null) scoreText.text = score + " miles";
    }
}