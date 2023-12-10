using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputFactory
{
    public static KeyCode GetKeyCode(PlayerType playerType, ActionKey key)
    {
        switch (key)
        {
            case ActionKey.Attack:
                return playerType == PlayerType.Player1 ? KeyCode.K : KeyCode.Keypad2;
            case ActionKey.Jump:
                return playerType == PlayerType.Player1 ? KeyCode.Space : KeyCode.Keypad0;
            case ActionKey.Block:
                return playerType == PlayerType.Player1 ? KeyCode.J : KeyCode.Keypad1;
            case ActionKey.Skill:
                return playerType == PlayerType.Player1 ? KeyCode.L : KeyCode.Keypad3;
            case ActionKey.Dash:
                return playerType == PlayerType.Player1 ? KeyCode.LeftShift : KeyCode.KeypadPeriod;
            default:
                return KeyCode.Keypad3;
        }
    }
    public static string GetInputAxisMovement(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
        {
            return "Horizontal1";
        }
        else
            return "Horizontal2";
    }
}

public enum ActionKey
{
    Attack,
    Jump,
    Block,
    Skill,
    Dash
}
