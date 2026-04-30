using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class HighScoreNotifier : MonoBehaviour
{
    [SerializeField] private float showSeconds = 2.5f;

    private TMP_Text label;
    private Coroutine routine;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        label.enabled = false;
    }
    
    public void Show(int newBestMileage)
    {
        label.text = $"New Best: {newBestMileage} miles!";
        if (routine != null) StopCoroutine(routine);
        // routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(showSeconds);
        gameObject.SetActive(false);
        routine = null;
    }
}