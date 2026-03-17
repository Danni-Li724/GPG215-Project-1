using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeUIAnimation : MonoBehaviour
{
    [Header("Life Icons")]
    [SerializeField] private List<GameObject> lifeIcons = new List<GameObject>(5);

    [Header("Anim Obj")]
    [SerializeField] private RectTransform lifeAnimObject;

    [Header("Waypoints")]
    [SerializeField] private List<RectTransform> waypoints = new List<RectTransform>(7); // anim object will end at waypoint 7

    [Header("Timing")]
    [SerializeField] private float animSeconds = 1f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(false);
        }

        if (lifeAnimObject != null)
            lifeAnimObject.gameObject.SetActive(false);
    }

    // lifeCount is 1..5
    public void PlayLifeCollectAnimationAndThenReveal(int lifeCount)
    {
        if (lifeCount <= 0)
            return;

        int index = lifeCount - 1;
        if (index < 0 || index >= lifeIcons.Count)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateAndReveal(index));
    }

    private IEnumerator AnimateAndReveal(int iconIndex)
    {
        if (lifeAnimObject == null || waypoints == null || waypoints.Count < 7)
        {
            RevealIcon(iconIndex);
            yield break;
        }

        // reset anim object to waypoint[0] every time it needs to be played
        lifeAnimObject.gameObject.SetActive(true);
        lifeAnimObject.anchoredPosition = waypoints[0].anchoredPosition;

        float duration = Mathf.Max(0.01f, animSeconds);
        float segmentDuration = duration / 6f; 

        for (int i = 0; i < 6; i++)
        {
            RectTransform a = waypoints[i];
            RectTransform b = waypoints[i + 1];

            if (a == null || b == null)
                continue;

            Vector2 start = a.anchoredPosition;
            Vector2 end = b.anchoredPosition;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / segmentDuration;
                float u = Mathf.Clamp01(t);

                lifeAnimObject.anchoredPosition = Vector2.Lerp(start, end, u);
                yield return null;
            }
        }
        // hide anim object and reveal life icon
        lifeAnimObject.gameObject.SetActive(false);
        RevealIcon(iconIndex);
    }

    private void RevealIcon(int iconIndex)
    {
        if (iconIndex < 0 || iconIndex >= lifeIcons.Count)
            return;

        if (lifeIcons[iconIndex] != null)
            lifeIcons[iconIndex].SetActive(true);
    }
    
    public void HideLifeIconByCount(int lifeCountBeforeSpend)
    {
        // hide from end of index when using life
        int index = lifeCountBeforeSpend - 1;
        if (index < 0 || index >= lifeIcons.Count)
            return;

        if (lifeIcons[index] != null)
            lifeIcons[index].SetActive(false);
    }
}