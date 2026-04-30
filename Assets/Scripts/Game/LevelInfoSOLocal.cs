using UnityEngine;

// Replaces LevelInfoSO. Fully SO-driven — no DB ids, no streaming assets paths.
[CreateAssetMenu(menuName = "Game/Level Info Local", fileName = "LevelInfoLocal")]
public class LevelInfoSOLocal : ScriptableObject
{
    [Header("Identity")]
    public string levelName   = "Level 1";
    public int    mileageGoal = 400;

    [Header("Enemies")]
    public LevelEnemiesSO levelEnemies;

    [Header("Map")]
    public LevelMapSO levelMap;

    [Header("Boss")]
    public GameObject levelBoss;
    public Transform  bossSpawnPos;
    public Transform  bossFinalPos;
    public float      bossMoveSpeed = 4f;

    [Header("Destination")]
    public GameObject levelDestination;
    public Transform  destinationSpawnPos;
    public Transform  destinationFinalPos;
    public float      destinationMoveSpeed = 4f;

    [Header("Notices")]
    public string bossNoticeText      = "Boss Incoming!";
    public float  bossNoticeDuration  = 3f;
    public string arrivalNoticeText   = "You have arrived!";
    public float  arrivalNoticeDuration = 3f;
}
