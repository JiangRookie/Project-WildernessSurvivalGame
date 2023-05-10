public class PlayerDead : PlayerStateBase
{
    public override void Enter()
    {
        m_PlayerCtrl.PlayAnimation("Dead");
        m_PlayerCtrl.Collider.enabled = false;
    }
}