using System;
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
        this.ball.transform.localScale = new Vector3(0.25f, 0.25f);
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

        if (path != null)
        {
            StartCoroutine(IEBallIn(path, onDone));
        }
        else
        {
            this.ball.transform.position = this.gridManager.CellToPos(this.CellIndex);
        }
    }

    private IEnumerator IEBallIn(List<Vector2Int> path, Action onDone)
    {
        foreach (var point in path)
        {
            this.ball.transform.position = this.gridManager.CellToPos(new Vector3Int(point.x, point.y, 0));
            yield return new WaitForSeconds(0.15f);
        }

        onDone();
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
            Debug.LogError("Null ball");
            return;
        }

        Destroy(this.ball.gameObject);
        this.ball = null;
        this.HasBall = false;
    }

    public void ShowSelectedEffect()
    {
        this.ball.transform.localScale = new Vector3(0.75f, 0.75f);
    }

    public void HideSelectedEffect()
    {
        this.ball.transform.localScale = Vector3.one;
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
