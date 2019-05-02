using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultilities;
using Ultilities.AStar;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BallColor
{
    Blank,
    Red,
    Green,
    Blue
}

public enum SelectionState
{
    Selectable,
    SelectFirstDone,
    SelectSecondDone,
    UnSelectable
}

public class Gameplay : MonoBehaviour
{
    public int numbCellWidth;
    public int numbCellHeight;
    public GridManager gridManager;

    public BallCtrl[] ballColors;

    private int[][] dataIndexes;
    private List<int> blankTiles;
    private int totalTileCount;

    private int[] previewBallIndexes;
    private int[] previewBallColors;
    private BallCtrl[] previewBalls;
    private SelectionState selectState;
    private TileCtrl firstTileSelectedIndex;
    private TileCtrl secondTileSelectedIndex;
    private Grid2D aStarGrid;
    private AStar aStar;

    private const int MinPointToMatch = 3;

    public int[][] DataIndexes { get => dataIndexes;}

    public System.Action onScore;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        this.selectState = SelectionState.UnSelectable;

        this.totalTileCount = this.numbCellWidth * this.numbCellHeight;
        this.blankTiles = new List<int>(this.totalTileCount);
        this.dataIndexes = new int[this.numbCellHeight][];
        for (int i = 0; i < this.dataIndexes.Length; i++)
        {
            this.dataIndexes[i] = new int[this.numbCellWidth];
            for (int j = 0; j < this.numbCellWidth; j++)
            {
                this.blankTiles.Add(i * this.numbCellHeight + j);
                this.dataIndexes[i][j] = 0;
            }
        }

        this.aStarGrid = new Grid2D(this.numbCellWidth, this.numbCellHeight, 0, 0, 1, 1);
        this.aStar = new AStar(this.aStarGrid.Start, this.aStarGrid.Goal);

        this.gridManager.Init(this, this.numbCellWidth, this.numbCellHeight);

        //init balls
        CreateFirstBalls();

        CreateBalls();

