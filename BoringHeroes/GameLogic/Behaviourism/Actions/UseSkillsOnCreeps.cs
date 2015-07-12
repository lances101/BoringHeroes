using System;
using System.Drawing;
using System.Linq;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.Interaction;
using TreeSharp;
using Action = TreeSharp.Action;

namespace BoringHeroes.GameLogic.Behaviourism.Actions
{
    internal class UseSkillsOnCreeps : Action
    {
        protected override RunStatus Run(object context)
        {
            var gameState = (CurrentGameState) context;
            foreach (var skill in gameState.Me.HeroData.Skills)
            {
                if (!skill.ShouldCastOnCreeps) continue;
                if (DateTime.Now < skill.OnCooldownUntil) continue;

                if (skill.Properties.Contains(Skill.SkillProperty.Buff))
                {
                    if (gameState.FriendlyCreepsNearby.Any(o =>
                        o.Distance2D(gameState.Me.HeroScreenPosition) < 500))
                    {
                        ControlInput.SendKey(skill.Key);
                        ControlInput.MouseClick(ControlInput.MouseClickButton.Left,
                            gameState.FriendlyCreepsNearby.First());
                        skill.OnCooldownUntil = DateTime.Now.AddSeconds(skill.Cooldown);
                        MainWindow.Log(MainWindow.LogType.Combat,
                            string.Format("Using '{0}' (range - {1})", skill.Name, skill.Range));

                        return RunStatus.Success;
                    }
                }
                if (!gameState.EnemyCreepsNearby.Any()) return RunStatus.Failure;
                var unit =
                    gameState.EnemyCreepsNearby.OrderBy(
                        o => o.Distance2D(gameState.Me.HeroScreenPosition)).First();
                //TODO: ADD SMART AOE. THIS JUST SUCKS.
                var dist = unit.Distance2D(gameState.Me.HeroScreenPosition);
                if (dist < skill.Range)
                {
                    var center =
                        new Point(
                            gameState.EnemyCreepsNearby.Sum(o => o.X)/
                            gameState.EnemyCreepsNearby.Length,
                            gameState.EnemyCreepsNearby.Sum(o => o.Y)/
                            gameState.EnemyCreepsNearby.Length);
                    ControlInput.SendKey(skill.Key);
                    ControlInput.MouseClick(ControlInput.MouseClickButton.Left, unit);
                    skill.OnCooldownUntil = DateTime.Now.AddSeconds(skill.Cooldown);
                    MainWindow.Log(MainWindow.LogType.Combat,
                        string.Format("Using '{0}' ({1} -> {2})", skill.Name, skill.Range, dist));
                    return RunStatus.Success;
                }
            }
            return RunStatus.Failure;
        }
    }
}