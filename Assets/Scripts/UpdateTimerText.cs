using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateTimerText : MonoBehaviour
{
    private TMP_Text timerText;

    void Start()
    {
        timerText = GetComponent<TMP_Text>();
        Debug.Log(timerText);
    }

    public void UpdateTime(float newTime)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Round(newTime).ToString();
        }
    }
}