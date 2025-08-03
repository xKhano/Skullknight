using Skullknight.Player.Statemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Statemachine
{
    public class PlayerRunningState : PlayerState
    {
        public PlayerRunningState(PlayerController stateManager) : base(stateManager)
        {
            this.controller = stateManager;
        }

        public override void EnterState()
        {
            controller.Animator.Play("Run");
            controller.ActiveBoxCollider2D.sharedMaterial = controller.normalPhysicMaterial;
            controller.MoveOnGround(controller.playerInput.actions["Horizontal"].ReadValue<float>());
        }
        public override void ExitState()
        {
            
        }

        private void OnHorizontalCanceled(InputAction.CallbackContext context)
        {
            controller.ChangeState(EPlayerState.Idle);
        }

        public override void StateUpdate()
        {
            float velocityProgress = Mathf.Abs(controller.rb.linearVelocity.x) / controller.maxRunningVelocity;
        
            controller.RegenerateStamina();
            
            if (controller.playerInput.actions["Crouch"].IsPressed())
            {
                if(Mathf.Abs(controller.rb.linearVelocity.x) > controller.SlidingSpeed) controller.ChangeState(EPlayerState.Sliding);
                else
                {
                    controller.ChangeState(EPlayerState.Crouching);
                }
            }
            else if (controller.playerInput.actions["Roll"].IsPressed())
            {
                controller.ChangeState(EPlayerState.Roll);
            }
            else if (!controller.playerInput.actions["Horizontal"].IsPressed())
            {
                controller.ChangeState(EPlayerState.Idle);
            }
            else
            {
                //if(!controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Run")) controller.animator.SetTrigger("run");
                controller.Animator.SetFloat("runAnimSpeed", velocityProgress * controller.maxRunningAnimSpeed);
            }
        
        }

        public override void StateFixedUpdate()
        {
            
            float horizontal = controller.playerInput.actions["Horizontal"].ReadValue<float>();
            if (controller.groundCollisionChecker.IsColliding)
            {
                if (controller.playerInput.actions["Horizontal"].IsPressed())
                {
                    controller.MoveOnGround(horizontal);
                }
            }
            else
            {
                controller.ChangeState(EPlayerState.Falling);
            }
        }
        

        public override void SubscribeEvents()
        {
            controller.playerInput.actions["Horizontal"].canceled += OnHorizontalCanceled;
            controller.playerInput.actions["Jump"].performed += controller.OnJumpPerformed;
            controller.playerInput.actions["Attack"].performed += OnAttackPerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext obj)
        {
            controller.ChangeState(EPlayerState.AttackOne);
        }

        public override void UnsubscribeEvents()
        {
            controller.playerInput.actions["Horizontal"].canceled -= OnHorizontalCanceled;   
            controller.playerInput.actions["Jump"].performed -= controller.OnJumpPerformed;
            controller.playerInput.actions["Attack"].performed -= OnAttackPerformed;
        }
    }
}