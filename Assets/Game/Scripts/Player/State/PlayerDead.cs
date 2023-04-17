public class PlayerDead : PlayerStateBase
{
    public override void Enter()
    {
        m_PlayerCtrl.PlayerAnimation("Dead");
        m_PlayerCtrl.Collider.enabled = false;
    }
}