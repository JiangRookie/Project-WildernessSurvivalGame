using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public abstract class CanHitMapObjectBase : MapObjectBase
{
    [SerializeField] Animator m_Animator;
    [SerializeField] AudioClip[] m_HurtAudioClips;
    [SerializeField] float m_MaxHp;
    [SerializeField] int m_LootConfigID = -1; // 死亡时掉落的配置ID
    float m_Hp;
    static readonly int s_Hurt = Animator.StringToHash("Hurt");

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);
        m_Hp = m_MaxHp;
    }

    public void Hurt(float damage)
    {
        m_Hp -= damage;
        if (m_Hp <= 0)
        {
            Dead();
        }
        else
        {
            m_Animator.SetTrigger(s_Hurt);
        }
        AudioManager.Instance.PlayOneShot(m_HurtAudioClips[Random.Range(0, m_HurtAudioClips.Length)], transform.position);
    }

    void Dead()
    {
        RemoveOnMap();
        if (m_LootConfigID == -1) return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.LOOT, m_LootConfigID);
        if (lootConfig == null) return;
        foreach (var lootConfigModel in lootConfig.LootConfigList)
        {
            int randomValue = Random.Range(1, 101);
            if (randomValue < lootConfigModel.Probability)
            {
                // 生成掉落物品
                Vector3 spawnPos = transform.position + Vector3.up;
                MapManager.Instance.SpawnMapObject(mapChunkController, lootConfigModel.LootObjectConfigID, spawnPos,false);
            }
        }
    }
}