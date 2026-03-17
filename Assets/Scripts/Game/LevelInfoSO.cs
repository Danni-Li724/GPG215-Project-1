using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Info", fileName = "LevelInfo")]
public class LevelInfoSO : ScriptableObject
{
    public string levelName = "Level 1";
    public int mileageGoal = 100000;
}