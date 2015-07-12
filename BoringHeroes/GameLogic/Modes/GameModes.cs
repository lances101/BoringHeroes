using System.Collections.Generic;
using System.Drawing;
using TreeSharp;

namespace BoringHeroes.GameLogic.Modes
{
    internal class GameModes
    {
        public static Composite EmptyComposite = new Action(a => RunStatus.Failure);
        public static GMBlackHeart BlackHeart = new GMBlackHeart();
        public static GMCursedHollow CursedHollow = new GMCursedHollow();
        public static GMDragonshire DragonShire = new GMDragonshire();
        public static GMGardenOfTerror GardenOfTerror = new GMGardenOfTerror();

        public static GMHauntedMines HauntedMines = new GMHauntedMines("Haunted Mines",
            "./images/loading_hauntedmines.png", new[] {2, 2, 1}, new Rectangle(0, 0, 0, 0),
            mapChallengeBehavior: EmptyComposite,
            leftBasePoint: new Point(0, 0), rightBasePoint: new Point(0, 0),
            leftFountain: new Point(0, 0), rightFountain: new Point(0, 0),
            topLane: new[] {new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0)},
            midLane: new[] {new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0)},
            botLane: new[] {new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0)});

        public static GMSkyTemple SkyTemple = new GMSkyTemple();

        public static List<GameMode> ModesList = new List<GameMode>
        {
            BlackHeart,
            CursedHollow,
            DragonShire,
            GardenOfTerror,
            HauntedMines,
            SkyTemple
        };
    }
}