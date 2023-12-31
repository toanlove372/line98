﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCtrl : MonoBehaviour
{
    private GridManager gridManager;
    private Vector3Int cellIndex;
    private BallCtrl ball;
    private bool hasBall;

    public Vector3Int CellIndex { get => cellIndex;}
    public bool HasBall { get => hasBall; set => hasBall = value; }

    public bool HasPreviewBall { get; set; }
    public GridManager GridManager { get => gridManager; set => gridManager = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitPos(GridManager gridManager, Vector3Int cellIndex)
    {
        this.gridManager = gridManager;
        this.cellIndex = cellIndex;

        this.transform.SetParent(this.gridManager.grid.transform);
        this.name = string.Format("Tile ({0}, {1})", this.cellIndex.x, this.cellIndex.y);
        Vector3 tilePos = this.gridManager.grid.CellToLocal(this.cellIndex);
        this.transform.localPosition = tilePos;
    }

    public void OnBallPreview(BallCtrl ball)
    {
        this.HasPreviewBall = true;

        this.ball = ball;
        this.ball.transform.position = this.gridManager.CellToPos(this.CellIndex);
        this.ball.transform.localScale = new Vector3(0.4f, 0.4f);
    }

    public void MoveBallPreview(TileCtrl newTile)
    {
        newTile.OnBallPreview(this.ball);

        this.HasPreviewBall = false;
        this.ball = null;
    }

    public void OnBallInPreview()
    {
        this.HasPreviewBall = false;

        OnBallIn(this.ball);
        //TODO: show ball effect
        this.ball.transform.localScale = Vector3.one;
    }

    public void OnBallIn(BallCtrl ball, List<Vector2Int> path = null, Action onDone = null)
    {
        this.ball = ball;
        this.HasBall = true;
        this.ball.OnBallIn(this, path, onDone);
    }

    public BallCtrl OnBallOut()
    {
        BallCtrl ball = this.ball;
        this.ball = null;
        this.HasBall = false;
        return ball;
    }

    public void DestroyBall()
    {
        if (this.ball == null)
        {
            return;
        }

        Destroy(this.ball.gameObject);
        this.ball = null;
        this.HasBall = false;

        GameObject explosion = Instantiate<GameObject>(EffectPool.Instance.ballExplosion);
        explosion.transform.position = this.transform.position;

        SoundManager.Instance.PlaySound("BallExplode");
    }

    public void ShowSelectedEffect()
    {
        this.ball.transform.localScale = new Vector3(0.75f, 0.75f);

        SoundManager.Instance.PlaySound("ButtonClicked");
    }

    public void HideSelectedEffect()
    {
        this.ball.transform.localScale = Vector3.one;
    }

    public void Reset()
    {
        DestroyBall();
        this.HasPreviewBall = false;
    }

    private void OnSelected()
    {
        this.gridManager.OnTileSelected(this);
    }

    private void OnMouseDown()
    {
        OnSelected();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position, string.Format("{0},{1}", this.cellIndex.x, this.cellIndex.y), style);
    }
#endif
}
