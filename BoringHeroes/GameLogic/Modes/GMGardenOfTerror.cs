using System.Drawing;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GMGardenOfTerror : GameMode
    {
        public GMGardenOfTerror()
        {
            Name = "Garden of Terror";
            LoadingMatchPath = "./images/loading_gardenofterror.png";
            LaneRule = new[] {2, 2, 1};
            NavigationFactor = 15;
            MinimapRectangle = new Rectangle(1485, 766, 435, 285);
            LeftFountain = new Point(1531, 942);
            RightFountain = new Point(1861, 884);
            LeftBasePoint = new Point(1544, 925);
            RightBasePoint = new Point(1845, 900);
            TopLane = new[]
            {
                new Point(1562, 873), new Point(1600, 839), new Point(1650, 818),
                new Point(1730, 810), new Point(1784, 827), new Point(1813, 856)
            };
            MidLane = new[]
            {
                new Point(1602, 916), new Point(1602, 916),
                new Point(1735, 909), new Point(1791, 901)
            };
            BotLane = new[]
            {
                new Point(1581, 966), new Point(1613, 1000), new Point(1664, 1012),
                new Point(1701, 1007),
                new Point(1741, 1004), new Point(1791, 988), new Point(1816, 947)
            };
        }

        public override void PregameActions()
        {
        }
    }
}