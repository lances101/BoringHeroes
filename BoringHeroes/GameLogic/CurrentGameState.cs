using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.GameLogic.Modes;

namespace BoringHeroes.GameLogic
{
    public class CurrentGameState
    {
        public enum FriendlySide
        {
            Undecided,
            Left,
            Right
        }

        public CurrentGameState()
        {
            Me = new CurrentHeroData();
            Minimap = new MinimapData();
        }

        public CurrentGameState(GameMode gameMode, Hero hero)
        {
            Me = new CurrentHeroData();
            Minimap = new MinimapData();
            Me.HeroData = hero;
            GameMode = gameMode;
        }

        public FriendlySide CurrentSide { get; set; }
        public CurrentHeroData Me { get; set; }
        public MinimapData Minimap { get; set; }
        public GameMode GameMode { get; set; }
        public bool IsGameEnd { get; set; }

        public bool IsPlayerSafe
        {
            get
            {
                if (EnemyHeroes.Length == 0 && EnemyCreepsNearby.Length == 0
                    //&& LastReceivedDamageSeconds > 5 
                    && TowersFound.Length == 0)
                {
                    return true;
                }
                return false;
            }
        }

        public ScreenReader.OnScreenHero[] FriendlyHeroes { get; set; }
        public Point[] TowersFound { get; set; }

        public Point ClosestTower
        {
            get { return TowersFound.OrderBy(o => o.Distance2D(Me.HeroScreenPosition)).ElementAt(0); }
        }

        public ScreenReader.OnScreenHero[] EnemyHeroes { get; set; }

        public Point ClosestEnemyHero
        {
            get
            {
                if (!EnemyHeroes.Any()) return Me.HeroScreenPosition;
                return EnemyHeroes.OrderBy(o => o.Position.Distance2D(Me.HeroScreenPosition)).ElementAt(0);
            }
        }

        public Point[] FriendlyCreepsNearby { get; set; }
        public Point[] EnemyCreepsNearby { get; set; }

        public class CurrentHeroData
        {
            private float heroHp = -101;
            private bool isMounted;
            public DateTime LastHeroAliveTime = DateTime.Now;
            private DateTime lastReceivedDamage = DateTime.Now;
            private float previousHeroHp = -101;
            public Hero HeroData { get; set; }
            public Point HeroScreenPosition { get; set; }
            public Point HeroMinimapPosition { get; set; }

            public float HeroHP
            {
                get { return heroHp; }
                set
                {
                    previousHeroHp = heroHp;
                    heroHp = value;
                    if (heroHp > 0)
                        LastHeroAliveTime = DateTime.Now;
                    if (heroHp - previousHeroHp < 0)
                    {
                        lastReceivedDamage = DateTime.Now;
                    }
                }
            }

            public bool CanLevelTalents { get; set; }
            public int CharacterLevel { get; set; }
            public float HeroMP { get; set; }

            public bool IsMounted
            {
                get { return isMounted; }
                set
                {
                    var last = IsMounted;
                    isMounted = value;
                    if (last && IsMounted == false)
                        DismountedAt = DateTime.Now;
                }
            }

            public DateTime DismountedAt { get; set; }

            public bool IsAlive
            {
                get
                {
                    if (HeroHP > 0)
                        return true;
                    return false;
                }
            }

            public int LastReceivedDamageSeconds
            {
                get { return (int) (DateTime.Now - lastReceivedDamage).TotalSeconds; }
            }

            public bool CanCaptureSmallCamp { get; set; }
            public bool CanCaptureBigCamp { get; set; }
            public List<Point> SmallCamps { get; set; }
            public List<Point> BigCamps { get; set; }
        }

        public class MinimapData
        {
            public int[][] TowerStatus =
            {
                new[] {0, 0, 0, 0},
                new[] {0, 0, 0, 0, 0, 0},
                new[] {0, 0, 0, 0}
            };

            public bool MapChallengeAvailable { get; set; }
            public Point[] CampsAvailable { get; set; }
        }
    }
}