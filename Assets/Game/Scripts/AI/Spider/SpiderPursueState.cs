using JKFrame;
using UnityEngine;

public class SpiderPursueState : AIPursueState
{
    SpiderController m_SpiderController;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        m_SpiderController = owner as SpiderController;
    }

    public override void Enter()
    {
        m_AI.Agent.Enable();
        m_AI.PlayAnimation("Run");
        m_AI.AddAnimationEvent("FootStep", FootStep);
        m_AI.Agent.speed = m_SpiderController.RunSpeed;
    }

    public override void Update()
    {
        if (GameSceneManager.Instance.IsGameOver) return;
        var distance = Vector3.Distance(m_AI.transform.position, PlayerController.Instance.transform.position);
        if (distance < m_AI.Radius + m_AI.AttackRange) // 在攻击范围
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

    public override void Exit()
    {
        base.Exit();
        m_AI.Agent.speed = m_SpiderController.WalkSpeed;
    }
}