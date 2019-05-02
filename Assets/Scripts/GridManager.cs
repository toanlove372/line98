using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    public Grid grid;
    public TileCtrl tilePrefab;

    private GameManager gameManager;
    private int numbCellWidth;
    private int numbCellHeight;

    private TileCtrl[][] tileIndexes;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(GameManager gameManager, int numbCellWidth, int numbCellHeight)
    {
        this.gameManager = gameManager;
        this.numbCellWidth = numbCellWidth;
        this.numbCellHeight = numbCellHeight;
        CreateGrid();
    }

    private void CreateGrid()
    {
        Vector3Int cellPos;
        this.tileIndexes = new TileCtrl[this.numbCellHeight][];

        for (int i = 0; i < this.numbCellHeight; i++)
        {
            this.tileIndexes[i] = new TileCtrl[this.numbCellWidth];
            for (int j = 0; j < this.numbCellWidth; j++)
            {
                cellPos = new Vector3Int(j, i, 0);

                TileCtrl tile = Instantiate<TileCtrl>(this.tilePrefab);
                tile.InitPos(this, cellPos);
                this.tileIndexes[i][j] = tile;
            }
        }

        Vector3 bottomLeftGridPos = new Vector3(
            this.numbCellWidth * (this.grid.cellSize.x + this.grid.cellGap.x) * -0.5f + this.grid.cellSize.x * 0.5f,
            this.numbCellHeight * (this.grid.cellSize.y + this.grid.cellGap.y) * -0.5f + this.grid.cellSize.y * 0.5f);
        this.grid.transform.localPosition = bottomLeftGridPos;
    }

    public Vector3 CellToPos(Vector3Int pos)
    {
        return this.grid.CellToWorld(pos);
    }

    public TileCtrl GetTileByIndex(int x, int y)
    {
        return this.tileIndexes[y][x];
    }

    public void OnTileSelected(TileCtrl tile)
    {

        this.gameManager.OnSelect(tile);
    }
}
