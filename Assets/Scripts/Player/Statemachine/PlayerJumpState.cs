using Skullknight.Player.Statemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Statemachine
{
    public class PlayerJumpState : PlayerState
    {
        public PlayerJumpState(PlayerController stateManager) : base(stateManager){}
        public override void EnterState()
        {
            controller.Animator.Play("Jump");
            controller.Jump();
        }
        public override void ExitState()
        {
            
        }

        public override void StateUpdate()
        {
            float jump = controller.playerInput.actions["Jump"].ReadValue<float>();
            if (jump != 1)
            {
                controller.JumpCut();
                //controller.PlayerState = controller.FallingState;
            }
        }

        public override void StateFixedUpdate()
        {
            float horizontal = controller.playerInput.actions["Horizontal"].ReadValue<float>();
            if (controller.CanJump && controller.groundCollisionChecker.IsColliding)
            {
                controller.ChangeState(EPlayerState.Idle);
            }
            else if (controller.rb.linearVelocity.y < 0)
            {
                controller.ChangeState(EPlayerState.Falling);
            }
            else if (horizontal != 0)
            {
                if(controller.wallSlideCheckDeathCoroutine == null)
                {
                    RaycastHit2D wallcheck = Physics2D.BoxCast(
                        controller.wallCheckCollider.bounds.center,
                        controller.wallCheckCollider.bounds.size,
                        0f,
                        Vector2.zero,
                        0f,
                        controller.wallMask);
                    if (wallcheck.collider != null)
                    {
                        if (Mathf.Sign(wallcheck.point.x - controller.rb.position.x) == Mathf.Sign(horizontal))
                        {
                            controller.SetFlip(!controller.SpriteRenderer.flipX);
                            controller.ChangeState(EPlayerState.Wallsliding);
                            return;
                        }
                    }
                }
                controller.Airstrafe(horizontal);
            }
        }

        public override void SubscribeEvents()
        {
            controller.playerInput.actions["Hold"].performed += controller.OnHoldPerformed;
        }

        public override void UnsubscribeEvents()
        {
            controller.playerInput.actions["Hold"].performed -= controller.OnHoldPerformed;
        }
    }
}
