public interface IState
{
    public void OnEnter(PlayerController controller);
    public void UpdateState();
    public void OnExit();
}
