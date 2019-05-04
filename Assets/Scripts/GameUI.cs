using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Camera mainCam;

    public Text scoreText;
    public Text timeText;
    
    public Transform menuPopup;
    public Transform endGamePopup;
    public Text endGameScoreText;
    public Text endGameHighScoreText;

    private int timeRunner;
    private Coroutine corTimer;

    private bool isLandscapeLayoutSet;
    private bool isPortraitLayoutSet;

    public Action onRestartClicked;

    // Start is called before the first frame update
    void Start()
    {
        if ((Input.deviceOrientation == DeviceOrientation.Portrait) 
            && (Screen.orientation == ScreenOrientation.Portrait)
            && this.isPortraitLayoutSet == false)
        {
            UsePortraitLayout();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width < Screen.height)
        {
            UsePortraitLayout();
        }
        else
        {
            UseLandscapeLeftLayout();
        }

        //if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) && (Screen.orientation != ScreenOrientation.LandscapeLeft))
        //{
        //    Screen.orientation = ScreenOrientation.LandscapeLeft;
        //    UseLandscapeLeftLayout();
        //}

        //if ((Input.deviceOrientation == DeviceOrientation.Portrait) && (Screen.orientation != ScreenOrientation.Portrait))
        //{
        //    Screen.orientation = ScreenOrientation.Portrait;
        //    UsePortraitLayout();
        //}
    }

    private void UseLandscapeLeftLayout()
    {
        if (this.isLandscapeLayoutSet)
        {
            return;
        }

        this.isLandscapeLayoutSet = true;
        this.isPortraitLayoutSet = false;

        this.mainCam.orthographicSize = 5;
    }

    private void UsePortraitLayout()
    {
        if (this.isPortraitLayoutSet)
        {
            return;
        }

        this.isPortraitLayoutSet = true;
        this.isLandscapeLayoutSet = false;

        this.mainCam.orthographicSize = 9;
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

        SoundManager.Instance.PlaySound("ButtonClicked");
    }

    public void OnEndGame(int score, int highScore)
    {
        this.endGamePopup.gameObject.SetActive(true);
        this.endGameScoreText.text = score.ToString();
        this.endGameHighScoreText.text = string.Format("High score: {0}", highScore);

        SoundManager.Instance.PlaySound("GameOver");
    }

    public void OnRestartClicked()
    {
        this.endGamePopup.gameObject.SetActive(false);
        this.scoreText.text = "0";
        this.timeText.text = "0";

        OnSetMenuPopupVisible(false);
        StopTimer();

        if (this.onRestartClicked != null)
        {
            this.onRestartClicked();
        }
    }
}