        StartGame();
    }

    private void CreateFirstBalls()
    {
        this.previewBallIndexes = this.blankTiles.GetRandom<int>(3);
        this.previewBallColors = new int[3]
        {
            UnityEngine.Random.Range(1, 4),
            UnityEngine.Random.Range(1, 4),
            UnityEngine.Random.Range(1, 4),
        };
        this.previewBalls = new BallCtrl[3];
        for (int i = 0; i < this.previewBallIndexes.Length; i++)
        {
            int xPos = this.previewBallIndexes[i] % this.numbCellWidth;
            int yPos = this.previewBallIndexes[i] / this.numbCellHeight;
            int colorIndex = UnityEngine.Random.Range(1, 4);
            this.previewBallColors[i] = colorIndex;

            BallCtrl ball = Instantiate<BallCtrl>(this.ballColors[colorIndex - 1]);
            ball.color = (BallColor)colorIndex;
            TileCtrl tile = this.gridManager.GetTileByIndex(xPos, yPos);
            tile.OnBallPreview(ball);

            this.previewBalls[i] = ball;
        }
    }

    private void CreateBalls()
    {
        int[] newBallIndexes = this.previewBallIndexes;// this.blankTiles.GetRandom<int>(3);

        for (int i = 0; i < newBallIndexes.Length; i++)
        {
            this.blankTiles.Remove(newBallIndexes[i]);

            int xPos = newBallIndexes[i] % this.numbCellWidth;
            int yPos = newBallIndexes[i] / this.numbCellHeight;
            int colorIndex = this.previewBallColors[i];// UnityEngine.Random.Range(1, 4);
            this.dataIndexes[yPos][xPos] = colorIndex;

            this.aStarGrid.SetWall(true, xPos, yPos);

            TileCtrl tile = this.gridManager.GetTileByIndex(xPos, yPos);
            tile.OnBallInPreview();
        }

        //create preview balls
        this.previewBallIndexes = this.blankTiles.GetRandom<int>(3);

        if (this.previewBallIndexes.Length < 3)
        {
            EndGame();
        }
        else
        {
            for (int i = 0; i < this.previewBallIndexes.Length; i++)
            {
                int xPos = this.previewBallIndexes[i] % this.numbCellWidth;
                int yPos = this.previewBallIndexes[i] / this.numbCellHeight;
                int colorIndex = UnityEngine.Random.Range(1, 4);
                this.previewBallColors[i] = colorIndex;

                BallCtrl ball = Instantiate<BallCtrl>(this.ballColors[colorIndex - 1]);
                ball.color = (BallColor)colorIndex;
                TileCtrl tile = this.gridManager.GetTileByIndex(xPos, yPos);
                tile.OnBallPreview(ball);

                this.previewBalls[i] = ball;
            }
        }
    }

    private void StartGame()
    {
        this.selectState = SelectionState.Selectable;
    }

    private void EndGame()
    {
        Debug.LogError("End game");
        this.selectState = SelectionState.UnSelectable;
    }

    public void Restart()
    {
        this.selectState = SelectionState.UnSelectable;

        this.blankTiles.Clear();
        this.dataIndexes = new int[this.numbCellHeight][];
        for (int i = 0; i < this.dataIndexes.Length; i++)
        {
            this.dataIndexes[i] = new int[this.numbCellWidth];
            for (int j = 0; j < this.numbCellWidth; j++)
            {
                this.blankTiles.Add(i * this.numbCellHeight + j);
                this.dataIndexes[i][j] = 0;
            }
        }

        this.aStarGrid.Reset();
        this.aStar.Reset(this.aStarGrid.Start, this.aStarGrid.Goal);
        this.gridManager.Reset();

        //init balls
        CreateFirstBalls();

        CreateBalls();

        StartGame();
    }

    private void AddScore()
    {
        if (this.onScore != null)
        {
            this.onScore();
        }
    }

    public void OnSelect(TileCtrl tile)
    {
        Vector3Int cellIndex = tile.CellIndex;

        switch (this.selectState)
        {
            case SelectionState.Selectable:
                if (tile.HasBall)
                {
                    this.firstTileSelectedIndex = tile;
                    this.selectState = SelectionState.SelectFirstDone;
                    tile.ShowSelectedEffect();
                }
                break;
            case SelectionState.SelectFirstDone:
                if (tile.HasBall == false)
                {
                    List<Vector2Int> path;
                    if (CanMoveBall(tile, out path))
                    {
                        this.secondTileSelectedIndex = tile;
                        this.selectState = SelectionState.SelectSecondDone;

                        this.firstTileSelectedIndex.HideSelectedEffect();
                        OnMoveBall(path);
                    }
                    
                }
                else
                {
                    this.selectState = SelectionState.Selectable;
                    this.firstTileSelectedIndex.HideSelectedEffect();
                    this.firstTileSelectedIndex = null;
                }

                break;
            case SelectionState.SelectSecondDone:
                break;
            case SelectionState.UnSelectable:
                break;
        }
    }

    private bool CanMoveBall(TileCtrl tile, out List<Vector2Int> path)
    {
        path = null;
        Vector3Int cellIndex = tile.CellIndex;

        //AStar check
        Vector3Int firstSelected = this.firstTileSelectedIndex.CellIndex;
        Vector3Int secondSelected = tile.CellIndex;

        this.aStarGrid.SetStart(firstSelected.x, firstSelected.y);
        this.aStarGrid.SetGoal(secondSelected.x, secondSelected.y);

        this.aStar.Reset(this.aStarGrid.Start, this.aStarGrid.Goal);
        State result = aStar.Run();
        //Debug.Log(result);
        IEnumerable<INode> pathNode = this.aStar.GetPath();
        //Debug.Log(this.aStarGrid.Print(pathNode));
        this.aStarGrid.Reset();
        if (result == State.Failed)
        {
            return false;
        }

        path = new List<Vector2Int>();
        GridNode gridNode;
        foreach(var node in pathNode)
        {
            gridNode = (GridNode)node;
            Vector2Int vec = new Vector2Int(gridNode.X, gridNode.Y);
            path.Add(vec);
        }

        return true;
    }

    private void OnMoveBall(List<Vector2Int> path)
    {
        BallCtrl ball = this.firstTileSelectedIndex.OnBallOut();

        //check has preview ball
        for (int i = 0; i < this.previewBallIndexes.Length; i++)
        {
            int xPos = this.previewBallIndexes[i] % this.numbCellWidth;
            int yPos = this.previewBallIndexes[i] / this.numbCellHeight;

            if (this.secondTileSelectedIndex.CellIndex.x == xPos && this.secondTileSelectedIndex.CellIndex.y == yPos)
            {
                //move preview ball to near by position
                List<Vector2Int> availableList = FindNearbyAvailable(xPos, yPos);
                Vector2Int newPoint = availableList[UnityEngine.Random.Range(0, availableList.Count)];
                this.previewBallIndexes[i] = newPoint.y * this.numbCellWidth + newPoint.x;
                TileCtrl tileOriginal = this.gridManager.GetTileByIndex(xPos, yPos);
                TileCtrl tileNew = this.gridManager.GetTileByIndex(newPoint.x, newPoint.y);
                tileOriginal.MoveBallPreview(tileNew);
            }
        }

        //if (this.secondTileSelectedIndex.HasPreviewBall)
        //{
        //    this.secondTileSelectedIndex.DestroyBall();
        //}

        this.secondTileSelectedIndex.OnBallIn(ball, path, ()=>
        {
            Vector3Int firstSelected = this.firstTileSelectedIndex.CellIndex;
            Vector3Int secondSelected = this.secondTileSelectedIndex.CellIndex;
            int current = this.dataIndexes[firstSelected.y][firstSelected.x];
            this.dataIndexes[firstSelected.y][firstSelected.x] = 0;
            this.dataIndexes[secondSelected.y][secondSelected.x] = current;
            this.blankTiles.Add(firstSelected.y * this.numbCellWidth + firstSelected.x);
            this.blankTiles.Remove(secondSelected.y * this.numbCellWidth + secondSelected.x);

            this.aStarGrid.SetWall(false, firstSelected.x, firstSelected.y);
            this.aStarGrid.SetWall(true, secondSelected.x, secondSelected.y);

            this.selectState = SelectionState.Selectable;

            for (int i = 0; i < this.dataIndexes.Length; i++)
            {
                for (int j = 0; j < this.numbCellWidth; j++)
                {
                    CheckTileMatched(i, j, 0, 1);
                    CheckTileMatched(i, j, 1, 0);
                    CheckTileMatched(i, j, 1, 1);
                    CheckTileMatched(i, j, -1, 1);
                }
            }

            CreateBalls();

            for (int i = 0; i < this.dataIndexes.Length; i++)
            {
                for (int j = 0; j < this.numbCellWidth; j++)
                {
                    CheckTileMatched(i, j, 0, 1);
                    CheckTileMatched(i, j, 1, 0);
                    CheckTileMatched(i, j, 1, 1);
                    CheckTileMatched(i, j, -1, 1);
                }
            }
        });
        

    }

    private void CheckTileMatched(int y, int x, int xDirection, int yDirection)
    {
        int colorIndex = this.dataIndexes[y][x];
        if (colorIndex == 0)
        {
            return;
        }

        int currentMatched = 0;
        int checkX = x;
        int checkY = y;
        while (true)
        {
            checkX += xDirection;
            if (checkX < 0 || checkX >= this.numbCellWidth)
            {
                break;
            }

            checkY += yDirection;
            if (checkY >= this.numbCellHeight)
            {
                break;
            }

            int colorCheck = this.dataIndexes[checkY][checkX];
            if (colorCheck == colorIndex)
            {
                currentMatched++;
            }
            else
            {
                break;
            }
        }

        if (currentMatched > MinPointToMatch)
        {
            for (int i = 0; i <= currentMatched; i++)
            {
                this.dataIndexes[y + i * yDirection][x + i * xDirection] = 0;
                this.blankTiles.Add((y + i * yDirection) * this.numbCellWidth + x + i * xDirection);
                this.aStarGrid.SetWall(false, x + i * xDirection, y + i * yDirection);

                TileCtrl tile = this.gridManager.GetTileByIndex(x + i * xDirection, y + i * yDirection);
                tile.DestroyBall();
            }

            AddScore();
        }
    }

    private List<Vector2Int> FindNearbyAvailable(int x, int y)
    {
        List<Vector2Int> availableList = new List<Vector2Int>();

        if (x != 0)
        {
            if (this.dataIndexes[y][x - 1] == 0 && IsPreviewContain(x - 1, y) == false)
            {
                availableList.Add(new Vector2Int(x - 1, y));
            }
        }

        if (x < this.numbCellWidth - 1)
        {
            if (this.dataIndexes[y][x + 1] == 0 && IsPreviewContain(x + 1, y) == false)
            {
                availableList.Add(new Vector2Int(x + 1, y));
            }
        }

        if (y != 0)
        {
            if (this.dataIndexes[y - 1][x] == 0 && IsPreviewContain(x, y - 1) == false)
            {
                availableList.Add(new Vector2Int(x, y - 1));
            }
        }

        if (y < this.numbCellHeight - 1)
        {
            if (this.dataIndexes[y + 1][x] == 0 && IsPreviewContain(x, y + 1) == false)
            {
                availableList.Add(new Vector2Int(x, y + 1));
            }
        }

        if (availableList.Count == 0)
        {
            availableList.Add(new Vector2Int(firstTileSelectedIndex.CellIndex.x, firstTileSelectedIndex.CellIndex.y));
        }

        return availableList;
    }

    private bool IsPreviewContain(int x, int y)
    {
        for (int i = 0; i < this.previewBallIndexes.Length; i++)
        {
            if (y * this.numbCellWidth + x == this.previewBallIndexes[i])
            {
                return true;
            }
        }

        return false;
    }

    [ContextMenu("Test")]
    public void Test()
    {
        List<int> test = new List<int>() { 1, 2, 3 };
        int[] randTest = test.GetRandom<int>(4);
        Debug.Log(randTest.Length);
        for (int i = 0; i < randTest.Length; i++)
        {
            Debug.Log("Value: " + randTest[i]);
        }
    }
}

