using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;

    // Input records for Player 1 and Player 2
    [SerializeField] private List<InputRecord> player1InputRecords = new List<InputRecord>();
    [SerializeField] private List<InputRecord> player2InputRecords = new List<InputRecord>();

    private float gameStartTime;
    public bool isStopRecord = false;

    private void Start()
    {
        // Record the start time of the game
        gameStartTime = Time.time;
        player1.OnPlayerDead += OnPlayerDead;
        player2.OnPlayerDead += OnPlayerDead;
    }

    private void Update()
    {
        if (!isStopRecord)
        {
            // Record input for Player 1
            RecordInput(PlayerType.Player1);

            // Record input for Player 2
            RecordInput(PlayerType.Player2);
        }
    }

    private void RecordInput(PlayerType playerType)
    {
        float horizontalInput = Input.GetAxis(InputFactory.GetInputAxisMovement(playerType));
        bool jumpInput = Input.GetKey(InputFactory.GetKeyCode(playerType, ActionKey.Jump));
        bool attackInput = Input.GetKey(InputFactory.GetKeyCode(playerType, ActionKey.Attack));
        bool blockInput = Input.GetKey(InputFactory.GetKeyCode(playerType, ActionKey.Block));
        bool skillInput = Input.GetKey(InputFactory.GetKeyCode(playerType, ActionKey.Skill));
        bool dashInput = Input.GetKey(InputFactory.GetKeyCode(playerType, ActionKey.Dash));

        // Check if any input is active before adding a new record
        if (horizontalInput != 0 || jumpInput || attackInput || blockInput || skillInput || dashInput)
        {
            InputRecord inputRecord = new InputRecord
            {
                timestamp = Time.time - gameStartTime,
                horizontalInput = horizontalInput,
                jumpInput = jumpInput,
                attackInput = attackInput,
                blockInput = blockInput,
                skillInput = skillInput,
                dashInput = dashInput
            };

            if (playerType == PlayerType.Player1)
            {
                player1InputRecords.Add(inputRecord);
            }
            else if (playerType == PlayerType.Player2)
            {
                player2InputRecords.Add(inputRecord);
            }
        }
    }

    public List<InputRecord> GetPlayer1InputRecords()
    {
        return player1InputRecords;
    }

    public List<InputRecord> GetPlayer2InputRecords()
    {
        return player2InputRecords;
    }

    public void Reset()
    {
        player1.Reset();
        player2.Reset();
        player1InputRecords = new List<InputRecord>();
        player2InputRecords = new List<InputRecord>();
        isStopRecord = false;
    }

    public void ResetPlayer()
    {
        player1.Reset();
        player2.Reset();
    }
    public void ReplayAllInputs()
    {
        player1.Reset();
        player2.Reset();
        player1.IsReplaying = true;
        player2.IsReplaying = true;
        StartCoroutine(ReplayInputsCoroutine());
    }

    private IEnumerator ReplayInputsCoroutine()
    {
        // Replay input for Player 1
        StartCoroutine(ReplayPlayerInputs(player1InputRecords, PlayerType.Player1));

        // Replay input for Player 2
        StartCoroutine(ReplayPlayerInputs(player2InputRecords, PlayerType.Player2));
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator ReplayPlayerInputs(List<InputRecord> inputRecords, PlayerType playerType)
    {
        float startTime = Time.time;

        foreach (InputRecord inputRecord in inputRecords)
        {
            // Adjust the timestamp based on the time elapsed since the start of the game
            float adjustedTimestamp = startTime + inputRecord.timestamp;

            // Wait until it's time to apply the input
            yield return new WaitForSeconds(adjustedTimestamp - Time.time);

            // Apply the replayed input to the corresponding player
            ApplyReplayedInput(playerType, inputRecord);

            // Optionally, you can use this time to simulate the game state based on the replayed input
        }
    }

    private void ApplyReplayedInput(PlayerType playerType, InputRecord inputRecord)
    {
        // Retrieve the appropriate PlayerController based on playerType
        PlayerController playerController = GetPlayerController(playerType);

        if (playerController != null)
        {
            // Apply the replayed input to the player controller
            playerController.ApplyInput(inputRecord.horizontalInput, inputRecord.jumpInput, inputRecord.attackInput, inputRecord.blockInput, inputRecord.skillInput, inputRecord.dashInput);
        }
    }

    private PlayerController GetPlayerController(PlayerType playerType)
    {
        // You need to implement logic to get the correct PlayerController based on playerType
        // This might involve finding the corresponding GameObject or using some other mechanism
        // For example, assuming each player has a PlayerController component on their GameObject:

        return playerType == PlayerType.Player1 ? player1 : player2;
    }

    private void OnPlayerDead()
    {
        isStopRecord = true;
        uiManager.OnEndGame();
    }
}
[Serializable]
public class InputRecord
{
    public float timestamp;
    public float horizontalInput;
    public bool jumpInput;
    public bool attackInput;
    public bool blockInput;
    public bool skillInput;
    public bool dashInput;
}
