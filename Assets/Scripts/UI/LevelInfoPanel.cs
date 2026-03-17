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
            levelNameText.text = info.levelName;

        if (mileageGoalText != null)
            mileageGoalText.text = info.mileageGoal.ToString() + " miles";

        gameObject.SetActive(true);
    }
    
    public void OnStartClicked()
    {
        gameObject.SetActive(false);

        if (GameManager.instance != null)
            GameManager.instance.BeginRun();
    }
}
