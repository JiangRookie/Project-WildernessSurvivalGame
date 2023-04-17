using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class PlayerMove : PlayerStateBase
    {
        CharacterController m_CharacterController;

        public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
        {
            base.Init(owner, stateType, stateMachine);
            m_CharacterController = m_PlayerCtrl.CharacterController;
        }

        public override void Enter()
        {
            m_PlayerCtrl.PlayerAnimation("Move");
        }

        public override void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            if (horizontal == 0 && vertical == 0)
            {
                m_PlayerCtrl.ChangeState(PlayerState.Idle);
                return;
            }

            var inputDirection = new Vector3(horizontal, 0, vertical);

            // 朝向计算
            var targetQuaternion = Quaternion.LookRotation(inputDirection);
            m_PlayerCtrl.PlayerTransform.localRotation = Quaternion.Slerp(m_PlayerCtrl.PlayerTransform.localRotation
                                                                      , targetQuaternion
                                                                      , Time.deltaTime * m_PlayerCtrl.RotateSpeed);

            // 检查地图边界
            if ((m_PlayerCtrl.PlayerTransform.position.x < m_PlayerCtrl.PositionXScope.x && horizontal < 0)
             || (m_PlayerCtrl.PlayerTransform.position.x > m_PlayerCtrl.PositionXScope.y && horizontal > 0))
                inputDirection.x = 0;
            if ((m_PlayerCtrl.PlayerTransform.position.z < m_PlayerCtrl.PositionZScope.x && vertical < 0)
             || (m_PlayerCtrl.PlayerTransform.position.z > m_PlayerCtrl.PositionZScope.y && vertical > 0))
                inputDirection.z = 0;

            m_CharacterController.Move(Time.deltaTime * m_PlayerCtrl.MoveSpeed * inputDirection);
        }
    }
}