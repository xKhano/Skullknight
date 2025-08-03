using System.Collections;
using Skullknight.Player.Statemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Player.Statemachine
{
    public class PlayerWallSlideState : PlayerState
    {
        public PlayerWallSlideState(PlayerController stateManager) : base(stateManager)
        {
        
        }

        public override void ExitState()
        {
            controller.rb.constraints =  controller.rb.constraints ^ RigidbodyConstraints2D.FreezePositionX;
        }

        public override void StateFixedUpdate()
        {
            RaycastHit2D result = Physics2D.BoxCast(
                controller.wallCheckCollider.bounds.center,
                controller.wallCheckCollider.bounds.size,
                0f,
                Vector2.zero,
                0f,
                controller.wallMask);
            if (controller.groundCollisionChecker.IsColliding)
            {
                controller.ChangeState(EPlayerState.Idle);
            }
            else if (result.collider == null)
            {
                controller.ChangeState(EPlayerState.Falling);
            }
        }

        public override void SubscribeEvents()
        {
            controller.playerInput.actions["Jump"].performed += OnJumpPerformed;
        }

        private void OnJumpPerformed(InputAction.CallbackContext obj)
        {
            if (controller.rb.linearVelocity.y > 0)
            {
                Vector2 vel = controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right;
                vel *= controller.maxRunningVelocity;
                vel += Vector2.up * controller.wallJumpForce;
                controller.rb.linearVelocity = vel;
            }
            else controller.rb.linearVelocity = new Vector2(controller.SpriteRenderer.flipX ? -controller.maxRunningVelocity : controller.maxRunningVelocity,controller.jumpVelocity);

            controller.wallSlideCheckDeathCoroutine = controller.StartCoroutine(WallslideDeath());
            controller.ChangeState(EPlayerState.Falling);
        }

        public override void UnsubscribeEvents()
        {
            controller.playerInput.actions["Jump"].performed -= OnJumpPerformed;
        }

        public override void EnterState()
        {
            controller.Animator.Play("WallSlide");
            controller.rb.constraints =  controller.rb.constraints ^ RigidbodyConstraints2D.FreezePositionX;
            if (controller.rb.linearVelocity.y < 0)
            {
                controller.rb.linearVelocity = Vector2.down * controller.rb.linearVelocity.magnitude;
            }
            else
            {
                controller.rb.linearVelocity = Vector2.up * controller.rb.linearVelocity.magnitude;
            }
        }

        public override void StateUpdate()
        {
            
        
        }

        private IEnumerator WallslideDeath()
        {
            yield return new WaitForSeconds(controller.wallJumpDeathDuration);
            controller.wallSlideCheckDeathCoroutine = null;
        }
    }
}
