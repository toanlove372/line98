using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text scoreText;
    public Text timeText;
    public Transform menuPopup;

    private int timeRunner;
    private Coroutine corTimer;

    public Action onRestartClicked;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowScore(int score)
    {
        this.scoreText.text = score.ToString();
    }

    public void RunTimer()
    {
        this.corTimer = StartCoroutine(IERunTimer());
    }

    public void StopTimer()
    {
        StopCoroutine(this.corTimer);
    }

    private IEnumerator IERunTimer()
    {
        this.timeRunner = 0;

        WaitForSeconds waitOneSec = new WaitForSeconds(1);
        int minute;
        int second;
        while (true)
        {
            minute = this.timeRunner / 60;
            second = this.timeRunner % 60;

            this.timeText.text = string.Format("{0}:{1}", minute, second);

            this.timeRunner++;
            yield return waitOneSec;
        }
    }

    public void OnSetMenuPopupVisible(bool isVisible)
    {
        this.menuPopup.gameObject.SetActive(isVisible);
    }

    public void OnRestartClicked()
    {
        OnSetMenuPopupVisible(false);

        if (this.onRestartClicked != null)
        {
            this.onRestartClicked();
        }
    }
}
