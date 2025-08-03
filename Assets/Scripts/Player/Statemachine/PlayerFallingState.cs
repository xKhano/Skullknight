using Skullknight.Player.Statemachine;
using UnityEngine;

namespace Player.Statemachine
{
    public class PlayerFallingState : PlayerState
    {

        public PlayerFallingState(PlayerController _controller) : base(_controller){}
        public override void EnterState()
        {
            controller.rb.gravityScale = controller.FallingGravityScale;
            controller.Animator.Play("Fallbetween");
        } 
        public override void ExitState()
        {
            controller.rb.gravityScale = 1f;
        }

        public override void StateUpdate()
        {
            
        }

        public override void StateFixedUpdate()
        {
            float horizontal = controller.playerInput.actions["Horizontal"].ReadValue<float>();
            if (controller.rb.linearVelocity.y < -controller.MaxFallingVelocity)
            {
                controller.rb.linearVelocity = new Vector3(controller.rb.linearVelocity.x, -controller.MaxFallingVelocity, 0f);
            }
            if (controller.groundCollisionChecker.IsColliding && controller.CanJump)
            {
                controller.landingAudioPlayer.PlayRandom();
                controller.ChangeState(EPlayerState.Idle);
            }
            else if (horizontal != 0)
            {
                if (controller.rb.linearVelocity.y > 0 && controller.wallSlideCheckDeathCoroutine == null)
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
