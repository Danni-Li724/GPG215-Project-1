using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Enemies", fileName = "LevelEnemies")]
public class LevelEnemiesSO : ScriptableObject
{
    [Header("Clasher Enemies")]
    public List<GameObject> clasherEnemies = new List<GameObject>();

    [Header("Ranger Enemies")]
    public List<GameObject> rangerEnemies = new List<GameObject>();

    [Header("Boss")]
    public GameObject boss;
    
    public List<GameObject> AllEnemies()
    {
        List<GameObject> all = new List<GameObject>(clasherEnemies);
        all.AddRange(rangerEnemies);
        return all;
    }
}
