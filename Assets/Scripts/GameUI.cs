using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    #region Attributes
    [SerializeField]
    private TextMeshProUGUI _currencyText;
    [SerializeField]
    private TextMeshProUGUI _livesText;
    [SerializeField]
    private TextMeshProUGUI _waveText;
    #endregion


    #region Unity Methods
    private void Awake()
    {
        _waveText.text = "-";

        MessageDispatcher.AddListener(GameEvents.PLAYER__CURRENCY_CHANGE, OnCurrencyChange);
        MessageDispatcher.AddListener(GameEvents.PLAYER__LIVES_CHANGE, OnLivesChange);
        MessageDispatcher.AddListener(GameEvents.WAVE__START, OnWaveStart);
    }

    private void OnDestroy()
    {
        MessageDispatcher.RemoveListener(GameEvents.PLAYER__CURRENCY_CHANGE, OnCurrencyChange);
        MessageDispatcher.RemoveListener(GameEvents.PLAYER__LIVES_CHANGE, OnLivesChange);
        MessageDispatcher.RemoveListener(GameEvents.WAVE__START, OnWaveStart);
    }
    #endregion


    #region Event Handlers
    private void OnCurrencyChange(IMessage message)
    {
        CurrencyChangeData currencyChange = (CurrencyChangeData)message.Data;

        _livesText.text = $"{currencyChange.balance}";
    }

    private void OnLivesChange(IMessage message)
    {
        LivesChangeData livesChange = (LivesChangeData)message.Data;

        _livesText.text = $"{livesChange.remaining} / {livesChange.starting}";
    }

    private void OnWaveStart(IMessage message)
    {
        int waveNumber = (int)message.Data;

        _waveText.text = $"{waveNumber}";
    }
    #endregion
}
