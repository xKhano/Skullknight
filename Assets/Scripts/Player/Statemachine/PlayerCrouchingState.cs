using System;
using Skullknight.Player.Statemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Statemachine
{
    public class PlayerCrouchingState : PlayerState
    {

        public PlayerCrouchingState(PlayerController stateManager) : base(stateManager){}

        public override void ExitState()
        {
            
        }

        public override void EnterState()
        {
            controller.ActiveBoxCollider2D = controller.crouchCollider;
            controller.Animator.Play("Crouch");
        }

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            if (!controller.standUpCollisionChecker.IsColliding)
            {
                controller.ChangeState(EPlayerState.Idle);  
            }
        }


        public override void StateUpdate()
        {
            controller.RegenerateStamina();

            
            float horizontal = controller.playerInput.actions["Horizontal"].ReadValue<float>();
            
            if (horizontal != 0)
            {
                if(!controller.Animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchWalk")) controller.Animator.Play("CrouchWalk");
                float crouchWalkProgress = Mathf.Abs(controller.rb.linearVelocity.x) / controller.maxCrouchingVelocity;
                controller.Animator.SetFloat("crouchWalkSpeed",crouchWalkProgress);
            }
            else
            {
                if(!controller.Animator.GetCurrentAnimatorStateInfo(0).IsName("Crouch")) controller.Animator.Play("Crouch");
            }
        }

        public override void StateFixedUpdate()
        {
            float horizontal = controller.playerInput.actions["Horizontal"].ReadValue<float>();
            float crouch = controller.playerInput.actions["Crouch"].ReadValue<float>();
            if (controller.groundCollisionChecker.IsColliding)
            {
                if(horizontal != 0) controller.Crouchwalk(horizontal);
                if (crouch == 0 && !controller.standUpCollisionChecker.IsColliding) controller.ChangeState(EPlayerState.Idle);
            }
            else
            {
                //to falling
            }
        }

        public override void SubscribeEvents()
        {
            controller.playerInput.actions["Crouch"].canceled += OnCrouchCanceled;
            controller.playerInput.actions["Attack"].performed += OnAttackPerformed;
        }
        public override void UnsubscribeEvents()
        {
            controller.playerInput.actions["Crouch"].canceled -= OnCrouchCanceled;
            controller.playerInput.actions["Attack"].performed -= OnAttackPerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext obj)
        {
            controller.ChangeState(EPlayerState.CrouchAttack);
        }

    }
}
