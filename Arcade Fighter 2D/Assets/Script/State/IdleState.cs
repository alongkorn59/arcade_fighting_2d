using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private PlayerController controller;
    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
    }
    public void UpdateState()
    {
        if (controller.IsReplaying)
            return;
        controller.BasicMovement();
        if (Input.GetKeyDown(InputFactory.GetKeyCode(controller.Player, ActionKey.Attack)))
        {
            controller.ChangeState(PlayerStateType.Attack);
        }
        if (Input.GetKeyDown(InputFactory.GetKeyCode(controller.Player, ActionKey.Dash)) && controller.IsDashAble)
        {
            controller.ChangeState(PlayerStateType.Dash);
        }
        if (Input.GetKeyDown(InputFactory.GetKeyCode(controller.Player, ActionKey.Block)))
        {
            controller.ChangeState(PlayerStateType.Block);
        }
    }

    public void OnExit()
    {
    }
}
