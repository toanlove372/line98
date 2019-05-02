using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Gameplay gameplay;
    public GameUI gameUI;

    public int score;

    private const int ScoreUnit = 10;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        this.gameplay.onScore += OnScore;
        this.gameUI.onRestartClicked += Restart;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        this.gameplay.Init();
        this.gameUI.RunTimer();
    }

    private void OnScore()
    {
        this.score += ScoreUnit;
        this.gameUI.ShowScore(this.score);
    }

    private void Restart()
    {
        this.gameplay.Restart();
    }
}
