using Project_WildernessSurvivalGame;
using UnityEngine;

public class AIPatrolState : AIStateBase
{
    Vector3 m_TargetPoint;

    public override void Enter()
    {
        m_AI.Agent.enabled = true;
        m_TargetPoint = m_AI.GetAIRandomPoint();
        m_AI.PlayAnimation("Move");
        m_AI.Agent.SetDestination(m_TargetPoint);
        m_AI.AddAnimationEvent("FootStep", FootStep);
    }

    public override void Exit()
    {
        m_AI.Agent.enabled = false;
        m_AI.RemoveAnimationEvent("FootStep", FootStep);
    }

    public override void Update()
    {
        m_AI.SavePosition();
        if (m_AI.HostileDistance > 0 && GameSceneManager.Instance.IsGameOver == false)
        {
            // 判断敌对距离
            if (Vector3.Distance(m_AI.transform.position, PlayerController.Instance.transform.position) < m_AI.HostileDistance)
            {
                // 进入追击状态
                m_AI.ChangeState(AIState.Pursue);
                return;
            }
        }
        if (Vector3.Distance(m_AI.transform.position, m_TargetPoint) < 0.5f)
        {
            m_AI.ChangeState(AIState.Idle);
        }
    }

    void FootStep()
    {
        int index = Random.Range(1, 3);
        m_AI.PlayAudio("FootStep" + index.ToString());
    }
}