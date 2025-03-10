using Skullknight.Player.Statemachine;
using xKhano.StateMachines.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Skullknight.Enemy.Skeleton
{
    public class SkeletonChasingState : EntityState<Skullknight.Skeleton.ESkeletonState,Skullknight.Skeleton>
    {
        public SkeletonChasingState(Skullknight.Skeleton _controler) : base(_controler)
        { }

        public override void EnterState()
        {
            controller.SetDamageImmunity(false);
            controller.Animator.Play("Walking");
        }

        public override void ExitState()
        {
           
        }

        public override void StateUpdate()
        {
            float distance = controller.transform.position.x - PlayerController.Instance.transform.position.x;
            if (Mathf.Abs(distance) < controller.attackingDistance)
            {
                controller.ChangeState(Skullknight.Skeleton.ESkeletonState.Attacking);
            }
            else
            {
                controller.LookPlayer();
                float closestT = SplineUtility.GetNearestPoint(controller.splineContainer.Spline, new float3((float2)PlayerController.Instance.rb.position,0f), out float3 closestPoint, out float asd);
                float step = Mathf.Clamp(asd - controller.splinePosition,-controller.evaluationRate,controller.evaluationRate);
                controller.splinePosition += step * Time.deltaTime;
                controller.transform.position =
                    controller.splineContainer.Spline.EvaluatePosition(controller.splinePosition);
            }
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