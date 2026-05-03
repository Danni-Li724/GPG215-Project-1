using TMPro;
using UnityEngine;

public class LevelInfoPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text mileageGoalText;

    public void Show(LevelInfoSO info)
    {
        if (info == null)
            return;

        if (levelNameText != null)
            levelNameText.text = "You are going to: " + info.levelName;

        if (mileageGoalText != null)
            mileageGoalText.text = "Distance: " + info.mileageGoal.ToString() + " miles";

        gameObject.SetActive(true);
    }
    
    public void OnStartClicked()
    {
        gameObject.SetActive(false);
        if (GameManager.instance != null)
            GameManager.instance.BeginRun();
    }
}
