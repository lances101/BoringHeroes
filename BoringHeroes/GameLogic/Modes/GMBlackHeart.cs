using System.Drawing;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMBlackHeart : GameMode
    {
        public GMBlackHeart()
        {
            Name = "Black Heart Bay";
            LoadingMatchPath = "./images/loading_blackheart.png";
            LaneRule = new[] {2, 2, 1};
            NavigationFactor = 15;
            MinimapRectangle = new Rectangle(740, 560, 300, 200);
            MapChallengeCheck = GameModes.EmptyComposite;
            MapChallengeBehavior = GameModes.EmptyComposite;
            LeftBasePoint = new Point(784, 660);
            RightBasePoint = new Point(976, 661);
            LeftFountain = new Point(772, 648);
            RightFountain = new Point(989, 647); //989, 647
            TopLane = new[]
            {
                new Point(787, 633), new Point(787, 633), new Point(837, 584),
                new Point(881, 579),
                new Point(925, 585), new Point(948, 598), new Point(974, 637)
            };
            MidLane = TopLane;
            BotLane = TopLane;
            /*MidLane = new[]
            {
                new Point(1608, 931), new Point(1645, 876), new Point(1696, 873), new Point(1729, 870),
                new Point(1785, 925)
            };
            BotLane = new[]
            {
                new Point(1570, 978), new Point(1656, 1004), new Point(1694, 999), new Point(1737, 1004),
                new Point(1822, 975)
            };*/
        }

        public override void PregameActions()
        {
        }
    }
}