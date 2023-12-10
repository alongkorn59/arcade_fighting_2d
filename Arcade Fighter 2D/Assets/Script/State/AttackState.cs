using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    private PlayerController controller;
    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
        controller.Attack();

    }
    public void UpdateState()
    {
    }

    public void OnExit()
    {
    }
}
