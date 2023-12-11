using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private FollowCamera followCamera;
    [SerializeField] private GameObject startGamePanel;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject replayPanel;

    [SerializeField] private Button startNewButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button switchCameraButton;

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
        switchCameraButton.onClick.AddListener(OnSwitchCamera);
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
        replayPanel.SetActive(true);
        followCamera.FocusIndex = 0;
        followCamera.StartFocusing();
    }

    public void OnEndGame()
    {
        ShowEndGameUi(true);
        replayPanel.SetActive(false);
        StopFocusing();
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

    private void OnSwitchCamera()
    {
        followCamera.SwitchTarget();
    }

    public void StopFocusing()
    {
        followCamera.StopFocusing();
    }

}
