using System.Drawing;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMCursedHollow : GameMode
    {
        public GMCursedHollow()
        {
            Name = "Cursed Hollow";
            LoadingMatchPath = "./images/loading_cursed_hollow.png";
            NavigationFactor = 15;
            LaneRule = new[] {2, 2, 1};
            MinimapRectangle = new Rectangle(1485, 767, 435, 289);
            MapChallengeBehavior = GameModes.EmptyComposite;
            LeftBasePoint = new Point(1545, 924);
            RightBasePoint = new Point(1840, 903);
            LeftFountain = new Point(1530, 943);
            RightFountain = new Point(1849, 880);
            TopLane = new[]
            {
                new Point(1559, 884), new Point(1596, 839), new Point(1653, 815), new Point(1692, 815),
                new Point(1728, 809), new Point(1778, 824)
            };
            MidLane = new[] {new Point(1601, 917), new Point(1700, 913), new Point(1700, 913), new Point(1700, 913)};
            BotLane = new[]
            {
                new Point(1588, 969), new Point(1663, 1012), new Point(1700, 1006), new Point(1739, 1007),
                new Point(1820, 952)
            };
        }

        public override void PregameActions()
        {
        }
    }
}