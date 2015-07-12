using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.Interaction;
using TreeSharp;

namespace BoringHeroes.GameLogic.Behaviourism.Actions
{
    internal class HealWithAbilityAction : Action
    {
        protected override RunStatus Run(object context)
        {
            var gameState = (CurrentGameState) context;
            foreach (var skill in gameState.Me.HeroData.Skills)
            {
                if ((skill.Properties.Contains(Skill.SkillProperty.SoloHeal) ||
                     skill.Properties.Contains(Skill.SkillProperty.AOEHeal)) && skill.IsCastable)
                {
                    if (skill.Cooldown < gameState.Me.HeroHP/3)
                    {
                        ControlInput.SendKey(skill.Key);
                        ControlInput.MouseClick(ControlInput.MouseClickButton.Left,
                            gameState.Me.HeroScreenPosition);
                        return RunStatus.Success;
                    }
                }
            }
            return RunStatus.Failure;
        }
    }
}