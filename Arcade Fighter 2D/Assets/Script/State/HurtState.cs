using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtState : IState
{
    private PlayerController controller;
    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
        controller.Hurt();
    }
    public void UpdateState()
    {
    }

    public void OnExit()
    {
    }
}
