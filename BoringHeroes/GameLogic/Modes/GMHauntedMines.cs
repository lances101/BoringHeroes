using System.Drawing;
using TreeSharp;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMHauntedMines : GameMode
    {
        public GMHauntedMines(string name, string loadingMatchPath, int[] laneRule, Rectangle minimapRectangle,
            Point leftFountain, Point rightFountain, Composite mapChallengeBehavior, Point leftBasePoint,
            Point rightBasePoint, Point[] topLane, Point[] midLane, Point[] botLane)
            : base(
                name, loadingMatchPath, laneRule, minimapRectangle, leftFountain, rightFountain, mapChallengeBehavior,
                leftBasePoint, rightBasePoint, topLane, midLane, botLane)
        {
        }

        public override void PregameActions()
        {
        }
    }
}