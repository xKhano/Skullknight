using Player.Statemachine;

namespace Skullknight.Player.Statemachine
{
    public class PlayerDeadState : PlayerState
    {
        public PlayerDeadState(PlayerController controller) : base(controller)
        { }

        public override void EnterState()
        {
            controller.SetDamageImmunity(true);
        }

        public override void ExitState()
        {
            controller.SetDamageImmunity(false);
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