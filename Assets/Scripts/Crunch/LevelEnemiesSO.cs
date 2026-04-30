using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Enemies", fileName = "LevelEnemies")]
public class LevelEnemiesSO : ScriptableObject
{
    [Header("Clasher Enemies")]
    [Tooltip("All clasher-type enemy prefabs for this level. Order matches EnemyPool pool indices.")]
    public List<GameObject> clasherEnemies = new List<GameObject>();

    [Header("Ranger Enemies")]
    [Tooltip("All ranger-type enemy prefabs for this level.")]
    public List<GameObject> rangerEnemies = new List<GameObject>();

    [Header("Boss")]
    [Tooltip("The boss prefab for this level.")]
    public GameObject boss;
    
    public List<GameObject> AllEnemies()
    {
        List<GameObject> all = new List<GameObject>(clasherEnemies);
        all.AddRange(rangerEnemies);
        return all;
    }
}
