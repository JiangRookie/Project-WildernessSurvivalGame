using UnityEngine;

public class AIPursueState : AIStateBase
{
    public override void Enter()
    {
        m_AI.Agent.enabled = true;
        m_AI.PlayAnimation("Move");
        m_AI.AddAnimationEvent("FootStep", FootStep);
    }

    public override void Update()
    {
        if (GameSceneManager.Instance.IsGameOver == false)
        {
            if (Vector3.Distance(m_AI.transform.position, PlayerController.Instance.transform.position) < m_AI.Radius + m_AI.AttackRange)
            {
                m_AI.ChangeState(AIState.Attack);
            }
            else
            {
                m_AI.Agent.SetDestination(PlayerController.Instance.transform.position);
                m_AI.SavePosition();

                // 检测AI的归属地图快
                CheckAndTransferMapChunk();
            }
        }
    }

    protected void CheckAndTransferMapChunk()
    {
        // 通过 AI 所在坐标的地图块 和 AI 归属的地图块做比较
        MapChunkController newMapChunk = MapManager.Instance.GetMapChunk(m_AI.transform.position);
        if (newMapChunk != m_AI.MapChunk)
        {
            // 从当前地图块移除
            m_AI.MapChunk.RemoveAIObjectOnTransfer(m_AI.AIData.ID);

            // 加入新的地图块
            newMapChunk.AddAIObjectOnTransfer(m_AI.AIData, m_AI);
        }
    }

    protected void FootStep()
    {
        int index = Random.Range(1, 3);
        m_AI.PlayAudio("FootStep" + index.ToString());
    }

    public override void Exit()
    {
        m_AI.Agent.enabled = false;
        m_AI.RemoveAnimationEvent("FootStep", FootStep);
    }
}