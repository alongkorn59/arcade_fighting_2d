using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;
    [SerializeField] private PlayerController player1;
    public PlayerController Player1 => player1;
    [SerializeField] private PlayerController player2;
    public PlayerController Player2 => player2;

    [SerializeField] private List<InputRecord> player1InputRecords = new List<InputRecord>();
    [SerializeField] private List<InputRecord> player2InputRecords = new List<InputRecord>();

    [SerializeField] private DeadRecord deadRecord;

    private float gameStartTime;
    private bool isStopRecord = true;
    private float recordInterval = 0.1f;

    public bool isReplaying = false;
    private bool isGameStarted = false;
    public void StartGame()
    {
        isGameStarted = true;
        player1.isGameStart = isGameStarted;
        player2.isGameStart = isGameStarted;
        isStopRecord = false;
    }
    private void Start()
    {
        uiManager.Init(this);
        gameStartTime = Time.time;
        player1.OnPlayerDead += OnPlayerDead;
        player2.OnPlayerDead += OnPlayerDead;
    }

    private void Update()
    {
        if (!isStopRecord)
        {
            RecordInput(PlayerType.Player1);
            RecordInput(PlayerType.Player2);
        }
    }

    public void RecordInput(PlayerType playerType)
    {
        float horizontalInput = Input.GetAxisRaw(InputFactory.GetInputAxisMovement(playerType));
        bool jumpInput = Input.GetKeyDown(InputFactory.GetKeyCode(playerType, ActionKey.Jump));
        bool attackInput = Input.GetKeyDown(InputFactory.GetKeyCode(playerType, ActionKey.Attack));
        bool blockInput = Input.GetKeyDown(InputFactory.GetKeyCode(playerType, ActionKey.Block));
        bool dashInput = Input.GetKeyDown(InputFactory.GetKeyCode(playerType, ActionKey.Dash));


        Vector2 playerPosition;
        if (playerType == PlayerType.Player1)
            playerPosition = player1.transform.position;
        else
            playerPosition = player2.transform.position;

        if (horizontalInput != 0 || jumpInput || attackInput || blockInput || dashInput)
        {
            float timestamp = Time.time - gameStartTime;
            timestamp = Mathf.Floor(timestamp / recordInterval) * recordInterval;

            InputRecord inputRecord = new InputRecord
            {
                timestamp = timestamp,
                horizontalInput = horizontalInput,
                jumpInput = jumpInput,
                attackInput = attackInput,
                blockInput = blockInput,
                dashInput = dashInput,
                playerPosition = playerPosition  // Record player position
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
    public void ReplayAllEvent()
    {
        player1.Reset();
        player2.Reset();
        player1.IsReplaying = true;
        player2.IsReplaying = true;
        StartCoroutine(ReplayInputsCoroutine());
    }

    bool isReplayEnd1 = false;
    bool isReplayEnd2 = false;
    bool isAllReplayEnd => isReplayEnd1 && isReplayEnd2;

    private IEnumerator ReplayInputsCoroutine()
    {
        isReplaying = true;
        // Replay input for Player 1
        StartCoroutine(ReplayPlayerInputs(player1InputRecords, PlayerType.Player1));
        // Replay input for Player 2
        StartCoroutine(ReplayPlayerInputs(player2InputRecords, PlayerType.Player2));
        yield return StartCoroutine(ReplayHpUpdate(deadRecord));
        yield return new WaitUntil(() => isAllReplayEnd);
        player1.IsReplaying = false;
        player2.IsReplaying = false;
        isReplayEnd1 = false;
        isReplayEnd2 = false;

    }
    IEnumerator ReplayPlayerInputs(List<InputRecord> inputRecords, PlayerType playerType)
    {
        float startTime = Time.time;

        foreach (InputRecord inputRecord in inputRecords)
        {
            float adjustedTimestamp = startTime + inputRecord.timestamp;

            yield return new WaitForSeconds(adjustedTimestamp - Time.time);

            ApplyReplayedInput(playerType, inputRecord);
        }
        if (playerType == PlayerType.Player1)
            isReplayEnd1 = true;
        else
            isReplayEnd2 = true;
    }

    IEnumerator ReplayHpUpdate(DeadRecord deadRecord)
    {
        float startTime = Time.time;
        float adjustedTimestamp = startTime + deadRecord.timestamp;
        yield return new WaitForSeconds(adjustedTimestamp - Time.time);
        PlayerController playerController = GetPlayerController(deadRecord.playerType);
        playerController.ApplyReplayDead();
    }

    private void ApplyReplayedInput(PlayerType playerType, InputRecord inputRecord)
    {
        PlayerController playerController = GetPlayerController(playerType);

        if (playerController != null)
        {
            // Apply the replayed input to the player controller
            playerController.ApplyReplayMovement(inputRecord.horizontalInput, inputRecord.jumpInput);
            playerController.ApplyReplayInput(inputRecord.attackInput, inputRecord.blockInput, inputRecord.dashInput, inputRecord.playerPosition);
        }
    }

    private PlayerController GetPlayerController(PlayerType playerType)
    {
        return playerType == PlayerType.Player1 ? player1 : player2;
    }

    private void OnPlayerDead(PlayerType player)
    {
        if (!isReplaying)
        {
            float timestamp = Time.time - gameStartTime;
            timestamp = Mathf.Floor(timestamp / recordInterval) * recordInterval;
            deadRecord = new DeadRecord(player, timestamp);
        }
        StartCoroutine(DelayEndGame());
    }

    IEnumerator DelayEndGame()
    {
        yield return new WaitForSeconds(3f);
        isStopRecord = true;
        player1.isGameStart = false;
        player2.isGameStart = false;
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
    public bool dashInput;
    public Vector2 playerPosition;
}

public class DeadRecord
{
    public PlayerType playerType;
    public float timestamp;
    public DeadRecord(PlayerType playerType, float timestamp)
    {
        this.playerType = playerType;
        this.timestamp = timestamp;
    }
}
