using com.ootii.Messages;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    #region Attributes
    [Header("Starting Values")]
    [Min(0)]
    [SerializeField]
    private int _startingLives = 0;
    [Min(0)]
    [SerializeField]
    private int _startingCurrency = 0;

    [Header("Core")]
    [DisplayAsString]
    [ShowInInspector]
    private int _lives = 0;
    [DisplayAsString]
    [ShowInInspector]
    private int _currency = 0;
    [DisplayAsString]
    [ShowInInspector]
    private int _round = 0;

    [Title("Stats")]
    [DisplayAsString]
    [ShowInInspector]
    public int enemiesDestroyed { get; private set; } = 0;
    [DisplayAsString]
    [ShowInInspector]
    public int enemiesSpawned { get; private set; } = 0;
    [DisplayAsString]
    [ShowInInspector]
    public int wavesSurvived { get; private set; } = 0;
    [DisplayAsString]
    [ShowInInspector]
    public int currencyEarned { get; private set; } = 0;
    [DisplayAsString]
    [ShowInInspector]
    public int damageDealt { get; private set; } = 0;
    #endregion


    #region Properties
    public int lives => _lives;
    public int currency => _currency;
    public int round => _round;
    #endregion


    #region Unity Methods
    private void Awake()
    {
        _currency = _startingCurrency;
        _lives = _startingLives;

        MessageDispatcher.AddListener(GameEvents.ENEMY__DESTROYED, OnEnemyDestroy);
        MessageDispatcher.AddListener(GameEvents.ENEMY__REACHED_EXIT, OnEnemyReachExit);
        MessageDispatcher.AddListener(GameEvents.WAVE__START, OnWaveStart);
    }

    void Start()
    {
        // Notify listeners about the initial values for money and lives
        CurrencyChangeData moneyChangeData = new CurrencyChangeData(_currency, 0);
        MessageDispatcher.SendMessageData(GameEvents.PLAYER__CURRENCY_CHANGE, moneyChangeData, -1);
        LivesChangeData livesChangeData = new LivesChangeData(_lives, 0, _lives);
        MessageDispatcher.SendMessageData(GameEvents.PLAYER__LIVES_CHANGE, livesChangeData, -1);
    }

    private void OnDestroy()
    {
        // Remove events
        MessageDispatcher.RemoveListener(GameEvents.ENEMY__DESTROYED, OnEnemyDestroy);
        MessageDispatcher.RemoveListener(GameEvents.ENEMY__REACHED_EXIT, OnEnemyReachExit);
    }
    #endregion


    #region Custom Methods
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        _currency += amount;

        CurrencyChangeData moneyChangeData = new CurrencyChangeData(_currency, amount);
        MessageDispatcher.SendMessageData(GameEvents.PLAYER__CURRENCY_CHANGE, moneyChangeData);
    }

    public void RemoveMoney(int amount)
    {
        if (amount <= 0) return;

        _currency -= amount;

        // Notify listeners about the money change
        CurrencyChangeData moneyChangeData = new CurrencyChangeData(_currency, -amount);
        MessageDispatcher.SendMessageData(GameEvents.PLAYER__CURRENCY_CHANGE, moneyChangeData);
    }

    public void RemoveLives(int livesLost)
    {
        if (_lives <= 0 || livesLost <= 0) return;

        _lives -= livesLost;

        LivesChangeData livesLostData = new LivesChangeData(_lives, livesLost, _startingLives);
        MessageDispatcher.SendMessageData(GameEvents.PLAYER__LIVES_CHANGE, livesLostData);

        // End the game when player has no more lives
        if (lives <= 0)
            GameManager.Instance.LoseGame();
    }
    #endregion


    #region Event Handlers
    private void OnEnemyDestroy(IMessage message)
    {
        Enemy enemy = (Enemy)message.Data;

        AddMoney(enemy.reward);
    }

    private void OnEnemyReachExit(IMessage message)
    {
        Enemy enemy = (Enemy)message.Data;

        RemoveLives(enemy.damage);
    }

    private void OnWaveStart(IMessage message)
    {
        _round = (int)message.Data;
    }
    #endregion
}
