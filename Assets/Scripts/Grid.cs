using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid: MonoBehaviour
{
    public static Grid instance;
    public enum CandyType
    {
        EMPTY,
        NORMAL, 
        COUNT,
    };

    public int width, height;

    private Dictionary<CandyType, GameObject> candyPrefabDict;

    private Candy pressedCandy;
    private Candy enteredCandy;

    [System.Serializable]
    public struct CandyPrefab
    {
        public CandyType type;
        public GameObject prefab;
    }

    public CandyPrefab[] candyPrefabs;
    private Candy[,] candies;

    private bool isDestroying;
    public bool IsDestroying
    {
        get { return isDestroying; }
    }
    private void Start()
    {
        instance = GetComponent<Grid>();

        isDestroying = false;
        candyPrefabDict = new Dictionary<CandyType, GameObject>();
        for (int i = 0; i < candyPrefabs.Length; i++) { 
            if (!candyPrefabDict.ContainsKey(candyPrefabs[i].type)) {
                candyPrefabDict.Add(candyPrefabs[i].type, candyPrefabs[i].prefab);
            }
        }

        candies = new Candy[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnNewCandy(x, y, CandyType.EMPTY);

                if (candies[x, y].IsColored())
                    candies[x, y].ColorComponent.SetColor((CandyColor.ColorType)UnityEngine.Random.Range(0, candies[x, y].ColorComponent.NumColors));
            }
        }

        StartCoroutine(Fill());
    }

    public bool IsOnBoard(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - width / 2.0f + x,
            transform.position.y + height / 2.0f - y);
    }

    public Candy SpawnNewCandy(int x, int y, CandyType type)
    {
        GameObject candy = Instantiate(candyPrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        candy.name = "Candy(" + x + ", " + y + ")";

        candy.transform.parent = transform;

        candies[x, y] = candy.GetComponent<Candy>();
        candies[x, y].Init(x, y, this, type);

        return candies[x, y];
    }

    public float fillTime;
    public IEnumerator Fill()
    {
        bool needsRefill = true;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                yield return new WaitForSeconds(fillTime);
            }
            needsRefill = ClearAllMatches();
            isDestroying = true;
        }
        isDestroying = false;
        GUIManager.instance.timerIsRunning = true;
        GetAllPossibleMoves();
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = height - 2; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Candy candy = candies[x, y];
                if (candy.IsMovable())
                {
                    Candy candyBelow = candies[x, y + 1];
                    if (candyBelow.Type == CandyType.EMPTY)
                    {
                        Destroy(candyBelow.gameObject);
                        candy.MovableComponent.Move(x, y + 1, fillTime);
                        candies[x, y + 1] = candy;
                        SpawnNewCandy(x, y, CandyType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            Candy candyBelow = candies[x, 0];
            if (candyBelow.Type == CandyType.EMPTY)
            {
                Destroy(candyBelow.gameObject);
                GameObject candy = Instantiate(candyPrefabDict[CandyType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                candy.transform.parent = transform;

                candies[x, 0] = candy.GetComponent<Candy>();
                candies[x, 0].Init(x, -1, this, CandyType.NORMAL);
                candies[x, 0].MovableComponent.Move(x, 0, fillTime);
                candies[x, 0].ColorComponent.SetColor((CandyColor.ColorType)Random.Range(0, candies[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }
        }


        return movedPiece;
    }

    public bool IsAdjacent(Candy candy1, Candy candy2)
    {
        return (candy1.X == candy2.X && (int)Mathf.Abs(candy2.Y - candy1.Y) == 1)
            || (candy1.Y == candy2.Y && (int)Mathf.Abs(candy2.X - candy1.X) == 1);
    }

    public void SwapCandies(Candy candy1, Candy candy2)
    {
        if (candy1.IsMovable() && candy2.IsMovable())
        {
            candies[candy1.X, candy1.Y] = candy2;
            candies[candy2.X, candy2.Y] = candy1;

            if (GetMatch(candy1, candy2.X, candy2.Y) != null ||
                GetMatch(candy2, candy1.X, candy1.Y) != null)
            {

                int candy1X = candy1.X;
                int candy1Y = candy1.Y;

                candy1.MovableComponent.Move(candy2.X, candy2.Y, fillTime);
                candy2.MovableComponent.Move(candy1X, candy1Y, fillTime);

                SFXManager.instance.PlaySFX(Clip.Swap);

                ClearAllMatches();

                pressedCandy = null;
                enteredCandy = null;

                StartCoroutine(Fill());
            }
            else
            {
                candies[candy1.X, candy1.Y] = candy1;
                candies[candy2.X, candy2.Y] = candy2;

                //Debug.Log("Can't swap");
            }
        }
    }

    public void PressCandy(Candy candy)
    {
        pressedCandy = candy;
    }

    public void EnterCandy(Candy candy)
    {
        enteredCandy = candy;
    }

    public void ReleaseCandy()
    {
        if(IsAdjacent(pressedCandy, enteredCandy))
        {
            SwapCandies(pressedCandy, enteredCandy);
        }
    }

    public List<Candy> GetMatch(Candy candy, int newX, int newY)
    {
        CandyColor.ColorType color = candy.ColorComponent.Color;
        List<Candy> matchingCandies = new List<Candy>();

        if (candy.IsColored())
        {
            int[] u = { 0, 1};
            int[] v = { 1, 0};

            int i, j, k;
            for (int t = 0; t < u.Length; t++)
            {
                k = 0;
                i = newX;
                j = newY;

                while (true) {
                    i += u[t];
                    j += v[t];

                    if (!IsOnBoard(i, j))
                        break;
                    if (candies[i, j].Type == CandyType.EMPTY || candies[i, j].Type != CandyType.EMPTY &&
                        color != candies[i, j].ColorComponent.Color)
                        break;
                    k++;
                }
                i = newX;
                j = newY;

                while (true)
                {
                    i -= u[t];
                    j -= v[t];

                    if (!IsOnBoard(i, j))
                        break;
                    if (candies[i, j].Type == CandyType.EMPTY || candies[i, j].Type != CandyType.EMPTY &&
                        color != candies[i, j].ColorComponent.Color)
                        break;
                    k++;
                }
                //First candy
                k++;
                if (k >= 3)
                {
                    while (k-- > 0)
                    {
                        i += u[t];
                        j += v[t];
                        //Debug.Log(i + ", " + j);

                        if (candies[i, j].Type != CandyType.EMPTY
                            && candies[i, j].ColorComponent.Color == color
                            && (i != newX || j != newY)) 
                            matchingCandies.Add(candies[i, j]);
                    }
                }
            }
        }

        if (matchingCandies.Count > 0)
        {
            //Debug.Log("Yay");
            matchingCandies.Add(candy);
        }
        else
            matchingCandies = null;
        return matchingCandies;
    }

    public bool ClearAllMatches()
    {
        bool needsRefill = false;
        List<Candy> match = new List<Candy>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                 if (candies[x, y].IsClearable()) {
                    List<Candy> tmp = GetMatch(candies[x, y], x, y);

                    if (tmp != null) { 
                        for (int i = 0; i < tmp.Count; i++) {
                            if (!match.Contains(tmp[i]))
                                match.Add(tmp[i]);
                        }
                    }
                }
            }
        }

        int count = match.Count;
        for (int i = 0; i < count; i++)
        {
            if (ClearCandy(match[i].X, match[i].Y))
            {
                SFXManager.instance.PlaySFX(Clip.Clear);
                //Debug.Log("Number of matches: " + count--);
                needsRefill = true;
            }
        }

        GUIManager.instance.Score += 100 * count;
        //Debug.Log(match.Count - 3 + 1);
        return needsRefill;
    }

    public bool ClearCandy(int x, int y)
    {
        if (candies[x, y].IsClearable() && !candies[x, y].ClearableComponent.IsBeingCleared)
        {
            candies[x, y].ClearableComponent.Clear();
            //Debug.Log("Clear " + x + ", " + y);
            SpawnNewCandy(x, y, CandyType.EMPTY);
            return true;
        }
        return false;
    }

    public int GetAllPossibleMoves()
    {
        List<PossibleMove> allPossibleMoveList = new List<PossibleMove>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<PossibleMove> tmpPossibleMoveList = new List<PossibleMove>();
                if (IsOnBoard(x + 1, y) && CanSwap(candies[x, y], candies[x + 1, y])) {
                    tmpPossibleMoveList.Add(new PossibleMove(x, y, x + 1, y));
                    //Debug.Log("Right " + x + ", " + y;
                }
                if (IsOnBoard(x - 1, y) && CanSwap(candies[x, y], candies[x - 1, y]))
                {
                    tmpPossibleMoveList.Add(new PossibleMove(x, y, x - 1, y));
                    //Debug.Log("Left " + x + ", " + y);
                }
                if (IsOnBoard(x, y + 1) && CanSwap(candies[x, y], candies[x, y + 1]))
                {
                    tmpPossibleMoveList.Add(new PossibleMove(x, y, x, y + 1));
                    //Debug.Log("Down " + x + ", " + y);
                }
                if (IsOnBoard(x, y - 1) && CanSwap(candies[x, y], candies[x, y - 1]))
                {
                    tmpPossibleMoveList.Add(new PossibleMove(x, y, x, y - 1));
                   // Debug.Log("Up " + x + ", " + y);
                }

                for (int i = 0; i < tmpPossibleMoveList.Count; i++) {
                    bool skipPossibleMove = false;
                    PossibleMove possibleMove = tmpPossibleMoveList[i];

                    for (int j = 0; j < allPossibleMoveList.Count; j++)
                    {
                        PossibleMove tmpPossibleMove = allPossibleMoveList[j];
                        if (tmpPossibleMove.startX == possibleMove.startX &&
                            tmpPossibleMove.startY == possibleMove.startY &&
                            tmpPossibleMove.endX == possibleMove.endX &&
                            tmpPossibleMove.endY == possibleMove.endY)
                        {
                            skipPossibleMove = true;
                            break;
                        }

                        if (tmpPossibleMove.startX == possibleMove.endX &&
                           tmpPossibleMove.startY == possibleMove.endY &&
                           tmpPossibleMove.endX == possibleMove.startX &&
                           tmpPossibleMove.endY == possibleMove.startY)
                        {
                            skipPossibleMove = true;
                            break;
                        }
                    }

                    if (skipPossibleMove)
                        continue;
                    else
                        allPossibleMoveList.Add(possibleMove);
                }
            }
        }

        Debug.Log("Possible moves left: " + allPossibleMoveList.Count);
        if (allPossibleMoveList.Count == 0)
        {
            Debug.Log("GAME OVER");
            GameManager.instance.gameOver = true;
        }
        return allPossibleMoveList.Count;
    }

    public bool CanSwap(Candy candy1, Candy candy2)
    {
        bool canSwap = false;
        if (candy1.IsMovable() && candy2.IsMovable())
        {
            candies[candy1.X, candy1.Y] = candy2;
            candies[candy2.X, candy2.Y] = candy1;

            if (GetMatch(candy1, candy2.X, candy2.Y) != null ||
                GetMatch(candy2, candy1.X, candy1.Y) != null)
                canSwap = true;
            else
                canSwap = false;
        }
        candies[candy1.X, candy1.Y] = candy1;
        candies[candy2.X, candy2.Y] = candy2;
        return canSwap;
    }


    public class PossibleMove
    {
        public int startX, startY, endX, endY;
        public PossibleMove(int startX, int startY, int endX, int endY)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }
    }
}