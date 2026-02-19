using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MileageSystem : MonoBehaviour, ITickable
{
    [Header("Rate")]
    [SerializeField] private float milesPerSecond = 10f;
    [SerializeField] private float milesPerUnit = 1f;
    [Header("UI")]
    [SerializeField] private TMP_Text mileageText;
    [Header("Milestone UI")]
    [SerializeField] private GameObject milestonePopupRoot;
    [SerializeField] private TMP_Text milestonePopupText;
    [SerializeField] private float milestoneShowSeconds = 2f;
    [Header("Milestones")]
    [SerializeField] private List<int> milestoneMiles = new List<int> { 100 };

    private float mileAccumulator;
    private int currentMiles;

    private int nextMilestoneIndex;

    private float popupTimer;
    private bool popupVisible;

    private void Awake()
    {
        if (milestoneMiles != null)
            milestoneMiles.Sort();
        HideMilestone();
        UpdateMileageText();
    }

    public void Tick(float dt)
    {
        if (popupVisible)
        {
            popupTimer -= dt;
            if (popupTimer <= 0f)
                HideMilestone();
        }
        // milleage accumulates by time
        mileAccumulator += milesPerSecond * dt;
        // convert to whole-mile steps (UI ticks by 1)
        while (mileAccumulator >= milesPerUnit)
        {
            mileAccumulator -= milesPerUnit;
            currentMiles += 1;

            UpdateMileageText();
            CheckMilestones();
        }
    }

    private void UpdateMileageText()
    {
        if (mileageText != null)
            mileageText.text = currentMiles.ToString();
    }

    private void CheckMilestones()
    {
        if (milestoneMiles == null || milestoneMiles.Count == 0)
            return;

        if (nextMilestoneIndex >= milestoneMiles.Count)
            return;

        int target = milestoneMiles[nextMilestoneIndex];

        if (currentMiles >= target)
        {
            ShowMilestone(currentMiles);
            nextMilestoneIndex += 1;
        }
    }

    private void ShowMilestone(int miles)
    {
        if (milestonePopupRoot == null || milestonePopupText == null)
            return;

        milestonePopupText.text = miles.ToString() + " miles!";
        milestonePopupRoot.SetActive(true);

        popupVisible = true;
        popupTimer = milestoneShowSeconds;
    }

    private void HideMilestone()
    {
        popupVisible = false;
        popupTimer = 0f;

        if (milestonePopupRoot != null)
            milestonePopupRoot.SetActive(false);
    }
}
