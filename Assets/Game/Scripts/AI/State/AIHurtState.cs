public class AIHurtState : AIStateBase
{
    public override void Enter()
    {
        m_AI.PlayAnimation("Hurt");
        m_AI.PlayAudio("Hurt");
        m_AI.AddAnimationEvent("HurtOver", HurtOver);
    }

    public override void Exit()
    {
        m_AI.RemoveAnimationEvent("HurtOver", HurtOver);
    }

    void HurtOver()
    {
        m_AI.ChangeState(AIState.Pursue);
    }
}