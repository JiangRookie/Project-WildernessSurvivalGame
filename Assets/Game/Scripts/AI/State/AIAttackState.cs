using JKFrame;
using UnityEngine;

public class AIAttackState : AIStateBase
{
    bool m_IsAttacked = false;

    public override void Enter()
    {
        // 随机播放一个攻击动作
        int index = Random.Range(1, 3);
        m_AI.PlayAnimation("Attack_" + index.ToString());
        m_AI.transform.LookAt(PlayerController.Instance.transform);
        m_AI.PlayAudio("Attack");

        m_AI.AddAnimationEvent("StartHit", StartHit);
        m_AI.AddAnimationEvent("StopHit", StopHit);
        m_AI.AddAnimationEvent("AttackOver", AttackOver);
        m_AI.Weapon.OnTriggerStay(CheckHitOnTriggerStay);
    }

    public override void Exit()
    {
        m_AI.RemoveAnimationEvent("StartHit", StartHit);
        m_AI.RemoveAnimationEvent("StopHit", StopHit);
        m_AI.RemoveAnimationEvent("AttackOver", AttackOver);
        m_AI.Weapon.RemoveTriggerStay(CheckHitOnTriggerStay);
    }

    void StartHit() => m_AI.Weapon.Show();

    void CheckHitOnTriggerStay(Collider other, object[] args)
    {
        if (m_IsAttacked) return; // 避免一次攻击产生多次伤害
        if (other.gameObject.CompareTag("Player") == false) return;
        m_IsAttacked = true;
        m_AI.PlayAudio("Hit");
        PlayerController.Instance.Hurt(m_AI.AttackValue);
    }

    void StopHit()
    {
        m_IsAttacked = false;
        m_AI.Weapon.Hide();
    }

    void AttackOver() => m_AI.ChangeState(AIState.Pursue);
}