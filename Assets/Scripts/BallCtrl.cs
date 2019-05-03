using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallCtrl : MonoBehaviour
{
    public BallColor color;

    private Sequence moveSequence;

    private static float moveTime = 0.35f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBallIn(TileCtrl tile, List<Vector2Int> path = null, Action onDone = null)
    {
        if (path != null)
        {
            Transform trail = EffectPool.Instance.ballTrails[(int)this.color - 1].transform;
            trail.SetParent(this.transform);
            trail.localPosition = Vector3.zero;
            trail.gameObject.SetActive(true);

            float stepDuration = moveTime / path.Count;
            this.moveSequence = DOTween.Sequence();
            this.moveSequence.SetAutoKill(true);
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 pos = tile.GridManager.CellToPos(new Vector3Int(path[i].x, path[i].y, 0));
                this.moveSequence.Append(this.transform.DOMove(pos, stepDuration).SetEase(Ease.Linear));
            }
            this.moveSequence.OnComplete(()=>
            {
                trail.SetParent(EffectPool.Instance.transform);
                trail.gameObject.SetActive(false);

                if (onDone != null)
                {
                    onDone();
                }
            });
        }
        else
        {
            this.transform.position = tile.GridManager.CellToPos(tile.CellIndex);
            this.transform.localScale = new Vector3(0.5f, 0.5f);
            this.transform.DOScale(1, 0.25f);
        }
    }
}
