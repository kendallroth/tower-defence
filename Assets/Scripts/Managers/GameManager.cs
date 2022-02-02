using com.ootii.Messages;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GameStats))]
public class GameManager : GameSingleton<GameManager>
{
    #region Attributes
    private bool _gamePaused = false;
    private bool _gameOver = false;
    #endregion

    #region Properties
    public GameStats gameStats { get; private set; }
    public bool gamePaused => _gamePaused;
    public bool gameOver => _gameOver;
    #endregion


    #region Unity Methods
    void Awake()
    {
        gameStats = GetComponent<GameStats>();

        Time.timeScale = 1;
    }

    private void Update()
    {
        // DEBUG: Trigger a game loss for testing purposes
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            LoseGame();
        }

        // TODO: Handle pausing with new input system
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    #endregion


    #region Custom Methods
    public void TogglePause()
    {
        _gamePaused = !_gamePaused;

        Time.timeScale = _gamePaused ? 0 : 1;

        MessageDispatcher.SendMessageData(GameEvents.GAME__PAUSE_CHANGE, _gamePaused);
    }

    public void LoseGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        // NOTE: Only possible through debugging or anything that suddenly triggers game end
        if (gameStats.lives > 0)
            gameStats.RemoveLives(gameStats.lives);

        Debug.Log("Game has been lost!");

        GameOverData message = new GameOverData(GameResult.LOST, gameStats.wavesSurvived);
        MessageDispatcher.SendMessageData(GameEvents.GAME__OVER, message, -1);
    }

    public void WinGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        Debug.Log("Game has been won!");

        GameOverData message = new GameOverData(GameResult.WON, gameStats.wavesSurvived);
        MessageDispatcher.SendMessageData(GameEvents.GAME__OVER, message);
    }
    #endregion


    #region Event Handlers
    #endregion
}
