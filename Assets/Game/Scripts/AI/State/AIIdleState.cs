using System.Collections;
using JKFrame;
using UnityEngine;

public class AIIdleState : AIStateBase
{
    Coroutine m_GoPatrolCoroutine;

    public override void Enter()
    {
        // 播放待机动画
        m_AI.PlayAnimation("Idle");

        // 休息一段时间然后去巡逻
        m_GoPatrolCoroutine = MonoManager.Instance.StartCoroutine(GoPatrolCoroutine());

        // 有一定概率发生叫声
        if (Random.Range(0, 30) == 0) m_AI.PlayAudio("Idle", 0.5f);
    }

    public override void Update()
    {
        if (m_AI.HostileDistance > 0 && GameSceneManager.Instance.IsGameOver == false)
        {
            // 判断敌对距离
            if (Vector3.Distance(m_AI.transform.position, PlayerController.Instance.transform.position) < m_AI.HostileDistance)
            {
                m_AI.ChangeState(AIState.Pursue);
            }
        }
    }

    IEnumerator GoPatrolCoroutine()
    {
        yield return CoroutineTool.WaitForSeconds(Random.Range(0f, 6f));
        m_AI.ChangeState(AIState.Patrol);
    }

    public override void Exit()
    {
        if (m_GoPatrolCoroutine != null)
        {
            MonoManager.Instance.StopCoroutine(m_GoPatrolCoroutine);
            m_GoPatrolCoroutine = null;
        }
    }
}