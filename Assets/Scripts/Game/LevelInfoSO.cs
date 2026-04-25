using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Info", fileName = "LevelInfo")]
public class LevelInfoSO : ScriptableObject
{
    public string levelName = "";
    public int mileageGoal = 100;
    
    public int sqlLevelId = 0;

    [Header("Boss")]
    public GameObject levelBoss;
    public Transform bossSpawnPos;
    public Transform bossFinalPos;
    public float bossMoveSpeed = 4f;

    [Header("Destination")]
    public GameObject levelDestination;
    public Transform destinationSpawnPos;
    public Transform destinationFinalPos;
    public float destinationMoveSpeed = 4f;

    [Header("Notices")] 
    public string bossNoticeText;
    public float bossNoticeDuration = 3f;
    public string arrivalNoticeText;
    public float arrivalNoticeDuration = 3f;
    
    [Header("DLC")]
    public string dlcPackId;
}