using System;
using System.Collections.Generic;
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

        #region 初始化

        public void Init(float mapSizeOnWorld)
        {
            m_PlayerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.PLAYER);
            m_PlayerTransformData = ArchiveManager.Instance.PlayerTransformData;
            m_PlayerCoreData = ArchiveManager.Instance.PlayerCoreData;

            m_PlayerModel.Init(PlayAudioOnFootStep, OnStartHit, OnStopHit, OnAttackOver);
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

        #endregion

        #region 工具

        List<MapObjectBase> m_LastAttackedMapObjectList = new();

        void PlayAudioOnFootStep(int index)
        {
            AudioManager.Instance.PlayOneShot(m_PlayerConfig.FootStepAudioClips[index], PlayerTransform.position
                                            , m_PlayerConfig.FootStepVolume);
        }

        // 让武器开启伤害检测
        void OnStartHit()
        {
            m_AttackSucceedCount = 0;
            m_CurrentWeaponGameObject.transform.OnTriggerEnter(OnWeaponTriggerEnter);
        }

        // 让武器停止伤害检测
        void OnStopHit()
        {
            m_CurrentWeaponGameObject.transform.RemoveTriggerEnter(OnWeaponTriggerEnter);
            m_LastAttackedMapObjectList.Clear();
        }

        int m_AttackSucceedCount; // 攻击成功的数量

        // 整个攻击状态的结束
        void OnAttackOver()
        {
            // 成功命中过几次就消耗几次耐久度
            for (int i = 0; i < m_AttackSucceedCount; i++)
            {
                // 让武器受损
                EventManager.EventTrigger(EventName.PlayerWeaponAttackSucceed);
            }

            m_CanAttack = true;

            // 切换状态到待机
            ChangeState(PlayerState.Idle);

            // 允许使用物品
            CanUseItem = true;
        }

        /// <summary>
        /// 当武器碰到其他游戏物体时
        /// </summary>
        /// <param name="other"></param>
        /// <param name="arg2"></param>
        void OnWeaponTriggerEnter(Collider other, object[] arg2)
        {
            // 对方得是地图对象
            if (other.TryGetComponent(out MapObjectBase mapObject))
            {
                // 已经攻击过的防止二次伤害
                if (m_LastAttackedMapObjectList.Contains(mapObject)) return;
                m_LastAttackedMapObjectList.Add(mapObject);
                Item_WeaponInfo itemWeaponInfo = (Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo;

                // 检测对方是什么类型 以及 自己手上是什么武器
                switch (mapObject.MapObjectType)
                {
                    case MapObjectType.Tree:

                        // 当前是不是斧头
                        if (itemWeaponInfo.WeaponType == WeaponType.Axe)
                        {
                            // 让树受伤
                            ((TreeController)mapObject).Hurt(itemWeaponInfo.AttackValue);
                            m_AttackSucceedCount += 1;
                        }
                        break;
                    case MapObjectType.Stone: break;
                    case MapObjectType.SmallStone: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void ChangeState(PlayerState playerState)
        {
            switch (playerState)
            {
                case PlayerState.Idle:
                    m_StateMachine.ChangeState<PlayerIdle>((int)playerState);
                    break;
                case PlayerState.Move:
                    m_StateMachine.ChangeState<PlayerMove>((int)playerState);
                    break;
                case PlayerState.Attack:
                    m_StateMachine.ChangeState<PlayerAttack>((int)playerState);
                    break;
                case PlayerState.BeAttack: break;
                case PlayerState.Dead: break;
            }
        }

        #endregion

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

        #region 武器

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

        #region 战斗、伐木、采摘

        public void OnSelectMapObject(RaycastHit hitInfo)
        {
            if (hitInfo.collider.TryGetComponent(out MapObjectBase mapObject))
            {
                // 根据玩家选中的地图对象类型以及当前角色的武器来判断做什么
                Debug.Log("选中的是：" + mapObject.gameObject.name);
                float distance = Vector3.Distance(PlayerTransform.position, mapObject.transform.position);
                switch (mapObject.MapObjectType)
                {
                    case MapObjectType.Tree:
                        if (distance < 2)
                        {
                            FellingTree(mapObject);
                        }
                        break;
                    case MapObjectType.Stone: break;
                    case MapObjectType.SmallStone: break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        bool m_CanAttack = true;
        public Quaternion AttackDirection { get; private set; }

        void FellingTree(MapObjectBase mapObject)
        {
            // 能攻击 是斧头
            if (m_CanAttack
             && m_CurrentWeaponItemData != null
             && ((Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo).WeaponType == WeaponType.Axe)
            {
                m_CanAttack = false; // 防止攻击过程中再次攻击

                // 计算方向
                AttackDirection = Quaternion.LookRotation(mapObject.transform.position - transform.position);

                // 切换状态
                ChangeState(PlayerState.Attack);

                // 禁止使用物品
                CanUseItem = false;
            }
        }

        #endregion

        #region 存档

        PlayerTransformData m_PlayerTransformData;
        PlayerCoreData m_PlayerCoreData;

        #endregion
    }
}