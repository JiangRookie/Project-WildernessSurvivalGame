using JKFrame;
using UnityEngine;

public class TreeController : MapObjectBase
{
    [SerializeField] Animator m_Animator;
    [SerializeField] AudioClip[] m_HurtAudioClips;
    [SerializeField] float m_MaxHp;
    float m_Hp;
    static readonly int s_Hurt = Animator.StringToHash("Hurt");

    void Start()
    {
        m_Hp = m_MaxHp;
    }

    public void Hurt(float damage)
    {
        m_Hp -= damage;
        if (m_Hp <= 0)
        {
            // 死亡
            Dead();
        }
        print("树：啊我受伤了");
        m_Animator.SetTrigger(s_Hurt);
        AudioManager.Instance.PlayOneShot(m_HurtAudioClips[Random.Range(0, m_HurtAudioClips.Length)], transform.position);
    }

    void Dead()
    {
        RemoveOnMap();
    }
}