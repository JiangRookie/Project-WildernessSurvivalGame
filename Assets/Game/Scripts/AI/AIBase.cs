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
    public NavMeshAgent Agent => m_Agent;
    [SerializeField] protected Collider m_InputCheckCollider;
    public Collider InputCheckCollider => m_InputCheckCollider;
    [SerializeField] protected Transform m_Weapon;
    public Transform Weapon => m_Weapon;

    #endregion

    #region Other

    [SerializeField] protected MapVertexType m_MapVertexType;

    [SerializeField] protected float m_Radius; // 交互距离
    public float Radius => m_Radius;
    [SerializeField] protected float m_AttackRange;
    public float AttackRange => m_AttackRange;
    [SerializeField] protected float m_AttackValue = 10;
    public float AttackValue => m_AttackValue;
    [SerializeField] protected float m_HostileDistance = -1; // 敌对距离，-1代表无效
    public float HostileDistance => m_HostileDistance;

    [SerializeField] protected float m_MaxHp = 10;
    float m_Hp;
    protected AIState m_CurrState;
    [SerializeField] protected int m_LootConfigID = -1; // 死亡时掉落的配置ID
    [SerializeField] protected Dictionary<string, AudioClip> m_AudioClipDict = new Dictionary<string, AudioClip>();

    MapChunkController m_MapChunk;
    public MapChunkController MapChunk => m_MapChunk;
    MapObjectData m_AIData;
    public MapObjectData AIData => m_AIData;

    StateMachine m_StateMachine;

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

    #endregion

    public void Init(MapChunkController mapChunk, MapObjectData aiData)
    {
        m_MapChunk = mapChunk;
        m_AIData = aiData;
        transform.position = aiData.Position;
        m_Hp = m_MaxHp;
        ChangeState(AIState.Idle);
    }

    public void InitOnTransfer(MapChunkController mapChunk) => m_MapChunk = mapChunk;

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
        => m_Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);

    public void PlayAudio(string audioName, float volumeScale = 1)
    {
        if (m_AudioClipDict.TryGetValue(audioName, out AudioClip audioClip))
        {
            AudioManager.Instance.PlayOneShot(audioClip, transform.position, volumeScale);
        }
    }

    public Vector3 GetAIRandomPoint() => m_MapChunk.GetAIRandomPoint(m_MapVertexType);

    public void SavePosition() => m_AIData.Position = transform.position;

    public void Hurt(float damage)
    {
        if (m_Hp <= 0) return;
        m_Hp -= damage;
        ChangeState(m_Hp <= 0 ? AIState.Dead : AIState.Hurt);
    }

    public void Destroy()
    {
        this.PushGameObj2Pool();
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

    #region 动画事件

    Dictionary<string, Action> m_AnimationEventDict = new Dictionary<string, Action>(5);

    void AnimationEvent(string eventName)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action)) action?.Invoke();
    }

    /// <summary>
    /// 添加动画事件
    /// </summary>
    public void AddAnimationEvent(string eventName, Action animationAction)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action))
        {
            // 如果eventName已经存在，先移除原有的eventName对应的Action
            action -= animationAction;
            m_AnimationEventDict[eventName] = action + animationAction;
        }
        else
        {
            m_AnimationEventDict.Add(eventName, animationAction);
        }
    }

    /// <summary>
    /// 移除指定的动画事件
    /// </summary>
    public void RemoveAnimationEvent(string eventName, Action animationAction)
    {
        if (m_AnimationEventDict.TryGetValue(eventName, out Action action))
        {
            action -= animationAction;
        }
    }

    /// <summary>
    /// 移除指定的动画事件
    /// </summary>
    public void RemoveAnimationEvent(string eventName)
    {
        if (m_AnimationEventDict.ContainsKey(eventName))
        {
            // 如果eventName存在，移除对应的Action
            m_AnimationEventDict.Remove(eventName);
        }
    }

    /// <summary>
    /// 清空所有的动画事件
    /// </summary>
    public void CleanAllAnimationEvent() => m_AnimationEventDict.Clear();

    #endregion
}