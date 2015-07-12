using System.Drawing;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMDragonshire : GameMode
    {
        public GMDragonshire()
        {
            Name = "Dragonshire";
            LoadingMatchPath = "./images/loading_dragonshire.png";
            LaneRule = new[] {2, 2, 1};
            NavigationFactor = 10;
            MinimapRectangle = new Rectangle(1486, 793, 420, 300);
            MapChallengeCheck = GameModes.EmptyComposite;
            MapChallengeBehavior = GameModes.EmptyComposite;
            LeftBasePoint = new Point(1557, 935);
            RightBasePoint = new Point(1833, 930);
            LeftFountain = new Point(1536, 906);
            RightFountain = new Point(1853, 910);
            TopLane = new[]
            {
                new Point(1599, 895), new Point(1661, 852), new Point(1697, 847), new Point(1737, 849),
                new Point(1794, 897)
            };
            MidLane = new[]
            {
                new Point(1614, 935), new Point(1654, 934), new Point(1696, 923), new Point(1696, 923),
                new Point(1778, 936)
            };
            BotLane = new[]
            {
                new Point(1593, 970), new Point(1660, 995), new Point(1698, 992), new Point(1736, 995),
                new Point(1800, 973)
            };
        }

        public override void PregameActions()
        {
        }
    }
}