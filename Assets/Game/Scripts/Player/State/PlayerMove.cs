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
            m_CharacterController = PlayerCtrl.CharacterController;
        }

        public override void Enter()
        {
            PlayerAnimation("Move");
        }

        public override void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            if (horizontal == 0 && vertical == 0)
            {
                ChangeState(PlayerState.Idle);
                return;
            }

            var inputDirection = new Vector3(horizontal, 0, vertical);

            // 朝向计算
            var targetQuaternion = Quaternion.LookRotation(inputDirection);
            PlayerCtrl.PlayerTransform.localRotation = Quaternion.Slerp(PlayerCtrl.PlayerTransform.localRotation
                                                                      , targetQuaternion
                                                                      , Time.deltaTime * PlayerCtrl.RotateSpeed);

            // 检查地图边界
            if ((PlayerCtrl.PlayerTransform.position.x < PlayerCtrl.PositionXScope.x && horizontal < 0)
             || (PlayerCtrl.PlayerTransform.position.x > PlayerCtrl.PositionXScope.y && horizontal > 0))
                inputDirection.x = 0;
            if ((PlayerCtrl.PlayerTransform.position.z < PlayerCtrl.PositionZScope.x && vertical < 0)
             || (PlayerCtrl.PlayerTransform.position.z > PlayerCtrl.PositionZScope.y && vertical > 0))
                inputDirection.z = 0;

            m_CharacterController.Move(Time.deltaTime * PlayerCtrl.MoveSpeed * inputDirection);
        }
    }
}