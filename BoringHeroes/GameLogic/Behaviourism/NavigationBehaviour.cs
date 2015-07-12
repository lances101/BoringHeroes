using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BoringHeroes.Interaction;
using TreeSharp;
using Action = TreeSharp.Action;

namespace BoringHeroes.GameLogic.Behaviourism
{
    internal class NavigationRoutine
    {
        private Composite _root;

        public NavigationRoutine(CurrentGameState gameState)
        {
            GameState = gameState;
        }

        private CurrentGameState GameState { get; set; }
        private Point? NavPoint { get; set; }

        public bool HasNavPoint
        {
            get { return NavPoint.HasValue; }
        }

        private bool Aggressive { get; set; }
        private bool ForceWalk { get; set; }

        public Composite Root
        {
            get
            {
                return _root ?? (_root = new PrioritySelector(
                    new Decorator(c => { return !NavPoint.HasValue; }, new Action(a => RunStatus.Success)),
                    new PrioritySelector(
                        new PrioritySelector(
                            new Decorator(c => GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value) < 60,
                                new Action(a => RunStatus.Failure)),
                            new Decorator(
                                c =>
                                    (GameState.CurrentSide == CurrentGameState.FriendlySide.Left
                                        ? GameState.GameMode.LeftFountain
                                        : GameState.GameMode.RightFountain)
                                        .Distance2D(NavPoint.Value) <
                                    GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value) &&
                                    GameState.IsPlayerSafe
                                    &&
                                    GameState.Me.HeroMinimapPosition.Distance2D(GameState.CurrentSide ==
                                                                                CurrentGameState.FriendlySide.Left
                                        ? GameState.GameMode.LeftFountain
                                        : GameState.GameMode.RightFountain) > 30,
                                new Action(a =>
                                {
                                    MainWindow.Log("Teleporting | " +
                                                   GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value));
                                    TeleportAction();
                                    return RunStatus.Success;
                                }
                                    )),
                            new Decorator(c =>
                            {
                                return false;
                                //ScreenReader.UpdateHeroMounted(GameState);
                                if (DateTime.Now < GameState.Me.DismountedAt.AddSeconds(5)) return false;
                                return GameState.IsPlayerSafe && !GameState.Me.IsMounted;
                            },
                                new Action(a =>
                                {
                                    MainWindow.Log(MainWindow.LogType.Navigation, "Mounting...");
                                    MountingRoutine();
                                    return RunStatus.Success;
                                })
                                )
                            ),
                        new PrioritySelector(
                            new Decorator(c =>
                                GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value) <
                                GameState.GameMode.NavigationFactor,
                                new Action(a =>
                                {
                                    NavPoint = null;
                                    return RunStatus.Success;
                                })),
                            new Decorator(c =>
                                GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value) >=
                                GameState.GameMode.NavigationFactor,
                                new Action(a =>
                                {
                                    //MainWindow.Log("Distance was " +
                                    //               GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value));
                                    MainWindow.Log(MainWindow.LogType.Navigation,
                                        string.Format("Moving to {0:F} AGGRO({1})",
                                            GameState.Me.HeroMinimapPosition.Distance2D(NavPoint.Value),
                                            Aggressive));
                                    if (Aggressive) ControlInput.SendKey(Keys.A);
                                    ControlInput.MouseClick(
                                        Aggressive
                                            ? ControlInput.MouseClickButton.Left
                                            : ControlInput.MouseClickButton.Right,
                                        NavPoint.Value);
                                    return RunStatus.Failure;
                                }))
                            )
                        )
                    ))
                    ;
            }
        }

        private Composite TeleportAction()
        {
            ControlInput.SendKey(Keys.B);
            return null;
        }

        public void SetNewPoint(Point? navPoint, bool aggro, bool forceWalk = false)
        {
            NavPoint = navPoint;
            Aggressive = aggro;
            ForceWalk = forceWalk;
        }

        private Composite MountingRoutine()
        {
            ControlInput.SendKey(Keys.S);
            ControlInput.SendKey(Keys.Z);
            Thread.Sleep(1100);
            return null;
        }

        internal void Flee()
        {
        }
    }
}