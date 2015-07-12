using TreeSharp;

namespace BoringHeroes.GameLogic.Behaviourism
{
    internal class BehaviorManager
    {
        public Composite BaseBehavior { get; set; }
        public Composite CombatBehavior { get; set; }
        public Composite ScaredBehavior { get; set; }
        public Composite RestBehavior { get; set; }
        public Composite PushBehavior { get; set; }
    }
}