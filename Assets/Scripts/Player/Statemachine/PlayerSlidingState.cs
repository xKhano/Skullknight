using Skullknight.Player.Statemachine;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Player.Statemachine
{
    public class PlayerSlidingState : PlayerState
    {
        public PlayerSlidingState(PlayerController stateManager) : base(stateManager)
        {
            this.controller = stateManager;
        }

        public override void ExitState()
        {
            controller.crouchCollider.sharedMaterial = controller.normalPhysicMaterial;
        }

        public override void EnterState()
        {
            controller.ActiveBoxCollider2D = controller.crouchCollider;
            controller.crouchCollider.sharedMaterial = controller.slidingPhysicMaterial;
            controller.Animator.Play("Slide");
            controller.Groundslide();
            controller.OnSlide?.Invoke();
        }

        public override void StateUpdate()
        {
            if (!controller.playerInput.actions["Crouch"].IsPressed())
            {
                controller.rb.linearVelocity = Vector2.zero; 
                if (controller.standUpCollisionChecker.IsColliding)
                {
                    controller.ChangeState(EPlayerState.Crouching);
                }
                else
                {
                    controller.ChangeState(EPlayerState.Idle);
                }
            }
        }

        public override void StateFixedUpdate()
        {
            if (!controller.groundCollisionChecker.IsColliding)
            {
                controller.ChangeState(EPlayerState.Falling);
            }
            else if (Mathf.Abs(controller.rb.linearVelocity.x) < controller.SlideStoppingSpeed)
            {
                if(controller.playerInput.actions["Crouch"].IsPressed()) controller.ChangeState(EPlayerState.Crouching);
                else if (!controller.standUpCollisionChecker.IsColliding)
                {
                    controller.ChangeState(EPlayerState.Idle);
                }
                else
                {
                    controller.ChangeState(EPlayerState.Crouching);
                }
            }
        }

        public override void SubscribeEvents()
        {
            
        }

        public override void UnsubscribeEvents()
        {
            
        }
    }
}
