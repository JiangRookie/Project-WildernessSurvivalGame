using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public class SpiderPursueState : AIPursueState
{
    protected SpiderController m_SpiderController;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        m_SpiderController = owner as SpiderController;
    }

    public override void Enter()
    {
        m_AI.Agent.enabled = true;
        m_AI.PlayAnimation("Run");
        m_AI.AddAnimationEvent("FootStep", FootStep);
        m_AI.Agent.speed = m_SpiderController.RunSpeed;
    }

    public override void Update()
    {
        if (GameSceneManager.Instance.IsGameOver == false)
        {
            var distance = Vector3.Distance(m_AI.transform.position, PlayerController.Instance.transform.position);
            if (distance < m_AI.Radius + m_AI.AttackRange)
            {
                m_AI.ChangeState(AIState.Attack);
            }
            else
            {
                m_AI.Agent.SetDestination(PlayerController.Instance.transform.position);
                m_AI.SavePosition();

                // 如果远离则待机
                if (distance > m_SpiderController.RetreatDistance)
                {
                    m_AI.ChangeState(AIState.Idle);
                    return;
                }

                // 检测AI的归属地图快
                CheckAndTransferMapChunk();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        m_AI.Agent.speed = m_SpiderController.WalkSpeed;
    }
}