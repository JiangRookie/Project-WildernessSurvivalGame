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

        #region 音效资源

        [SerializeField] AudioClip[] m_FootStepAudioClips;

        #endregion

        [HideInInspector] public Vector2 PositionXScope;
        [HideInInspector] public Vector2 PositionZScope;
        public Transform PlayerTransform { get; private set; }
        public float MoveSpeed { get; private set; } = 4;
        public float RotateSpeed { get; private set; } = 10;

        #region 存档

        PlayerTransformData m_PlayerTransformData;

        #endregion

        StateMachine m_StateMachine;

        public void Init(float mapSizeOnWorld)
        {
            m_PlayerTransformData = ArchiveManager.Instance.PlayerTransformData;

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
            AudioManager.Instance.PlayOnShot(m_FootStepAudioClips[index], PlayerTransform.position, 0.5f);
        }

        void OnDestroy()
        {
            // 把存档数据实际写入磁盘
            m_PlayerTransformData.Position = PlayerTransform.localPosition;
            m_PlayerTransformData.Rotation = PlayerTransform.localRotation.eulerAngles;
            ArchiveManager.Instance.SavePlayerTransformData();
        }
    }
}