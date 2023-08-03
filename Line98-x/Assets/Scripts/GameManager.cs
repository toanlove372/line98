using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Gameplay gameplay;
    public GameUI gameUI;

    public int score;

    private int highScore;

    private const int ScoreUnit = 10;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        this.gameplay.onScore += OnScore;
        this.gameplay.onBallCountChanged += OnBallCountChanged;
        this.gameplay.onEndGame += OnEndGame;
        this.gameUI.onRestartClicked += Restart;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        //this.highScore = PlayerPrefs.GetInt("HighScore", 0);

        this.gameplay.Init();
        this.gameUI.RunTimer();
    }

    private void OnScore()
    {
        this.score += ScoreUnit;
        this.gameUI.ShowScore(this.score);
    }

    private void OnBallCountChanged(int value)
    {
        this.gameUI.ShowBallCount(value);
    }

    private void OnEndGame()
    {
        this.gameUI.StopTimer();

        if (this.score > this.highScore)
        {
            this.highScore = this.score;
            //PlayerPrefs.SetInt("HighScore", this.highScore);
        }

        this.gameUI.OnEndGame(this.score, this.highScore);
    }

    private void Restart()
    {
        this.score = 0;
        this.gameUI.RunTimer();
        this.gameplay.Restart();
    }
}
