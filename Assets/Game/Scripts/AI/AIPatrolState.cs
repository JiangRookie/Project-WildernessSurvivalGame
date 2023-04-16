using UnityEngine;

public class AIPatrolState : AIStateBase
{
    Vector3 m_TargetPoint;

    public override void Enter()
    {
        m_AI.Agent.enabled = true;
        m_TargetPoint = m_AI.GetAIRandomPoint();
        m_AI.PlayerAnimation("Move");
        m_AI.Agent.SetDestination(m_TargetPoint);
    }

    public override void Update()
    {
        m_AI.SavePosition();
        if (Vector3.Distance(m_AI.transform.position, m_TargetPoint) < 0.5f)
        {
            m_AI.ChangeState(AIState.Idle);
        }
    }

    public override void Exit()
    {
        m_AI.Agent.enabled = false;
    }
}