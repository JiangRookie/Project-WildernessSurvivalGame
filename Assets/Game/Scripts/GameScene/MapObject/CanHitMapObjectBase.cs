using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public abstract class CanHitMapObjectBase : MapObjectBase
{
    [SerializeField] Animator m_Animator;
    [SerializeField] AudioClip[] m_HurtAudioClips;
    [SerializeField] float m_MaxHp;
    float m_Hp;
    static readonly int s_Hurt = Animator.StringToHash("Hurt");

    public override void Init(MapChunkController mapChunkController, ulong id)
    {
        base.Init(mapChunkController, id);
        m_Hp = m_MaxHp;
    }

    public void Hurt(float damage)
    {
        m_Hp -= damage;
        if (m_Hp <= 0)
        {
            Dead();
        }
        m_Animator.SetTrigger(s_Hurt);
        AudioManager.Instance.PlayOneShot(m_HurtAudioClips[Random.Range(0, m_HurtAudioClips.Length)], transform.position);
    }

    void Dead()
    {
        RemoveOnMap();
    }
}