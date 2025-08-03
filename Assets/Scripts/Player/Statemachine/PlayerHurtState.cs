using System.Collections;
using Player.Statemachine;
using UnityEngine;
using DG.Tweening;

namespace Skullknight.Player.Statemachine
{
    public class PlayerHurtState : PlayerState
    {
        public PlayerHurtState(PlayerController playerController) : base(playerController)
        {
        }

        public override void EnterState()
        {
            controller.SetDamageImmunity(true);
            coroutines.Add(controller.StartCoroutine(GroundWait(1.5f)));
            //fall to ground
            //wait a bit
            //maybe health reducing animation
            //get up
            //give player immunity time
        }

        private IEnumerator GroundWait(float duration)
        {
            Time.timeScale = .5f;
            controller.Animator.Play("Fallground");
            controller.bumpImpulse.GenerateImpulse();
            yield return new WaitForSeconds(duration);
            Time.timeScale = 1;
            if (controller.Health > 0)
            {
                controller.Animator.Play("Getup");
                yield return null;
                yield return new WaitUntil(()=> controller.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > .95f);
                controller.StartCoroutine(InvincibilityDuration(2.5f));
                controller.ChangeState(EPlayerState.Idle);
            }
            else
            {
                controller.ChangeState(EPlayerState.Dead);
            }
        }

        private IEnumerator InvincibilityDuration(float duration)
        {
            controller.SpriteRenderer.color = new Color(controller.SpriteRenderer.color.r, controller.SpriteRenderer.color.g, controller.SpriteRenderer.color.b, .25f);
            yield return new WaitForSeconds(duration);
            controller.SpriteRenderer.color = new Color(controller.SpriteRenderer.color.r, controller.SpriteRenderer.color.g, controller.SpriteRenderer.color.b, 1f);
            controller.SetDamageImmunity(false);
        }
        public override void ExitState()
        {
            
        }

        public override void StateUpdate()
        {
            
        }

        public override void StateFixedUpdate()
        {
            
        }

        public override void SubscribeEvents()
        {
            
        }

        public override void UnsubscribeEvents()
        {
            
        }
    }
}