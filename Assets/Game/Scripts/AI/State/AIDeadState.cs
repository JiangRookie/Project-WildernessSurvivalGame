using JKFrame;

public class AIDeadState : AIStateBase
{
    public override void Enter()
    {
        m_AI.InputCheckCollider.Disable();
        m_AI.PlayAnimation("Dead");
        m_AI.AddAnimationEvent("DeadOver", DeadOver);
    }

    public override void Exit()
    {
        m_AI.InputCheckCollider.Enable();
        m_AI.RemoveAnimationEvent("DeadOver", DeadOver);
    }

    void DeadOver() => m_AI.Dead();
}