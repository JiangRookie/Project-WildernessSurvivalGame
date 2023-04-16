using System.Collections;
using JKFrame;
using UnityEngine;

public class AIIdleState : AIStateBase
{
    Coroutine m_GoPatrolCoroutine;

    public override void Enter()
    {
        // 播放待机动画
        m_AI.PlayerAnimation("Idle");

        // 休息一段时间然后去巡逻
        MonoManager.Instance.StartCoroutine(GoPatrolCoroutine());
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