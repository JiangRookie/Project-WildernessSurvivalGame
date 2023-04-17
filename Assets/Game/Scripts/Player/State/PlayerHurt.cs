public class PlayerHurt : PlayerStateBase
{
    public override void Enter()
    {
        m_PlayerCtrl.PlayerAnimation("Hurt");
    }
}