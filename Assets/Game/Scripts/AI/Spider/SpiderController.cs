using UnityEngine;

public class SpiderController : AIBase
{
    [SerializeField] float m_WalkSpeed = 3;
    [SerializeField] float m_RunSpeed = 4;
    [SerializeField] float m_RetreatDistance = 4; // 撤退距离
    public float WalkSpeed => m_WalkSpeed;
    public float RunSpeed => m_RunSpeed;
    public float RetreatDistance => m_RetreatDistance;

    public override void ChangeState(AIState state)
    {
        m_CurrState = state;
        switch (state)
        {
            case AIState.Idle:
                StateMachine.ChangeState<AIIdleState>((int)state);
                break;
            case AIState.Patrol:
                StateMachine.ChangeState<AIPatrolState>((int)state);
                break;
            case AIState.Hurt:
                StateMachine.ChangeState<AIHurtState>((int)state, true);
                break;
            case AIState.Pursue:
                StateMachine.ChangeState<SpiderPursueState>((int)state);
                break;
            case AIState.Attack:
                StateMachine.ChangeState<AIAttackState>((int)state);
                break;
            case AIState.Dead:
                StateMachine.ChangeState<AIDeadState>((int)state);
                break;
        }
    }
}