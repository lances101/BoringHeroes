using System.Collections.Generic;
using System.Drawing;
using TreeSharp;

namespace BoringHeroes.GameLogic.Modes
{
    public abstract class GameMode
    {
        public GameMode(string name, string loadingMatchPath, int[] laneRule, Rectangle minimapRectangle,
            Point leftFountain, Point rightFountain, Composite mapChallengeBehavior,
            Point leftBasePoint, Point rightBasePoint, Point[] topLane, Point[] midLane, Point[] botLane)
        {
            Name = name;
            LoadingMatchPath = loadingMatchPath;
            LaneRule = laneRule;
            MapChallengeBehavior = mapChallengeBehavior;
            LeftFountain = leftFountain;
            RightFountain = rightFountain;
            MinimapRectangle = minimapRectangle;
            LeftBasePoint = leftBasePoint;
            RightBasePoint = rightBasePoint;
            TopLane = topLane;
            MidLane = midLane;
            BotLane = botLane;
        }

        protected GameMode()
        {
        }

        public string Name { get; set; }
        public string LoadingMatchPath { get; set; }
        public int NavigationFactor { get; set; }
        public Composite MapChallengeCheck { get; set; }
        public Composite MapChallengeBehavior { get; set; }

        public Point[] TopPushVector
        {
            get
            {
                var p = new List<Point>();
                p.Add(LeftBasePoint);
                p.AddRange(TopLane);
                p.Add(RightBasePoint);
                InterpolatePoints(p);
                return p.ToArray();
            }
        }

        public Point[] MidPushVector
        {
            get
            {
                var p = new List<Point>();
                p.Add(LeftBasePoint);
                p.AddRange(MidLane);
                p.Add(RightBasePoint);
                InterpolatePoints(p);
                return p.ToArray();
            }
        }

        public Point[] BotPushVector
        {
            get
            {
                var p = new List<Point>();
                p.Add(LeftBasePoint);
                p.AddRange(BotLane);
                p.Add(RightBasePoint);
                InterpolatePoints(p);
                return p.ToArray();
            }
        }

        public Point LeftFountain { get; set; }
        public Point RightFountain { get; set; }
        public Point LeftBasePoint { get; set; }
        public Point RightBasePoint { get; set; }
        public int[] LaneRule { get; set; }
        public Point[] TopLane { get; set; }
        public Point[] MidLane { get; set; }
        public Point[] BotLane { get; set; }
        public Rectangle MinimapRectangle { get; set; }

        private void InterpolatePoints(List<Point> p)
        {
            for (var i = 0; i < p.Count - 1;)
            {
                p.Insert(i + 1, new Point((p[i].X + p[i + 1].X)/2, (p[i].Y + p[i + 1].Y)/2));
                i += 2;
            }
        }

        public abstract void PregameActions();
    }
}