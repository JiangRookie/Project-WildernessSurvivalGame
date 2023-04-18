using System;
using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIBase : SerializedMonoBehaviour, IStateMachineOwner
{
    #region Component

    [SerializeField] protected Animator m_Animator;
    [SerializeField] protected NavMeshAgent m_Agent;
    [SerializeField] protected Collider m_InputCheckCollider;
    [SerializeField] protected Transform m_Weapon;
    public NavMeshAgent Agent => m_Agent;
    public Collider InputCheckCollider => m_InputCheckCollider;
    public Transform Weapon => m_Weapon;

    #endregion

    #region Other

    [SerializeField] protected MapVertexType m_MapVertexType;
    [SerializeField] protected float m_Radius;      // 交互距离
    [SerializeField] protected float m_AttackRange; // 交互距离
    [SerializeField] protected float m_MaxHp = 10;
    [SerializeField] protected float m_AttackValue = 10;
    [SerializeField] protected int m_LootConfigID = -1;      // 死亡时掉落的配置ID
    [SerializeField] protected float m_HostileDistance = -1; // 敌对距离，-1代表无效
    [SerializeField] protected Dictionary<string, AudioClip> m_AudioClipDict = new Dictionary<string, AudioClip>();

    protected MapChunkController m_MapChunk;
    protected MapObjectData m_AIData;
    protected StateMachine m_StateMachine;
    protected AIState m_CurrState;
    protected float m_Hp;

    public float Radius => m_Radius;
    public float AttackRange => m_AttackRange;
    public float AttackValue => m_AttackValue;
    public MapChunkController MapChunk => m_MapChunk;
    public MapObjectData AIData => m_AIData;

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

    public float HostileDistance => m_HostileDistance;

    #endregion

    public void Destroy()
    {
        this.JKGameObjectPushPool();
        m_CurrState = AIState.None;
        m_StateMachine.Stop();
    }

    public void Dead()
    {
        // 告知地图块移除自己
        m_MapChunk.RemoveAIObject(m_AIData.ID);
        if (m_LootConfigID == -1) return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.Loot, m_LootConfigID);
        if (lootConfig != null)
        {
            lootConfig.GenerateMapObject(m_MapChunk, transform.position + Vector3.up);
        }
    }

    public virtual void Init(MapChunkController mapChunk, MapObjectData aiData)
    {
        m_MapChunk = mapChunk;
        m_AIData = aiData;
        transform.position = aiData.Position;
        m_Hp = m_MaxHp;
        ChangeState(AIState.Idle);
    }

    public virtual void InitOnTransfer(MapChunkController mapChunk)
    {
        m_MapChunk = mapChunk;
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
            case AIState.Hurt:
                StateMachine.ChangeState<AIHurtState>((int)state, true);
                break;
            case AIState.Pursue:
                StateMachine.ChangeState<AIPursueState>((int)state);
                break;
            case AIState.Attack:
                StateMachine.ChangeState<AIAttackState>((int)state);
                break;
            case AIState.Dead:
                StateMachine.ChangeState<AIDeadState>((int)state);
                break;
        }
    }

    public void PlayAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        m_Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    public Vector3 GetAIRandomPoint()
    {
        return m_MapChunk.GetAIRandomPoint(m_MapVertexType);
    }

    public void SavePosition()
    {
        m_AIData.Position = transform.position;
    }

    public virtual void Hurt(float damage)
    {
        if (m_Hp == 0) return;
        m_Hp -= damage;

        // 死亡
        ChangeState(m_Hp <= 0 ? AIState.Dead : AIState.Hurt);
    }

    public void PlayAudio(string audioName, float volumeScale = 1)
    {
        if (m_AudioClipDict.TryGetValue(audioName, out AudioClip audioClip))
        {
            AudioManager.Instance.PlayOneShot(audioClip, transform.position, volumeScale);
        }
    }

    #region 动画事件

    Dictionary<string, Action> m_AnimationEventDict = new Dictionary<string, Action>();

    void AnimationEvent(string eventName)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action))
        {
            action?.Invoke();
        }
    }

    public void AddAnimationEvent(string eventName, Action animationAction)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action))
        {
            action += animationAction;
        }
        else
        {
            m_AnimationEventDict.Add(eventName, animationAction);
        }
    }

    public void RemoveAnimationEvent(string eventName, Action animationAction)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action))
        {
            action -= animationAction;
        }
    }

    public void RemoveAnimationEvent(string eventName)
    {
        m_AnimationEventDict.Remove(eventName);
    }

    public void CleanAllAnimationEvent()
    {
        m_AnimationEventDict.Clear();
    }

    #endregion
}