[CustomEditor(typeof(Gameplay))]
public class GameManagerEditor : Editor
{
    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Gameplay selected = (Gameplay)target;

        if (selected.DataIndexes != null)
        {
            EditorGUILayout.BeginVertical();
            for (int i = selected.DataIndexes.Length - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < selected.DataIndexes[i].Length; j++)
                {
                    int value = selected.DataIndexes[i][j];
                    if (value == 1)
                    {
                        GUI.backgroundColor = Color.red;
                    }
                    else if (value == 2)
                    {
                        GUI.backgroundColor = Color.blue;
                    }
                    else if (value == 3)
                    {
                        GUI.backgroundColor = Color.green;
                    }

                    GUILayout.Button(string.Format("{0}", value));

                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}

namespace Ultilities
{
    public static class Extensions
    {
        public static T[] GetRandom<T>(this List<T> list, int number)
        {
            T[] result = new T[number];
            List<T> selectedList = new List<T>(list);

            int currentIndex = 0;
            while (currentIndex < number)
            {
                if (selectedList.Count == 0)
                {
                    System.Array.Resize<T>(ref result, result.Length - 1);
                }
                else
                {
                    int randIndex = UnityEngine.Random.Range(0, selectedList.Count);
                    T item = selectedList[randIndex];
                    result[currentIndex] = item;

                    selectedList.RemoveAt(randIndex);
                }
                
                currentIndex++;
            }

            return result;
        }
    }

    namespace AStar
    {
        public class GridNode : INode
        {
            private bool isOpenList = false;
            private bool isClosedList = false;

            public int X { get; private set; }
            public int Y { get; private set; }

            public bool IsWall { get; set; }

            public Grid2D Grid;

            public GridNode(Grid2D grid, int x, int y, bool isWall)
            {
                Grid = grid;
                X = x;
                Y = y;
                IsWall = isWall;
            }

            /// <summary>
            /// Gets or sets whether this node is on the open list.
            /// </summary>
            public bool IsOpenList(IEnumerable<INode> openList)
            {
                return isOpenList;
            }

            public void SetOpenList(bool value)
            {
                isOpenList = value;
            }

            /// <summary>
            /// If this is a wall then return as unsearchable without ever checking the node.
            /// </summary>
            public bool IsClosedList(IEnumerable<INode> closedList)
            {
                return IsWall || isClosedList;
            }

            public void SetClosedList(bool value)
            {
                isClosedList = value;
            }

            /// <summary>
            /// Gets the total cost for this node.
            /// f = g + h
            /// TotalCost = MovementCost + EstimatedCost
            /// </summary>
            public int TotalCost { get { return MovementCost + EstimatedCost; } }

            /// <summary>
            /// Gets the movement cost for this node.
            /// This is the movement cost from this node to the starting node, or g.
            /// </summary>
            public int MovementCost { get; private set; }

            /// <summary>
            /// Gets the estimated cost for this node.
            /// This is the heuristic from this node to the goal node, or h.
            /// </summary>
            public int EstimatedCost { get; private set; }

            /// <summary>
            /// Parent.MovementCost + 1
            /// </summary>
            /// <param name="parent">Parent node, for access to the parents movement cost.</param>
            public void SetMovementCost(INode parent)
            {
                this.MovementCost = parent.MovementCost + 1;
            }

            /// <summary>
            /// Simple manhatten.
            /// </summary>
            /// <param name="goal">Goal node, for acces to the goals position.</param>
            public void SetEstimatedCost(INode goal)
            {
                var g = (GridNode)goal;
                this.EstimatedCost = System.Math.Abs(this.X - g.X) + System.Math.Abs(this.Y - g.Y);
            }

            /// <summary>
            /// Gets or sets the parent node to this node.
            /// </summary>
            public INode Parent { get; set; }

            // X - 1, Y - 1 | X, Y - 1 | X + 1, Y - 1
            // X - 1, Y     |          | X + 1, Y
            // X - 1, Y + 1 | X, Y + 1 | X + 1, Y + 1
            //private static int[] childXPos = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
            //private static int[] childYPos = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };

            private static int[] childXPos = new int[] { 0, -1, 1, 0, };
            private static int[] childYPos = new int[] { -1, 0, 0, 1, };

            /// <summary>
            /// Gets this node's children.
            /// </summary>
            /// <remarks>The children can be setup in a graph before starting the
            /// A* algorithm or they can be dynamically generated the first time
            /// the A* algorithm calls this property.</remarks>
            public IEnumerable<INode> Children {
                get {
                    var children = new List<GridNode>();

                    for (int i = 0; i < childXPos.Length; i++)
                    {
                        // skip any nodes out of bounds.
                        if (X + childXPos[i] >= Grid.Width || Y + childYPos[i] >= Grid.Height)
                            continue;
                        if (X + childXPos[i] < 0 || Y + childYPos[i] < 0)
                            continue;

                        children.Add(Grid.Grid[X + childXPos[i]][Y + childYPos[i]]);
                    }

                    return children;
                }
            }

            /// <summary>
            /// Returns true if this node is the goal, false if it is not the goal.
            /// </summary>
            /// <param name="goal">The goal node to compare this node against.</param>
            /// <returns>True if this node is the goal, false if it s not the goal.</returns>
            public bool IsGoal(INode goal)
            {
                return IsEqual((GridNode)goal);
            }

            /// <summary>
            /// Two nodes are equal if they share the same spot in the grid.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public bool IsEqual(GridNode node)
            {
                return (this == node) || (this.X == node.X && this.Y == node.Y);
            }

            public string Print(GridNode start, GridNode goal, IEnumerable<INode> path)
            {
                if (IsWall)
                {
                    return "W";
                }
                else if (IsEqual(start))
                {
                    return "s";
                }
                else if (IsEqual(goal))
                {
                    return "g";
                }
                else if (IsInPath(path))
                {
                    return ".";
                }
                else
                {
                    return " ";
                }
            }

            private bool IsInPath(IEnumerable<INode> path)
            {
                foreach (var node in path)
                {
                    if (IsEqual((GridNode)node))
                        return true;
                }
                return false;
            }

            public void Reset()
            {
                this.Parent = null;

                this.isClosedList = false;
                this.isOpenList = false;
            }
        }

        public class Grid2D
        {
            public GridNode[][] Grid;

            public int Width { get { return Grid.Length; } }
            public int Height { get { return Grid[0].Length; } }

            public GridNode Start;
            public GridNode Goal;

            public Grid2D(GridNode[][] grid, GridNode start, GridNode goal)
            {
                Grid = grid;
                Start = start;
                Goal = goal;
            }

            public Grid2D(int width, int height, int startX, int startY, int goalX, int goalY)
            {
                Start = new GridNode(this, startX, startY, false);
                Goal = new GridNode(this, goalX, goalY, false);

                Grid = new GridNode[height][];
                for (var i = 0; i < width; i++)
                    Grid[i] = new GridNode[width];

                Grid[Start.X][Start.Y] = Start;
                Grid[Goal.X][Goal.Y] = Goal;

                for (var i = 0; i < height; i++)
                {
                    for (var j = 0; j < width; j++)
                    {
                        // don't overwrite start/goal nodes
                        if (Grid[i][j] != null)
                            continue;

                        Grid[i][j] = new GridNode(this, i, j, false);
                    }
                }
            }

            public Grid2D(int width, int height, int wallPercentage, int startX, int startY, int goalX, int goalY)
            {
                var rand = new System.Random();
                Start = new GridNode(this, startX, startY, false);
                Goal = new GridNode(this, goalX, goalY, false);

                Grid = new GridNode[width][];
                for (var i = 0; i < width; i++)
                    Grid[i] = new GridNode[height];

                Grid[Start.X][Start.Y] = Start;
                Grid[Goal.X][Goal.Y] = Goal;

                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        // don't overwrite start/goal nodes
                        if (Grid[i][j] != null)
                            continue;

                        Grid[i][j] = new GridNode(this, i, j, rand.Next(100) < wallPercentage);
                    }
                }
            }

            public void SetWall(bool isWall, int x, int y)
            {
                Grid[x][y].IsWall = isWall;
            }

            public void SetStart(int x, int y)
            {
                //if (this.Start != null)
                //{
                //    this.Start.IsWall = true;
                //}

                GridNode node = this.Grid[x][y];
                node.IsWall = false;
                this.Start = node;
            }

            public void SetGoal(int x, int y)
            {
                GridNode node = this.Grid[x][y];
                this.Goal = node;
            }

            public string Print(IEnumerable<INode> path)
            {
                var output = "";
                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        output += Grid[i][j].Print(Start, Goal, path);
                    }
                    output += "\n";
                }
                return output;
            }

