using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Required for Coroutines
using System; // Required for Action

public class UIManager : MonoBehaviour
{
    public SudokuGame sudokuGame;

    [Header("UI Elements")]
    public GameObject sudokuCellPrefab;
    public Transform sudokuGridPanel;
    public GameObject loadingScreen; // Reference to a "Loading..." screen panel
    
    private TMP_InputField[] cellUIFields = new TMP_InputField[81];

    void Start()
    {
        sudokuGame = GetComponent<SudokuGame>();
        if (sudokuGame == null)
        {
            Debug.LogError("SudokuGame script not found on this GameObject!");
            return;
        }
        // Make sure the loading screen is hidden at the start
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }

    public void InitializeUI()
    {
        StartCoroutine(CreateGridUICoroutine());
    }

    private IEnumerator CreateGridUICoroutine()
    {
        for (int i = 0; i < 81; i++)
        {
            GameObject cellObject = Instantiate(sudokuCellPrefab, sudokuGridPanel);
            cellUIFields[i] = cellObject.GetComponent<TMP_InputField>();
            int cellIndex = i; 
            cellUIFields[i].onEndEdit.AddListener((value) => OnCellInputValueChanged(cellIndex, value));
            
            if ((i + 1) % 9 == 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        UpdateGridUI();
    }

    public void UpdateGridUI()
    {
        for (int i = 0; i < 81; i++)
        {
            int row = i / 9;
            int col = i % 9;
            int number = sudokuGame.grid[row, col];
            int initialNumber = sudokuGame.initialGrid[row, col];

            TMP_InputField cell = cellUIFields[i];

            if (number != 0)
            {
                cell.text = number.ToString();
            }
            else
            {
                cell.text = "";
            }

            // Handle different states: custom setup, locked cells, and editable cells
            if (sudokuGame.isInCustomSetupMode)
            {
                // In custom setup mode, all cells are editable
                cell.readOnly = false;
                cell.textComponent.color = Color.green; // Green for setup mode
            }
            else if (initialNumber != 0)
            {
                // Locked initial numbers
                cell.readOnly = true;
                cell.textComponent.color = Color.black;
            }
            else
            {
                // User-editable cells during gameplay
                cell.readOnly = false;
                cell.textComponent.color = Color.blue;
            }
        }
    }

    private void OnCellInputValueChanged(int cellIndex, string value)
    {
        int row = cellIndex / 9;
        int col = cellIndex % 9;

        // Allow editing if in custom setup mode, or if cell is not part of initial puzzle
        if (!sudokuGame.isInCustomSetupMode && sudokuGame.initialGrid[row, col] != 0) 
            return;

        int number = 0;
        if (!string.IsNullOrEmpty(value))
        {
            // Better input validation - only accept single digits 1-9
            if (!int.TryParse(value, out number) || number < 1 || number > 9)
            {
                number = 0; // Invalid input, clear the cell
            }
        }

        // In custom setup mode, just set the grid directly (no validation)
        if (sudokuGame.isInCustomSetupMode)
        {
            sudokuGame.grid[row, col] = number;
        }
        else
        {
            // Normal gameplay - validate placement
            if (number == 0 || sudokuGame.IsValidPlacement(sudokuGame.grid, row, col, number))
            {
                sudokuGame.grid[row, col] = number;
            }
        }
        
        // Update UI to show actual grid state
        int finalGridValue = sudokuGame.grid[row, col];
        if (finalGridValue != 0)
        {
            cellUIFields[cellIndex].SetTextWithoutNotify(finalGridValue.ToString());
        }
        else
        {
            cellUIFields[cellIndex].SetTextWithoutNotify("");
        }
    }

    /// <summary>
    /// A generic coroutine to handle slow operations without freezing the UI.
    /// </summary>
    private IEnumerator GenerateNewGameRoutine(Action generationFunction)
    {
        // Show a loading screen
        if (loadingScreen != null) loadingScreen.SetActive(true);

        // Wait a frame to ensure the loading screen is displayed
        yield return null;

        // Run the slow puzzle generation
        generationFunction();

        // Update the UI with the new puzzle
        UpdateGridUI();

        // Hide the loading screen
        if (loadingScreen != null) loadingScreen.SetActive(false);
    }

    // --- Public Methods for UI Buttons ---

    public void OnNewGameEasyClicked()
    {
        StartCoroutine(GenerateNewGameRoutine(() => sudokuGame.NewGame(SudokuGame.Difficulty.Easy)));
    }

    public void OnNewGameMediumClicked()
    {
        StartCoroutine(GenerateNewGameRoutine(() => sudokuGame.NewGame(SudokuGame.Difficulty.Medium)));
    }

    public void OnNewGameHardClicked()
    {
        StartCoroutine(GenerateNewGameRoutine(() => sudokuGame.NewGame(SudokuGame.Difficulty.Hard)));
    }

    public void OnSolveButtonClicked()
    {
        // Solve is fast, so no coroutine needed
        if (sudokuGame.SolveCurrentPuzzle())
        {
            UpdateGridUI();
        }
    }

    public void OnCustomGameClicked()
    {
        // Custom is fast (just clears the board), so no coroutine needed
        sudokuGame.NewGame(SudokuGame.Difficulty.Custom);
        UpdateGridUI();
    }

    public void OnLockPuzzleClicked()
    {
        sudokuGame.LockCustomPuzzle();
        UpdateGridUI(); // This will update colors and readonly states
    }
}