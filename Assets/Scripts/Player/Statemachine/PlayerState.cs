using System.Collections.Generic;
using Skullknight.Player.Statemachine;
using xKhano.StateMachines.Entities;

namespace Player.Statemachine
{
    public abstract class PlayerState : EntityState<EPlayerState, PlayerController>
    {
        protected PlayerState(PlayerController controller) : base(controller){}
    }
    public enum EPlayerState
    {
        Idle,
        Running,
        Sliding,
        Wallsliding,
        Roll,
        Jumping,
        Falling,
        Hanging,
        Crouching,
        Climbing,
        AttackOne,
        AttackTwo,
        Hurt,
        CrouchAttack,
        Dead
    }
}
