using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public enum PlayerState { Idle, Move, Attack, BeAttack, Dead }

    public class PlayerController : SingletonMono<PlayerController>, IStateMachineOwner
    {
        #region FIELD

        public CharacterController CharacterController;
        [SerializeField] Animator m_Animator;
        [SerializeField] PlayerModel m_PlayerModel;
        [HideInInspector] public Vector2 PositionXScope;
        [HideInInspector] public Vector2 PositionZScope;

        StateMachine m_StateMachine;

        public Transform PlayerTransform { get; private set; }
        public bool CanUseItem { get; private set; } = true;
        public float MoveSpeed => m_PlayerConfig.MoveSpeed;
        public float RotateSpeed => m_PlayerConfig.RotateSpeed;

        #region 配置

        PlayerConfig m_PlayerConfig;

        #endregion

        #region 存档

        PlayerTransformData m_PlayerTransformData;
        PlayerCoreData m_PlayerCoreData;

        #endregion

        #endregion

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

        int m_AttackSucceedCount; // 攻击成功的数量
        List<MapObjectBase> m_LastAttackedMapObjectList = new List<MapObjectBase>();

        void PlayAudioOnFootStep(int index)
        {
            AudioManager.Instance.PlayOneShot(m_PlayerConfig.FootStepAudioClips[index], PlayerTransform.position, m_PlayerConfig.FootStepVolume);
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
            // 判断对方是不是可击打的地图对象
            if (other.TryGetComponent(out CanHitMapObjectBase canHitMapObject))
            {
                // 防止已经攻击过的地图对象受到二次伤害
                if (m_LastAttackedMapObjectList.Contains(canHitMapObject)) return;
                m_LastAttackedMapObjectList.Add(canHitMapObject);

                // 检测对方是什么类型 以及 自己手上是什么武器
                switch (canHitMapObject.MapObjectType)
                {
                    case MapObjectType.Tree:
                        CheckMapObjectHurt(canHitMapObject, WeaponType.Axe);
                        break;
                    case MapObjectType.Stone:
                        CheckMapObjectHurt(canHitMapObject, WeaponType.PickAxe);
                        break;
                    case MapObjectType.Bush:
                        CheckMapObjectHurt(canHitMapObject, WeaponType.Sickle);
                        break;
                }
            }
            else if (other.TryGetComponent(out AIBase aiObject))
            {
                Item_WeaponInfo weaponInfo = (Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo;
                AudioManager.Instance.PlayOneShot(weaponInfo.HitAudio, transform.position);
                aiObject.Hurt(weaponInfo.AttackValue);
                m_AttackSucceedCount += 1;
            }
        }

        void CheckMapObjectHurt(CanHitMapObjectBase canHitMapObject, WeaponType weaponType)
        {
            Item_WeaponInfo weaponInfo = (Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo;

            if (weaponInfo.WeaponType == weaponType)
            {
                AudioManager.Instance.PlayOneShot(weaponInfo.HitAudio, transform.position);
                canHitMapObject.Hurt(weaponInfo.AttackValue);
                m_AttackSucceedCount += 1;
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

        public void PlayerAnimation(string animationName, float fixedTransitionDuration = 0.25f)
        {
            m_Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
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
                if (info != null)
                {
                    m_CurrentWeaponGameObject = PoolManager.Instance.GetGameObject(info.PrefabOnPlayer, m_PlayerModel.WeaponRoot);
                    m_CurrentWeaponGameObject.transform.localPosition = info.PositionOnPlayer;
                    m_CurrentWeaponGameObject.transform.localRotation = Quaternion.Euler(info.RotationOnPlayer);
                    m_Animator.runtimeAnimatorController = info.AnimatorOverrideController;
                }
            }
            else // 新武器是 null，意味着空手
            {
                m_Animator.runtimeAnimatorController = m_PlayerConfig.NormalAnimatorController;
            }

            // 由于动画是有限状态机驱动的，如果不重新激活一次动画，动画会出现错误
            // （比如在移动中突然切换AnimatorOverrideController会不播放走路动画）
            m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle, true);
            m_CurrentWeaponItemData = newWeapon;
        }

        #endregion

        #region 战斗、伐木、采摘

        bool m_CanAttack = true;
        public Quaternion AttackDirection { get; private set; }
        MapObjectBase m_LastHitMapObject; // 最后被攻击的地图对象

        public void OnSelectMapObject(RaycastHit hitInfo, bool isMouseButtonDown)
        {
            #region MapObject

            if (hitInfo.collider.TryGetComponent(out MapObjectBase mapObject))
            {
                float distance = Vector3.Distance(PlayerTransform.position, mapObject.transform.position);

                // 不在交互范围内
                if (distance > mapObject.TouchDistance)
                {
                    if (isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("请离近一点哦！");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    return;
                }

                // 判断拾取
                if (mapObject.CanPickUp)
                {
                    if (isMouseButtonDown == false) return;
                    m_LastHitMapObject = null;

                    // 获取捡到的物品
                    int itemConfigID = mapObject.CanPickUpItemConfigID;
                    if (itemConfigID != -1)
                    {
                        // 背包里面如果数据添加成功 则销毁地图物体
                        if (InventoryManager.Instance.AddItemToMainInventoryWindow(itemConfigID))
                        {
                            mapObject.OnPickUp();

                            // 播放拾取物品动画 这里没有切换状态，依然是Idle状态
                            PlayerAnimation("PickUp");
                            ProjectTool.PlayAudio(AudioType.Bag);
                        }
                        else
                        {
                            // if (isMouseButtonDown) // Expression is always true
                            // {
                            //     UIManager.Instance.AddTips("背包已经满了！");
                            //     ProjectTool.PlayAudio(AudioType.Fail);
                            // }
                            UIManager.Instance.AddTips("背包已经满了！");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                    }
                    return;
                }

                // 判断攻击
                if (m_CanAttack == false) { return; }

                // 现在交互的对象为一个新对象，且鼠标从上一个对象到现在这个新对象一直保持按着鼠标的状态则退出
                // 转换对象之后必须重新按下数鼠标左键才允许交互
                if (m_LastHitMapObject != mapObject && isMouseButtonDown == false) return;

                // 根据玩家选中的地图对象类型以及当前角色的武器来判断做什么
                switch (mapObject.MapObjectType)
                {
                    case MapObjectType.Tree:
                        if (CheckHitMapObject(mapObject, WeaponType.Axe) == false && isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("你需要装备斧头！");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                        break;
                    case MapObjectType.Stone:
                        if (CheckHitMapObject(mapObject, WeaponType.PickAxe) == false && isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("你需要装备镐！");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                        break;
                    case MapObjectType.Bush:
                        if (CheckHitMapObject(mapObject, WeaponType.Sickle) == false && isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("你需要装备镰刀！");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                        break;
                }
                return;
            }

            #endregion

            #region AI

            if (m_CanAttack && m_CurrentWeaponItemData != null && hitInfo.collider.TryGetComponent(out AIBase aiObject))
            {
                float distance = Vector3.Distance(PlayerTransform.position, aiObject.transform.position);
                Item_WeaponInfo weaponInfo = (Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo;

                // 交互距离：武器的长度 + AI的半径
                if (distance < weaponInfo.AttackDistance + aiObject.Radius)
                {
                    m_CanAttack = false; // 防止攻击过程中再次攻击
                    var position = transform.position;
                    AttackDirection = Quaternion.LookRotation(aiObject.transform.position - position); // 计算攻击方向
                    AudioManager.Instance.PlayOneShot(weaponInfo.AttackAudio, position);
                    ChangeState(PlayerState.Attack); // 切换状态
                    CanUseItem = false;              // 禁止使用物品
                }
            }

            #endregion
        }

        bool CheckHitMapObject(MapObjectBase mapObject, WeaponType weaponType)
        {
            if (m_CurrentWeaponItemData == null) return false;
            Item_WeaponInfo weaponInfo = (Item_WeaponInfo)m_CurrentWeaponItemData.Config.ItemTypeInfo;

            // 能攻击，有武器，武器类型符合要求
            if (weaponInfo.WeaponType == weaponType)
            {
                m_CanAttack = false; // 防止攻击过程中再次攻击
                var position = transform.position;
                AttackDirection = Quaternion.LookRotation(mapObject.transform.position - position); // 计算攻击方向
                AudioManager.Instance.PlayOneShot(weaponInfo.AttackAudio, position);
                ChangeState(PlayerState.Attack); // 切换状态
                CanUseItem = false;              // 禁止使用物品
                return true;
            }
            return false;
        }

        #endregion

        public void Hurt(float damage)
        {
            Debug.Log("玩家受伤：" + damage);
        }
    }
}