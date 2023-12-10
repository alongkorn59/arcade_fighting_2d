using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private GameObject startGamePanel;
    [SerializeField] private GameObject endGamePanel;

    [SerializeField] private Button resetButton;
    [SerializeField] private Button startNewButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button startButton;

    private GameManager gameManager;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    void Start()
    {
        startNewButton.onClick.AddListener(OnStartNewGame);
        replayButton.onClick.AddListener(OnReplayPreviousGame);
        startButton.onClick.AddListener(OnStartGame);
    }

    private void OnResetGame()
    {
        gameManager.ResetPlayer();
    }

    private void OnStartNewGame()
    {
        gameManager.Reset();
        gameManager.StartGame();
        ShowEndGameUi(false);
    }

    private void OnReplayPreviousGame()
    {
        gameManager.ReplayAllEvent();
        ShowEndGameUi(false);
    }

    public void OnEndGame()
    {
        ShowEndGameUi(true);
    }

    public void OnStartGame()
    {
        gameManager.StartGame();
        HideStartUi();
    }

    private void HideStartUi()
    {
        startGamePanel.SetActive(false);
    }
    private void ShowEndGameUi(bool isShow)
    {
        endGamePanel.SetActive(isShow);
    }

    private void OnDestroy()
    {
        startNewButton.onClick.RemoveListener(OnStartNewGame);
        replayButton.onClick.RemoveListener(OnReplayPreviousGame);
    }

}
