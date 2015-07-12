using System.Drawing;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMSkyTemple : GameMode
    {
        public GMSkyTemple()
        {
            Name = "Sky Temple";
            LoadingMatchPath = "./images/loading_skytemple.png";
            LaneRule = new[] {2, 2, 1};
            NavigationFactor = 15;
            MinimapRectangle = new Rectangle(1500, 670, 430, 400);
            MapChallengeBehavior = GameModes.EmptyComposite;
            LeftBasePoint = new Point(1580, 806);
            RightBasePoint = new Point(1847, 808);
            LeftFountain = new Point(1561, 787);
            RightFountain = new Point(1863, 789);
            TopLane = new[]
            {
                new Point(1603, 792), new Point(1673, 742), new Point(1714, 744), new Point(1753, 740),
                new Point(1815, 766)
            };
            MidLane = new[]
            {
                new Point(1632, 825), new Point(1681, 836), new Point(1713, 839), new Point(1747, 833),
                new Point(1795, 819)
            };
            BotLane = new[]
            {
                new Point(1607, 853), new Point(1679, 913), new Point(1713, 909), new Point(1747, 912),
                new Point(1815, 850)
            };
        }

        public override void PregameActions()
        {
        }
    }
}