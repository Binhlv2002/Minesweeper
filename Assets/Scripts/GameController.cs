using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public Camera cameraSize;
    public int width;
    public int height;
    public int mineCount;
    private Board board;
    private Cell[,] state;
    private bool gameOver;

    public TextMeshProUGUI timeDisplay;
    private bool timerStarted;
    private float startTime;
    private float elapsedTime;

    public TextMeshProUGUI flagCountDisplay;
    private int remainingFlags;

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        state = new Cell[width, height];
        gameOver = false;

        timerStarted = false;
        elapsedTime = 0f;
        timeDisplay.text = ""+ elapsedTime;

        remainingFlags = 10;
        flagCountDisplay.text = "" + remainingFlags;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10);

        if (width == 8 && height == 8)
        {
            cameraSize.orthographicSize = 10f;
        }

        else if (width == 16 && height == 16)
        {
            cameraSize.orthographicSize = 15f;
        }

        board.Draw(state);

    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (state[x, y].type == Cell.Type.Mine)
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                }
                if (y >= height)
                {
                    y = 0;
                }
            }

            state[x, y].type = Cell.Type.Mine;
            state[x, y].revealed = false;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x,y);

                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }
                cell.revealed = false;
                state[x,y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for(int adjacentX = -1;  adjacentX <= 1; adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x,y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;

    }


    private void Update()
    {
        if (!gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }

            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }

            if (timerStarted)
            {
                elapsedTime = Time.time - startTime;
                timeDisplay.text = elapsedTime.ToString("F2");
            }
        }
      
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if(cell.type == Cell.Type.Invalid || cell.revealed) 
        {
            return;
        }

        if (cell.flagged)
        {
            remainingFlags++;
        }
        else if (remainingFlags > 0)
        {
            remainingFlags--;
        }
        else
        {
            return; 
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        flagCountDisplay.text = "" + remainingFlags;
        board.Draw(state);

    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x,y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x>=0 && x < width && y>=0 && y < height;
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        
        if(cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        if (!timerStarted)
        {
            timerStarted = true;
            startTime = Time.time;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Exploded(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;

        }

       
        board.Draw(state);
    }

    private void Exploded(Cell cell)
    {
        Debug.Log("Thua");
        gameOver = true;
        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if(cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true ;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty) 
        {
            Flood(GetCell(cell.position.x -1, cell.position.y));
            Flood(GetCell(cell.position.x +1, cell.position.y));
            Flood(GetCell(cell.position.x , cell.position.y -1));
            Flood(GetCell(cell.position.x , cell.position.y +1));
        }
    }

    private void CheckWinCondition()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("Win");
        gameOver = true;

        
        if (PlayerPrefs.HasKey("BestTime"))
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime");
            if (elapsedTime < bestTime)
            {
                PlayerPrefs.SetFloat("BestTime", elapsedTime);
            }
        }
        else
        {
            PlayerPrefs.SetFloat("BestTime", elapsedTime);
        }

        PlayerPrefs.SetFloat("WinTime", elapsedTime);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }
}
