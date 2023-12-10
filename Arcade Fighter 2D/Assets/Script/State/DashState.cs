using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : IState
{
    private PlayerController controller;

    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
        controller.Dash();

    }
    public void UpdateState()
    {
        if (controller.IsReplaying || controller.IsDead)
            return;
        if (Input.GetKeyDown(InputFactory.GetKeyCode(controller.Player, ActionKey.Attack)))
        {
            controller.isDashAttack = true;
        }
    }

    public void OnExit()
    {

    }
}
