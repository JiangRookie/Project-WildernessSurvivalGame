using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public enum PlayerState
    {
        Idle, Move, Attack, BeAttack, Dead
    }

    public class PlayerController : SingletonMono<PlayerController>, IStateMachineOwner
    {
        public Animator Animator;
        public CharacterController CharacterController;
        [SerializeField] PlayerModel m_PlayerModel;
        [HideInInspector] public Vector2 PositionXScope;
        [HideInInspector] public Vector2 PositionZScope;
        PlayerConfig m_PlayerConfig;
        StateMachine m_StateMachine;

        public Transform PlayerTransform { get; private set; }
        public float MoveSpeed => m_PlayerConfig.MoveSpeed;
        public float RotateSpeed => m_PlayerConfig.RotateSpeed;
        public bool CanUseItem { get; private set; } = true;

        void Update()
        {
            if (GameSceneManager.Instance.IsInitialized == false) return;
            CalculateHungryOnUpdate();
        }

        void OnDestroy()
        {
            // 把存档数据实际写入磁盘
            m_PlayerTransformData.Position = PlayerTransform.localPosition;
            m_PlayerTransformData.Rotation = PlayerTransform.localRotation.eulerAngles;
            ArchiveManager.Instance.SavePlayerTransformData();
            ArchiveManager.Instance.SavePlayerCoreData();
            ArchiveManager.Instance.SaveInventoryData();
        }

        public void Init(float mapSizeOnWorld)
        {
            m_PlayerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.PLAYER);
            m_PlayerTransformData = ArchiveManager.Instance.PlayerTransformData;
            m_PlayerCoreData = ArchiveManager.Instance.PlayerCoreData;

            m_PlayerModel.Init(PlayAudioOnFootStep);
            PlayerTransform = transform;

            m_StateMachine = ResManager.Load<StateMachine>();
            m_StateMachine.Init(this);

            // 设置初始状态为待机状态
            m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle);
            InitPositionScope(mapSizeOnWorld);

            // 初始化存档相关的数据
            PlayerTransform.localPosition = m_PlayerTransformData.Position;
            PlayerTransform.localRotation = Quaternion.Euler(m_PlayerTransformData.Rotation);

            TriggerUpdateHpEvent();
            TriggerUpdateHungryEvent();
        }

        /// <summary>
        /// 初始化坐标范围
        /// </summary>
        /// <param name="mapSizeOnWorld"></param>
        void InitPositionScope(float mapSizeOnWorld)
        {
            PositionXScope = new Vector2(1, mapSizeOnWorld - 1);
            PositionZScope = new Vector2(1, mapSizeOnWorld - 1);
        }

        void PlayAudioOnFootStep(int index)
        {
            AudioManager.Instance.PlayOneShot(m_PlayerConfig.FootStepAudioClips[index], PlayerTransform.position
                                            , m_PlayerConfig.FootStepVolume);
        }

        #region 数值

        void CalculateHungryOnUpdate()
        {
            if (m_PlayerCoreData.Hungry > 0)
            {
                m_PlayerCoreData.Hungry -= Time.deltaTime * m_PlayerConfig.HungryReducingSpeed;
                if (m_PlayerCoreData.Hungry < 0) m_PlayerCoreData.Hungry = 0;
                TriggerUpdateHungryEvent();
            }
            else
            {
                if (m_PlayerCoreData.Hp > 0)
                {
                    m_PlayerCoreData.Hp -= Time.deltaTime * m_PlayerConfig.HpReducingSpeedOnHungryIsZero;
                    if (m_PlayerCoreData.Hp < 0)
                    {
                        m_PlayerCoreData.Hp = 0;
                    }
                    TriggerUpdateHpEvent();
                }
            }
        }

        void TriggerUpdateHpEvent()
        {
            EventManager.EventTrigger(EventName.UpdatePlayerHp, m_PlayerCoreData.Hp);
        }

        void TriggerUpdateHungryEvent()
        {
            EventManager.EventTrigger(EventName.UpdatePlayerHungry, m_PlayerCoreData.Hungry);
        }

        public void RecoverHp(float value)
        {
            m_PlayerCoreData.Hp = Mathf.Clamp(m_PlayerCoreData.Hp + value, 0, m_PlayerConfig.MaxHp);
            TriggerUpdateHpEvent();
        }

        public void RecoverHungry(float value)
        {
            m_PlayerCoreData.Hungry = Mathf.Clamp(m_PlayerCoreData.Hungry + value, 0, m_PlayerConfig.MaxHungry);
            TriggerUpdateHungryEvent();
        }

        #endregion

        #region 战斗

        ItemData m_CurrentWeaponItemData;
        GameObject m_CurrentWeaponGameObject;

        public void ChangeWeapon(ItemData newWeapon)
        {
            // 没切换武器
            if (m_CurrentWeaponItemData == newWeapon)
            {
                m_CurrentWeaponItemData = newWeapon;
                return;
            }

            // 旧武器如果有数据，就把旧武器模型回收到对象池
            if (m_CurrentWeaponItemData != null)
            {
                m_CurrentWeaponGameObject.JKGameObjectPushPool();
            }

            // 新武器如果不是null
            if (newWeapon != null)
            {
                Item_WeaponInfo info = newWeapon.Config.ItemTypeInfo as Item_WeaponInfo;
                m_CurrentWeaponGameObject
                    = PoolManager.Instance.GetGameObject(info.PrefabOnPlayer, m_PlayerModel.WeaponRoot);
                m_CurrentWeaponGameObject.transform.localPosition = info.PositionOnPlayer;
                m_CurrentWeaponGameObject.transform.localRotation = Quaternion.Euler(info.RotationOnPlayer);
                Animator.runtimeAnimatorController = info.AnimatorOverrideController;
            }
            else // 新武器是 null，意味着空手
            {
                Animator.runtimeAnimatorController = m_PlayerConfig.NormalAnimatorController;
            }

            // 由于动画是有限状态机驱动的，如果不重新激活一次动画，动画会出现错误
            // （比如在移动中突然切换AnimatorOverrideController会不播放走路动画）
            m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle, true);
            m_CurrentWeaponItemData = newWeapon;
        }

        #endregion

        #region 存档

        PlayerTransformData m_PlayerTransformData;
        PlayerCoreData m_PlayerCoreData;

        #endregion
    }
}