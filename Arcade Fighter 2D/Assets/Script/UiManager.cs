using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button replayButton;

    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;

    [SerializeField] private GameManager gameManager;


    void Start()
    {
        resetButton.onClick.AddListener(OnResetGame);
        startGameButton.onClick.AddListener(OnStartNewGame);
        replayButton.onClick.AddListener(OnReplayPreviousGame);
    }

    private void OnResetGame()
    {
        gameManager.ResetPlayer();
        resetButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(true);
    }

    private void OnStartNewGame()
    {
        gameManager.Reset();
        replayButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
    }

    private void OnReplayPreviousGame()
    {
        gameManager.ResetPlayer();
        replayButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        gameManager.ReplayAllInputs();

    }

    private void OnDestroy()
    {
        resetButton.onClick.RemoveListener(OnResetGame);
        startGameButton.onClick.RemoveListener(OnStartNewGame);
        replayButton.onClick.RemoveListener(OnReplayPreviousGame);
    }

    public void OnEndGame()
    {
        resetButton.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);
    }

}
