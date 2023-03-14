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
        public Transform PlayerTransform { get; private set; }
        StateMachine m_StateMachine;

        // transform.Translate(Time.deltaTime * 3 * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));

        public void Init()
        {
            PlayerTransform = transform;
            
            m_StateMachine = ResManager.Load<StateMachine>();
            m_StateMachine.Init(this);

            // 设置初始状态为待机状态
            m_StateMachine.ChangeState<PlayerIdle>((int)PlayerState.Idle);
        }
    }
}