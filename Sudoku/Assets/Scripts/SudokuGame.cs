using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SudokuGame : MonoBehaviour
{
    // Add a reference to the UIManager
    private UIManager uiManager;

    public int[,] grid = new int[9, 9];
    public int[,] initialGrid = new int[9, 9];
    
    // Add state tracking for custom mode
    public bool isInCustomSetupMode = false;

    public enum Difficulty { Easy, Medium, Hard, Custom }

    void Start()
    {
        // Get the UIManager component from the same GameObject
        uiManager = GetComponent<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager script not found on the GameManager object!");
            return;
        }

        // Generate a medium puzzle automatically on start
        NewGame(Difficulty.Medium);

        // NOW, tell the UIManager to start building the visual grid
        uiManager.InitializeUI();
    }

    /// <summary>
    /// Generates a new puzzle. The logic is now cleaner.
    /// </summary>
    public void NewGame(Difficulty difficulty)
    {
        if (difficulty == Difficulty.Custom)
        {
            // For custom mode, just clear the boards.
            grid = new int[9, 9];
            initialGrid = new int[9, 9];
            isInCustomSetupMode = true;
            Debug.Log("Custom mode started. Enter your puzzle and click 'Lock Puzzle'.");
        }
        else
        {
            // For other modes, generate a full puzzle.
            isInCustomSetupMode = false;
            GenerateSolvedGrid();
            System.Array.Copy(grid, initialGrid, 81);

            int cellsToRemove = 0;
            switch (difficulty)
            {
                case Difficulty.Easy:   cellsToRemove = 40; break;
                case Difficulty.Medium: cellsToRemove = 50; break;
                case Difficulty.Hard:   cellsToRemove = 60; break;
            }

            RemoveCells(cellsToRemove);
            System.Array.Copy(grid, initialGrid, 81);
            PrintGrid(grid);
        }
    }

    public void LockCustomPuzzle()
    {
        System.Array.Copy(grid, initialGrid, 81);
        isInCustomSetupMode = false;
        Debug.Log("Custom puzzle locked.");
    }

    private void GenerateSolvedGrid()
    {
        grid = new int[9, 9];
        SolveSudoku(grid);
    }

    private void RemoveCells(int count)
    {
        int removed = 0;
        while (removed < count)
        {
            int row = Random.Range(0, 9);
            int col = Random.Range(0, 9);

            if (grid[row, col] != 0)
            {
                grid[row, col] = 0;
                removed++;
            }
        }
    }

    public bool SolveCurrentPuzzle()
    {
        int[,] gridToSolve = new int[9, 9];
        System.Array.Copy(initialGrid, gridToSolve, 81);

        if (SolveSudoku(gridToSolve))
        {
            System.Array.Copy(gridToSolve, grid, 81);
            Debug.Log("Sudoku Solved!");
            PrintGrid(grid);
            return true;
        }
        else
        {
            Debug.Log("This Sudoku puzzle is unsolvable.");
            return false;
        }
    }

    private bool SolveSudoku(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsValidPlacement(board, row, col, num))
                        {
                            board[row, col] = num;
                            if (SolveSudoku(board))
                            {
                                return true;
                            }
                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsValidPlacement(int[,] board, int row, int col, int num)
    {
        for (int x = 0; x < 9; x++)
        {
            if (board[row, x] == num) return false;
        }
        for (int y = 0; y < 9; y++)
        {
            if (board[y, col] == num) return false;
        }
        int startRow = row - row % 3, startCol = col - col % 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i + startRow, j + startCol] == num) return false;
            }
        }
        return true;
    }

    public void SetNumber(int row, int col, int number)
    {
        if (number >= 0 && number <= 9)
        {
            if (initialGrid[row, col] == 0)
            {
                grid[row, col] = number;
            }
            else
            {
                Debug.LogWarning("Cannot change the initial numbers of the puzzle.");
            }
        }
    }
    
    public void SetCustomInitialNumber(int row, int col, int number)
    {
        if (number >= 0 && number <= 9)
        {
            grid[row, col] = number;
            initialGrid[row, col] = number;
        }
    }

    public void PrintGrid(int[,] board)
    {
        string gridString = "";
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                gridString += board[i, j] + " ";
            }
            gridString += "\n";
        }
        Debug.Log(gridString);
    }
}