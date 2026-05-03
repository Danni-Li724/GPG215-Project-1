using System;

[Serializable]
public class RunResultData
{
    public int totalMileage;
    public int enemiesKilled;
    public int livesLost;
    public string timestamp;
}

[Serializable]
public class SettingsData
{
    public float musicVolume = 1f;
    public float sfxVolume   = 1f;
    public bool proceduralBgEnabled = true;
    public bool leftHandMode = false;
}
