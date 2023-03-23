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
            AudioManager.Instance.PlayOnShot(m_PlayerConfig.FootStepAudioClips[index], PlayerTransform.position
                                           , m_PlayerConfig.FootStepVolume);
        }

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

        #region 存档

        PlayerTransformData m_PlayerTransformData;
        PlayerCoreData m_PlayerCoreData;

        #endregion
    }
}