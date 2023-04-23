public class AIDeadState : AIStateBase
{
    public override void Enter()
    {
        m_AI.InputCheckCollider.enabled = false;
        m_AI.PlayAnimation("Dead");
        m_AI.AddAnimationEvent("DeadOver", DeadOver);
    }

    public override void Exit()
    {
        m_AI.RemoveAnimationEvent("DeadOver", DeadOver);
        m_AI.InputCheckCollider.enabled = true;
    }

    void DeadOver() => m_AI.Dead();
}