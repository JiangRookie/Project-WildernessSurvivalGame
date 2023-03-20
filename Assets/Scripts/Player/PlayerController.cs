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
        [SerializeField] PlayerModel PlayerModel;
        [SerializeField] AudioClip[] FootStepAudioClips;
        [HideInInspector] public Vector2 PositionXScope;
        [HideInInspector] public Vector2 PositionZScope;
        public Transform PlayerTransform { get; private set; }
        public float MoveSpeed { get; private set; } = 10;
        public float RotateSpeed { get; private set; } = 10;

        StateMachine m_StateMachine;

        void Start()
        {
            Init();
        }

        public void Init()
        {
            this.PlayerModel.Init(PlayAudioOnFootStep);
            PlayerTransform = transform;

            m_StateMachine = ResManager.Load<StateMachine>();
            m_StateMachine.Init(this);

            // 设置初始状态为待机状态
            m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle);
            InitPositionScope(MapManager.Instance.MapSizeOnWorld);
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
            AudioManager.Instance.PlayOnShot(FootStepAudioClips[index], PlayerTransform.position, 0.5f);
        }
    }
}