            public void Reset()
            {
                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        Grid[i][j].Reset();
                    }
                }
            }
        }

        /// <summary>
        /// The A* algorithm takes a starting node and a goal node and searchings from
        /// start to the goal.
        /// 
        /// The nodes can be setup in a graph ahead of running the algorithm or the children
        /// nodes can be generated on the fly when the A* algorithm requests the Children property.
        /// 
        /// See the square puzzle implementation to see the children being generated on the fly instead
        /// of the classical image/graph search with walls.
        /// </summary>
        public interface INode
        {
            /// <summary>
            /// Determines if this node is on the open list.
            /// </summary>
            bool IsOpenList(IEnumerable<INode> openList);

            /// <summary>
            /// Sets this node to be on the open list.
            /// </summary>
            void SetOpenList(bool value);

            /// <summary>
            /// Determines if this node is on the closed list.
            /// </summary>
            bool IsClosedList(IEnumerable<INode> closedList);

            /// <summary>
            /// Sets this node to be on the open list.
            /// </summary>
            void SetClosedList(bool value);

            /// <summary>
            /// Gets the total cost for this node.
            /// f = g + h
            /// TotalCost = MovementCost + EstimatedCost
            /// </summary>
            int TotalCost { get; }

            /// <summary>
            /// Gets the movement cost for this node.
            /// This is the movement cost from this node to the starting node, or g.
            /// </summary>
            int MovementCost { get; }

            /// <summary>
            /// Gets the estimated cost for this node.
            /// This is the heuristic from this node to the goal node, or h.
            /// </summary>
            int EstimatedCost { get; }

            /// <summary>
            /// Sets the movement cost for the current node, or g.
            /// </summary>
            /// <param name="parent">Parent node, for access to the parents movement cost.</param>
            void SetMovementCost(INode parent);

            /// <summary>
            /// Sets the estimated cost for the current node, or h.
            /// </summary>
            /// <param name="goal">Goal node, for acces to the goals position.</param>
            void SetEstimatedCost(INode goal);

            /// <summary>
            /// Gets or sets the parent node to this node.
            /// </summary>
            INode Parent { get; set; }

            /// <summary>
            /// Gets this node's children.
            /// </summary>
            /// <remarks>The children can be setup in a graph before starting the
            /// A* algorithm or they can be dynamically generated the first time
            /// the A* algorithm calls this property.</remarks>
            IEnumerable<INode> Children { get; }

            /// <summary>
            /// Returns true if this node is the goal, false if it is not the goal.
            /// </summary>
            /// <param name="goal">The goal node to compare this node against.</param>
            /// <returns>True if this node is the goal, false if it s not the goal.</returns>
            bool IsGoal(INode goal);
        }

        /// <summary>
        /// Extension methods to make the System.Collections.Generic.SortedList easier to use.
        /// </summary>
        internal static class SortedListExtensions
        {
            /// <summary>
            /// Checks if the SortedList is empty.
            /// </summary>
            /// <param name="sortedList">SortedList to check if it is empty.</param>
            /// <returns>True if sortedList is empty, false if it still has elements.</returns>
            internal static bool IsEmpty<TKey, TValue>(this SortedList<TKey, TValue> sortedList)
            {
                return sortedList.Count == 0;
            }

            /// <summary>
            /// Adds a INode to the SortedList.
            /// </summary>
            /// <param name="sortedList">SortedList to add the node to.</param>
            /// <param name="node">Node to add to the sortedList.</param>
            internal static void Add(this SortedList<int, INode> sortedList, INode node)
            {
                sortedList.Add(node.TotalCost, node);
            }

            /// <summary>
            /// Removes the node from the sorted list with the smallest TotalCost and returns that node.
            /// </summary>
            /// <param name="sortedList">SortedList to remove and return the smallest TotalCost node.</param>
            /// <returns>Node with the smallest TotalCost.</returns>
            internal static INode Pop(this SortedList<int, INode> sortedList)
            {
                var top = sortedList.Values[0];
                sortedList.RemoveAt(0);
                return top;
            }
        }

        /// <summary>
        /// AStar algorithm states while searching for the goal.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// The AStar algorithm is still searching for the goal.
            /// </summary>
            Searching,

            /// <summary>
            /// The AStar algorithm has found the goal.
            /// </summary>
            GoalFound,

            /// <summary>
            /// The AStar algorithm has failed to find a solution.
            /// </summary>
            Failed
        }

        /// <summary>
        /// System.Collections.Generic.SortedList by default does not allow duplicate items.
        /// Since items are keyed by TotalCost there can be duplicate entries per key.
        /// </summary>
        internal class DuplicateComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return (x <= y) ? -1 : 1;
            }
        }

        /// <summary>
        /// Interface to setup and run the AStar algorithm.
        /// </summary>
        public class AStar
        {
            /// <summary>
            /// The open list.
            /// </summary>
            private SortedList<int, INode> openList;

            /// <summary>
            /// The closed list.
            /// </summary>
            private SortedList<int, INode> closedList;

            /// <summary>
            /// The current node.
            /// </summary>
            private INode current;

            /// <summary>
            /// The goal node.
            /// </summary>
            private INode goal;

            /// <summary>
            /// Gets the current amount of steps that the algorithm has performed.
            /// </summary>
            public int Steps { get; private set; }

            /// <summary>
            /// Gets the current state of the open list.
            /// </summary>
            public IEnumerable<INode> OpenList { get { return openList.Values; } }

            /// <summary>
            /// Gets the current state of the closed list.
            /// </summary>
            public IEnumerable<INode> ClosedList { get { return closedList.Values; } }

            /// <summary>
            /// Gets the current node that the AStar algorithm is at.
            /// </summary>
            public INode CurrentNode { get { return current; } }

            /// <summary>
            /// Creates a new AStar algorithm instance with the provided start and goal nodes.
            /// </summary>
            /// <param name="start">The starting node for the AStar algorithm.</param>
            /// <param name="goal">The goal node for the AStar algorithm.</param>
            public AStar(INode start, INode goal)
            {
                var duplicateComparer = new DuplicateComparer();
                openList = new SortedList<int, INode>(duplicateComparer);
                closedList = new SortedList<int, INode>(duplicateComparer);
                Reset(start, goal);
            }

            /// <summary>
            /// Resets the AStar algorithm with the newly specified start node and goal node.
            /// </summary>
            /// <param name="start">The starting node for the AStar algorithm.</param>
            /// <param name="goal">The goal node for the AStar algorithm.</param>
            public void Reset(INode start, INode goal)
            {
                openList.Clear();
                closedList.Clear();
                current = start;
                this.goal = goal;
                openList.Add(current);
                current.SetOpenList(true);
            }

            /// <summary>
            /// Steps the AStar algorithm forward until it either fails or finds the goal node.
            /// </summary>
            /// <returns>Returns the state the algorithm finished in, Failed or GoalFound.</returns>
            public State Run()
            {
                // Continue searching until either failure or the goal node has been found.
                while (true)
                {
                    State s = Step();
                    if (s != State.Searching)
                        return s;
                }
            }

            /// <summary>
            /// Moves the AStar algorithm forward one step.
            /// </summary>
            /// <returns>Returns the state the alorithm is in after the step, either Failed, GoalFound or still Searching.</returns>
            public State Step()
            {
                Steps++;
                while (true)
                {
                    // There are no more nodes to search, return failure.
                    if (openList.IsEmpty())
                    {
                        return State.Failed;
                    }

                    // Check the next best node in the graph by TotalCost.
                    current = openList.Pop();

                    // This node has already been searched, check the next one.
                    if (current.IsClosedList(ClosedList))
                    {
                        continue;
                    }

                    // An unsearched node has been found, search it.
                    break;
                }

                // Remove from the open list and place on the closed list 
                // since this node is now being searched.
                current.SetOpenList(false);
                closedList.Add(current);
                current.SetClosedList(true);

                // Found the goal, stop searching.
                if (current.IsGoal(goal))
                {
                    return State.GoalFound;
                }

                // Node was not the goal so add all children nodes to the open list.
                // Each child needs to have its movement cost set and estimated cost.
                foreach (var child in current.Children)
                {
                    // If the child has already been searched (closed list) or is on
                    // the open list to be searched then do not modify its movement cost
                    // or estimated cost since they have already been set previously.
                    if (child.IsOpenList(OpenList) || child.IsClosedList(ClosedList))
                    {
                        continue;
                    }

                    child.Parent = current;
                    child.SetMovementCost(current);
                    child.SetEstimatedCost(goal);
                    openList.Add(child);
                    child.SetOpenList(true);
                }

                // This step did not find the goal so return status of still searching.
                return State.Searching;
            }

            /// <summary>
            /// Gets the path of the last solution of the AStar algorithm.
            /// Will return a partial path if the algorithm has not finished yet.
            /// </summary>
            /// <returns>Returns null if the algorithm has never been run.</returns>
            public IEnumerable<INode> GetPath()
            {
                if (current != null)
                {
                    var next = current;
                    var path = new List<INode>();
                    while (next != null)
                    {
                        path.Add(next);
                        next = next.Parent;
                    }
                    path.Reverse();
                    return path.ToArray();
                }
                return null;
            }
        }
    }
}