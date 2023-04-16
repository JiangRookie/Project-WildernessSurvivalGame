using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIBase : MonoBehaviour, IStateMachineOwner
{
    [SerializeField] Animator m_Animator;
    [SerializeField] NavMeshAgent m_Agent;
    [SerializeField] MapVertexType m_MapVertexType;
    AIState m_CurrState;
    protected StateMachine m_StateMachine;
    protected MapChunkController mapChunkController;

    public NavMeshAgent Agent => m_Agent;

    public StateMachine StateMachine
    {
        get
        {
            if (m_StateMachine == null)
            {
                m_StateMachine = PoolManager.Instance.GetObject<StateMachine>();
                m_StateMachine.Init(this);
                return m_StateMachine;
            }
            return m_StateMachine;
        }
    }

    MapObjectData m_AIData;
    public MapObjectData AIData => m_AIData;

    public void Destroy()
    {
        this.JKGameObjectPushPool();
        m_CurrState = AIState.None;
        m_StateMachine.Stop();
    }

    public virtual void Init(MapChunkController chunk, MapObjectData aiData)
    {
        mapChunkController = chunk;
        m_AIData = aiData;
        transform.position = aiData.Position;
        ChangeState(AIState.Idle);
    }

    public virtual void ChangeState(AIState state)
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
            case AIState.Hurt: break;
            case AIState.Pursue: break;
            case AIState.Attack: break;
            case AIState.Die: break;
        }
    }

    public void PlayerAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        m_Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    public Vector3 GetAIRandomPoint()
    {
        return mapChunkController.GetAIRandomPoint(m_MapVertexType);
    }

    public void SavePosition()
    {
        m_AIData.Position = transform.position;
    }

    public virtual void RemoveOnMap() { }
}