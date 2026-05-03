using UnityEngine;

public class ControlAreaFlipper : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform controlArea;
    [SerializeField] private RectTransform mileageText;
    public void ApplyHandMode(bool leftHand)
    {
        if (controlArea == null) return;

        Vector3 areaScale = controlArea.localScale;
        areaScale.x = leftHand ? -1f : 1f;
        controlArea.localScale = areaScale;

        if (mileageText != null)
        {
            Vector3 textScale = mileageText.localScale;
            textScale.x = leftHand ? -1f : 1f;
            mileageText.localScale = textScale;
        }
    }

    private void Start()
    {
        if (MirageSaveSystem.Instance != null)
        {
            SettingsData data = MirageSaveSystem.Instance.LoadSettingsOrDefault();
            ApplyHandMode(data.leftHandMode);
        }
    }
}
