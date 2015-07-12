using System.Threading;
using BoringHeroes.Interaction;
using TreeSharp;

namespace BoringHeroes.GameLogic.Behaviourism.Actions
{
    internal class LevelTalentsAction : Action
    {
        protected override RunStatus Run(object context)
        {
            var gameState = (CurrentGameState) context;
            if (!ScreenReader.AreTalentsOpen())
                ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 95, 942);
            Thread.Sleep(700);
            //ScreenReader.UpdateCurrentTalentLevel(gameState);
            MainWindow.Log("Leveling talents for level " + gameState.Me.CharacterLevel);
            foreach (var talent in gameState.Me.HeroData.Talents)
            {
                if (gameState.Me.CharacterLevel < talent.TalentLevel) continue;
                if (!talent.IsLeveled)
                {
                    ControlInput.ChooseTalent(talent.TalentLevel, talent.TalentIndex);
                    talent.IsLeveled = true;
                    talent.HeroChanges(gameState.Me.HeroData);
                    MainWindow.Log("Leveling " + talent.TalentName + "(" + talent.TalentLevel + ")");
                    Thread.Sleep(200);
                }
            }
            Thread.Sleep(400);
            if (ScreenReader.AreTalentsOpen())
                ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 95, 942);
            return RunStatus.Failure;
        }
    }
}