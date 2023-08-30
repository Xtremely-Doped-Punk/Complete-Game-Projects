using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _2048
{
    public class GameManager : MonoBehaviour
    {
        private const string HIGH_TILE_VAL = "2048HighTileVal";
        private const string HIGH_SCORE = "2048HighScore";
        private const int MIN_ROWS = 2, MIN_COLS = 2;

        public static GameManager Instance { get; private set; } = null;

        [SerializeField, Tooltip("no.of rows")]
        private int gridHeight = 4;
        [SerializeField, Tooltip("no.of columns")] 
        private int gridWidth = 4;
        [SerializeField, Range(0, 1)] float animationDelay = 0.1f;

        [SerializeField] private TileBoard board;
        [SerializeField] private Button restartBtn;
        [SerializeField] private Button retryBtn;
        [SerializeField] private CanvasGroup gameOverCanvasGroup;
        [SerializeField] private TextMeshProUGUI currentScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI highestTileValueText;
        [SerializeField] private TMP_InputField NumOfRowsInp, NumOfColsInp;

        private int currentScore, highScore, highTileValue;

        private void Awake()
        {
            Instance = this;

            restartBtn.onClick.AddListener(NewGame);
            retryBtn.onClick.AddListener(NewGame);

            NumOfRowsInp.text = gridHeight.ToString();
            NumOfRowsInp.onEndEdit.AddListener((val) =>
            {
                if (int.TryParse(val, out int rows) && rows >= MIN_ROWS)
                    NumOfRowsInp.text = Mathf.Abs(rows).ToString();
                else
                    NumOfRowsInp.text = gridHeight.ToString();
            });

            NumOfColsInp.text = gridWidth.ToString();
            NumOfColsInp.onEndEdit.AddListener((val) =>
            {
                if (int.TryParse(val, out int cols) && cols >= MIN_COLS)
                    NumOfColsInp.text = Mathf.Abs(cols).ToString();
                else
                    NumOfColsInp.text = gridWidth.ToString();
            });
        }

        private void Start()
        {
            NewGame();
            
            board.OnGameOver += (_, _) => GameOver();
            board.OnMaxTileValueChanged += (_, _) => UpdateUI_OnMaxTileValueChanged();
        }

        private void NewGame()
        {
            LoadHighScores();
            ResetScore();
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.gameObject.SetActive(false);
            restartBtn.interactable = true; restartBtn.Select();
            board.ClearBoard();
            StartCoroutine(board.InitializeBoard(SetGridDimentions(), 1 / 3));
            board.enabled = true;
        }
        private bool SetGridDimentions()
        {
            if (!(int.TryParse(NumOfRowsInp.text, out int height) && int.TryParse(NumOfColsInp.text, out int width)))
                return false;

            if (height == gridHeight && width == gridWidth) return false;

            gridHeight = height;
            gridWidth = width;
            return true;
        }

        private void GameOver()
        {
            board.enabled = false;
            gameOverCanvasGroup.gameObject.SetActive(true);
            retryBtn.Select(); restartBtn.interactable = false;
            StartCoroutine(Fade(gameOverCanvasGroup, 1f, .5f, 1f));
        }

        private IEnumerator Fade(CanvasGroup canvasGroup, float toVal, float duration, float delay = .5f)
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            float fromVal;
            
            while (elapsed < duration)
            {
                fromVal = canvasGroup.alpha;
                canvasGroup.alpha = Mathf.Lerp(fromVal, toVal, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = toVal;
        }

        private void UpdateUI_OnMaxTileValueChanged()
        {
            int tileVal = board.GetMaxTileValue();
            if (tileVal > highTileValue)
            {
                highTileValue = tileVal;
                highestTileValueText.text = highTileValue.ToString();
                SaveHighTileValue();
            }
        }

        private void LoadHighScores()
        {
            highTileValue = PlayerPrefs.GetInt(HIGH_TILE_VAL, 0);
            highestTileValueText.text = highTileValue.ToString();

            highScore = PlayerPrefs.GetInt(HIGH_SCORE, 0);
            highScoreText.text = highScore.ToString();

        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HIGH_SCORE, highScore);
            PlayerPrefs.Save();
        }
        private void SaveHighTileValue()
        {
            PlayerPrefs.SetInt(HIGH_TILE_VAL, highTileValue);
            PlayerPrefs.Save();
        }

        public void IncrementScore(int increment)
        {
            currentScore += increment;
            currentScoreText.text = currentScore.ToString();
            if (currentScore > highScore)
            {
                highScore = currentScore;
                highScoreText.text = highScore.ToString();
                SaveHighScore();
            }
        }

        private void ResetScore()
        {
            currentScore = 0;
            currentScoreText.text = currentScore.ToString();
        }

        /// <summary>
        /// Total Size Grid Board (no.of rows * no. of cols)
        /// </summary>
        public int GridSize => gridWidth * gridHeight;
        /// <summary>
        /// Number of Rows in Grid Board
        /// </summary>
        public int GridHeight => gridHeight;
        /// <summary>
        /// Number of Columns in Grid Board
        /// </summary>
        public int GridWidth => gridWidth;
        public float GetDelay() => animationDelay;
    }
}