using UnityEngine;
using System.Collections;

public partial class PlayerController : MonoBehaviour
{
    private IState playerState = new IdleState();
    [SerializeField] private PlayerStateType currentState = PlayerStateType.Idle;

    public void ChangeState(PlayerStateType state)
    {
        currentState = state;
        switch (state)
        {
            case PlayerStateType.Idle:
                ChangeState(new IdleState());
                break;
            case PlayerStateType.Jump:
                ChangeState(new JumpState());
                break;
            case PlayerStateType.Block:
                ChangeState(new BlockState());
                break;
            case PlayerStateType.Attack:
                ChangeState(new AttackState());
                break;
            case PlayerStateType.Dash:
                ChangeState(new DashState());
                break;
            case PlayerStateType.Hurt:
                ChangeState(new HurtState());
                break;
            default:
                ChangeState(new IdleState());
                break;
        }
    }

    private void ChangeState(IState newState)
    {
        playerState.OnExit();
        playerState = newState;
        playerState.OnEnter(this);
    }

}
public enum PlayerStateType
{
    Idle,
    Run,
    Jump,
    Block,
    Attack,
    Dash,
    Hurt
}