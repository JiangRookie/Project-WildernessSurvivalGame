using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public enum PlayerState { Idle, Move, Attack, Hurt, Dead }

public class PlayerController : SingletonMono<PlayerController>, IStateMachineOwner
{
    #region FIELD

    public CharacterController CharacterController;
    [SerializeField] Animator m_Animator;
    [SerializeField] Collider m_Collider;
    [SerializeField] PlayerModel m_PlayerModel;

    StateMachine m_StateMachine;

    public Collider Collider => m_Collider;
    public Transform PlayerTransform { get; private set; }
    public float MoveSpeed => m_PlayerConfig.MoveSpeed;
    public float RotateSpeed => m_PlayerConfig.RotateSpeed;
    public Vector2 PositionXScope { get; private set; }
    public Vector2 PositionZScope { get; private set; }
    public bool CanUseItem { get; private set; } = true;
    bool CanPickUpItem { get; set; } = true;

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
        CalculateHungry();
    }

    void OnDestroy()
    {
        if (m_StateMachine == null) return;
        m_StateMachine.Destroy();
        m_StateMachine = null;
    }

    public void Init(float mapSizeOnWorld)
    {
        m_PlayerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
        m_PlayerTransformData = ArchiveManager.Instance.PlayerTransformData;
        m_PlayerCoreData = ArchiveManager.Instance.PlayerCoreData;

        m_PlayerModel.Init(PlayAudioOnFootStep, OnStartHit, OnStopHit, OnAttackOver, OnHurtOver, OnDeadOver);
        PlayerTransform = transform;

        m_StateMachine = ResManager.Load<StateMachine>();
        m_StateMachine.Init(this);

        // 设置初始状态为待机状态
        m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle);

        // 初始化坐标范围
        PositionXScope = new Vector2(1, mapSizeOnWorld - 1);
        PositionZScope = new Vector2(1, mapSizeOnWorld - 1);

        // 初始化存档相关的数据
        PlayerTransform.localPosition = m_PlayerTransformData.Position;
        PlayerTransform.localRotation = Quaternion.Euler(m_PlayerTransformData.Rotation);

        TriggerUpdateHpEvent();
        TriggerUpdateHungryEvent();

        EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
    }

    #region 工具

    void PlayAudioOnFootStep(int index) =>
        AudioManager.Instance.PlayOneShot(m_PlayerConfig.FootStepAudioClips[index], PlayerTransform.position, m_PlayerConfig.FootStepVolume);

    public void ChangeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                m_CanAttack = true;
                CanUseItem = true;
                CanPickUpItem = true;
                m_StateMachine.ChangeState<PlayerIdle>((int)playerState);
                break;
            case PlayerState.Move:
                m_CanAttack = true;
                CanUseItem = true;
                CanPickUpItem = false;
                m_StateMachine.ChangeState<PlayerMove>((int)playerState);
                break;
            case PlayerState.Attack:
                m_CanAttack = false;
                CanUseItem = false;
                CanPickUpItem = false;
                m_StateMachine.ChangeState<PlayerAttack>((int)playerState);
                break;
            case PlayerState.Hurt:
                m_CanAttack = false;
                CanUseItem = false;
                CanPickUpItem = false;
                m_StateMachine.ChangeState<PlayerHurt>((int)playerState);
                break;
            case PlayerState.Dead:
                m_CanAttack = false;
                CanUseItem = false;
                CanPickUpItem = false;
                m_StateMachine.ChangeState<PlayerDead>((int)playerState);
                break;
        }
    }

    public void PlayerAnimation(string animationName, float fixedTransitionDuration = 0.25f) =>
        m_Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);

    #endregion

    #region 生命值、饥饿值

    void CalculateHungry()
    {
        if (m_PlayerCoreData.Hungry > 0)
        {
            m_PlayerCoreData.Hungry -= Time.deltaTime * m_PlayerConfig.HungryReducingSpeed;
            if (m_PlayerCoreData.Hungry <= 0) m_PlayerCoreData.Hungry = 0;
            TriggerUpdateHungryEvent();
        }
        else
        {
            if (m_PlayerCoreData.Hp <= 0) return;
            m_PlayerCoreData.Hp -= Time.deltaTime * m_PlayerConfig.HpReducingSpeedOnHungryIsZero;
            if (m_PlayerCoreData.Hp <= 0)
            {
                m_PlayerCoreData.Hp = 0;
                ChangeState(PlayerState.Dead);
            }
            TriggerUpdateHpEvent();
        }
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

    void TriggerUpdateHpEvent() => EventManager.EventTrigger(EventName.UpdatePlayerHp, m_PlayerCoreData.Hp);

    void TriggerUpdateHungryEvent() => EventManager.EventTrigger(EventName.UpdatePlayerHungry, m_PlayerCoreData.Hungry);

    #endregion

    #region 武器

    ItemData m_CurrWeaponData;
    GameObject m_CurrWeaponGameObj;

    /// <summary>
    /// 更换武器
    /// </summary>
    public void ChangeWeapon(ItemData newWeaponData)
    {
        // 如果新旧武器相同，直接返回
        if (m_CurrWeaponData == newWeaponData)
        {
            m_CurrWeaponData = newWeaponData;
            return;
        }

        // 如果有旧武器，回收到对象池
        if (m_CurrWeaponData != null)
        {
            m_CurrWeaponGameObj.PushGameObj2Pool();
        }

        // 获取新武器信息
        Item_WeaponInfo newWeaponInfo = null;
        if (newWeaponData != null)
        {
            newWeaponInfo = newWeaponData.Config.ItemTypeInfo as Item_WeaponInfo;
        }

        // 生成或回收武器对象
        if (newWeaponInfo != null)
        {
            m_CurrWeaponGameObj = PoolManager.Instance.GetGameObject(newWeaponInfo.PrefabOnPlayer, m_PlayerModel.WeaponRoot);
            m_CurrWeaponGameObj.transform.localPosition = newWeaponInfo.PositionOnPlayer;
            m_CurrWeaponGameObj.transform.localRotation = Quaternion.Euler(newWeaponInfo.RotationOnPlayer);
            m_Animator.runtimeAnimatorController = newWeaponInfo.AnimatorOverrideController;
        }
        else // 新武器是 null，意味着空手
        {
            m_Animator.runtimeAnimatorController = m_PlayerConfig.NormalAnimatorController;
        }

        // 由于动画是有限状态机驱动的，如果不重新激活一次动画，动画会出现错误
        // （比如在移动中突然切换AnimatorOverrideController会不播放走路动画）
        m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle, true);

        // 记录当前武器数据
        m_CurrWeaponData = newWeaponData;
    }

    #endregion

    #region 战斗、伐木、采摘

    bool m_CanAttack = true;
    public Quaternion AttackDir { get; private set; }
    List<MapObjectBase> m_LastAttackedMapObjectList = new List<MapObjectBase>();
    MapObjectBase m_LastHitMapObject; // 最后被攻击的地图对象
    int m_AttackSucceedCount;         // 攻击成功的数量

    /// <summary>
    /// 当选择地图对象或者AI时
    /// </summary>
    public void OnSelectMapObjectOrAI(RaycastHit hitInfo, bool isMouseButtonDown)
    {
        #region MapObject

        if (hitInfo.collider.TryGetComponent(out MapObjectBase mapObject))
        {
            float distance = Vector3.Distance(PlayerTransform.position, mapObject.transform.position);

            // 判断是否在交互范围内
            if (distance > mapObject.InteractiveDistance)
            {
                if (isMouseButtonDown == false) return;
                UIManager.Instance.AddTips("请离近一点哦！");
                ProjectTool.PlayAudio(AudioType.Fail);
                return;
            }

            // 判断拾取
            if (mapObject.CanPickUp)
            {
                if (CanPickUpItem == false) return;
                if (isMouseButtonDown == false) return;
                m_LastHitMapObject = null;

                // 获取捡到的物品
                int itemConfigID = mapObject.CanPickUpItemConfigID;
                if (itemConfigID == -1) return;

                // 背包里面如果数据添加成功 则销毁地图物体
                if (InventoryManager.Instance.AddItemToMainInventory(itemConfigID))
                {
                    mapObject.OnPickUp();
                    transform.LookAt(mapObject.transform.position);

                    PlayerAnimation("PickUp");
                    ProjectTool.PlayAudio(AudioType.Bag);
                }
                else
                {
                    UIManager.Instance.AddTips("背包已经满了！");
                    ProjectTool.PlayAudio(AudioType.Fail);
                }
                return;
            }

            // 判断攻击
            if (m_CanAttack == false) { return; }

            // 现在交互的对象为一个新对象，且鼠标从上一个对象到现在这个新对象一直保持按着鼠标的状态则退出
            // 转换对象之后必须重新按下数鼠标左键才允许交互
            if (m_LastHitMapObject != mapObject && isMouseButtonDown == false) return;
            m_LastHitMapObject = mapObject;

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

        if (m_CanAttack && m_CurrWeaponData != null && hitInfo.collider.TryGetComponent(out AIBase ai))
        {
            float distance = Vector3.Distance(PlayerTransform.position, ai.transform.position);
            if (m_CurrWeaponData.Config.ItemTypeInfo is not Item_WeaponInfo weaponInfo) return;

            // 交互距离：武器的长度 + AI的半径
            if (distance > weaponInfo.AttackDistance + ai.Radius) return; // 超过攻击距离无法攻击
            var position = transform.position;
            AttackDir = Quaternion.LookRotation(ai.transform.position - position); // 计算攻击方向
            AudioManager.Instance.PlayOneShot(weaponInfo.AttackAudio, position);   // 播放攻击音效
            ChangeState(PlayerState.Attack);                                       // 切换攻击状态
        }

        #endregion
    }

    bool CheckHitMapObject(MapObjectBase mapObject, WeaponType weaponType)
    {
        if (m_CurrWeaponData == null) return false;

        // 能攻击，有武器，武器类型符合要求
        if (m_CurrWeaponData.Config.ItemTypeInfo is not Item_WeaponInfo weaponInfo || weaponInfo.WeaponType != weaponType) return false;
        var position = transform.position;
        AttackDir = Quaternion.LookRotation(mapObject.transform.position - position); // 计算攻击方向
        AudioManager.Instance.PlayOneShot(weaponInfo.AttackAudio, position);          // 播放攻击音效
        ChangeState(PlayerState.Attack);                                              // 切换攻击状态

        return true;
    }

    /// <summary>
    /// 开启伤害检测
    /// </summary>
    void OnStartHit()
    {
        m_AttackSucceedCount = 0;
        m_CurrWeaponGameObj.transform.OnTriggerEnter(OnWeaponTriggerEnter);
    }

    // 让武器停止伤害检测
    public void OnStopHit()
    {
        m_CurrWeaponGameObj.transform.RemoveTriggerEnter(OnWeaponTriggerEnter);
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
        ChangeState(PlayerState.Idle);
    }

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
                    CanHitMapObjectHurt(canHitMapObject, WeaponType.Axe);
                    break;
                case MapObjectType.Stone:
                    CanHitMapObjectHurt(canHitMapObject, WeaponType.PickAxe);
                    break;
                case MapObjectType.Bush:
                    CanHitMapObjectHurt(canHitMapObject, WeaponType.Sickle);
                    break;
            }
        }
        else if (other.TryGetComponent(out AIBase ai))
        {
            if (m_CurrWeaponData.Config.ItemTypeInfo is not Item_WeaponInfo weaponInfo) return;
            AudioManager.Instance.PlayOneShot(weaponInfo.HitAudio, transform.position);
            GameObject hitEffect = PoolManager.Instance.GetGameObject(weaponInfo.HitEffect);
            hitEffect.transform.position = other.ClosestPoint(m_CurrWeaponGameObj.transform.position);
            ai.Hurt(weaponInfo.AttackValue);
            m_AttackSucceedCount += 1;
        }
    }

    void OnHurtOver() => ChangeState(PlayerState.Idle);

    void OnDeadOver() => GameSceneManager.Instance.GameOver();

    /// <summary>
    /// 匹配当前武器的武器类型与传入的<paramref name="weaponType"/>是否一致<br/>
    /// 如果一致则执行可击打的地图对象的受伤方法
    /// </summary>
    /// <param name="canHitMapObject">可击打的地图对象</param>
    /// <param name="weaponType">武器类型</param>
    void CanHitMapObjectHurt(CanHitMapObjectBase canHitMapObject, WeaponType weaponType)
    {
        // 类型匹配
        if (m_CurrWeaponData.Config.ItemTypeInfo is not Item_WeaponInfo weaponInfo || weaponInfo.WeaponType != weaponType) return;
        AudioManager.Instance.PlayOneShot(weaponInfo.HitAudio, transform.position);
        canHitMapObject.Hurt(weaponInfo.AttackValue);
        m_AttackSucceedCount += 1;
    }

    public void Hurt(float damage)
    {
        if (m_PlayerCoreData.Hp <= 0) return;
        m_PlayerCoreData.Hp -= damage;
        if (m_PlayerCoreData.Hp <= 0)
        {
            m_PlayerCoreData.Hp = 0;
            TriggerUpdateHpEvent();
            ChangeState(PlayerState.Dead);
        }
        else
        {
            TriggerUpdateHpEvent();
            ChangeState(PlayerState.Hurt);
        }
    }

    #endregion

    void OnGameSave()
    {
        m_PlayerTransformData.Position = PlayerTransform.localPosition;
        m_PlayerTransformData.Rotation = PlayerTransform.localRotation.eulerAngles;
        ArchiveManager.Instance.SavePlayerTransformData();
        ArchiveManager.Instance.SavePlayerCoreData();
    }
}