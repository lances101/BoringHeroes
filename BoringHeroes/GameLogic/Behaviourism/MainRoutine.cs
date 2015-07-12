using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BoringHeroes.GameLogic.Behaviourism.Actions;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.Interaction;
using TreeSharp;
using Action = TreeSharp.Action;

namespace BoringHeroes.GameLogic.Behaviourism
{
    internal class MainRoutine
    {
        private static DateTime lastMain = DateTime.Now;
        private readonly int EngageDistance = 600;
        private readonly bool UpdateInside = true;
        private Composite _careEnemies;
        private Composite _careSelf;
        private Composite _choosePushingTarget;
        private DateTime _nextTalentCheck = DateTime.Now;
        private Composite _restRoutine;
        private PrioritySelector _root;
        private Composite _scaredRoutine;
        private Composite _timedRoutine;
        private Composite defaultPushingBehavior;
        private bool IsDebugging = false;
        private bool WasScaredBefore;

        public MainRoutine(CurrentGameState currentGameState)
        {
            GameState = currentGameState;

            NavRout = new NavigationRoutine(GameState);
        }

        private CurrentGameState GameState { get; set; }
        private NavigationRoutine NavRout { get; set; }
        private Point[] CurrentPushVector { get; set; }
        private int PushVectorIndex { get; set; }

        public Composite Root
        {
            get
            {
                return _root ?? (_root = new PrioritySelector("Main Start",
                    new Action(s =>
                    {
                        MainWindow.DebugReaderStats("Total:" + TickNow(lastMain));
                        lastMain = DateTime.Now;
                        if (UpdateInside)
                            if (CurrentPushVector != null)
                                ScreenReader.DebugPlotVectorOnMap(GameState, CurrentPushVector);
                        return RunStatus.Failure;
                    }),
                    TakeCareOfYourself,
                    TakeCareOfEnemies,
                    ChooseCurrentTask,
                    TimedRoutine,
                    //Select Pushing Behavior
                    new PrioritySelector("Choose Pushing Behaviour",
                        new Decorator("Do we have a PushComposite?",
                            c => { return GameState.Me.HeroData.PushComposite != null; },
                            new Action(a => { return GameState.Me.HeroData.PushComposite.Tick(null); })),
                        DefaultPushingBehavior
                        )
                    )
                    );
            }
        }

        public Composite TimedRoutine
        {
            get
            {
                return _timedRoutine ?? (_timedRoutine =
                    new PrioritySelector(
                        new Decorator(c => DateTime.Now > _nextTalentCheck,
                            new Sequence(
                                new DecoratorContinue(c => GameState.Me.CanLevelTalents,
                                    new LevelTalentsAction()
                                    ),
                                new Action(c => { _nextTalentCheck = DateTime.Now.AddSeconds(60); })
                                )
                            )
                        )
                    );
            }
        }

        public Composite TakeCareOfYourself
        {
            get
            {
                return _careSelf ?? (_careSelf =
                    new PrioritySelector("Take care of self",
                        new Decorator("Should check in ingame", c =>
                        {
                            //TODO:Are we ingame check
                            return false;
                        }, new Action(a => RunStatus.Success)),
                        new Decorator("Hero HP Check", s =>
                        {
                            if (UpdateInside) ScreenReader.UpdateHeroHP(GameState);
                            return !GameState.Me.IsAlive;
                        },
                            new Action(t =>
                            {
                                MainWindow.Log(MainWindow.LogType.Log, "Hero is dead. Waiting.");
                                Thread.Sleep(1000);
                                CurrentPushVector = null;
                                NavRout.SetNewPoint(null, false);
                                return RunStatus.Success;
                            })
                            ),
                        ScaredRoutine,
                        RestRoutine
                        ));
            }
        }

        public Composite RestRoutine
        {
            get
            {
                return _restRoutine ?? (_restRoutine =
                    new Sequence(
                        new PrioritySelector("Resting piece of shit",
                            //hp check
                            new Decorator("HP < 65?", c =>
                            {
                                if (UpdateInside) ScreenReader.UpdateHeroHP(GameState);
                                return (GameState.Me.HeroHP < 75);
                            },
                                new PrioritySelector("Choose your hp..",
                                    new Decorator("> 30", c => GameState.Me.HeroHP > 30,
                                        new PrioritySelector("Healing stuff",
                                            new HealWithAbilityAction(),
                                            new HealWithWell()
                                            )
                                        ),
                                    new Decorator("< 30", c => GameState.Me.HeroHP <= 30,
                                        new Decorator("Run back to fountain", c =>
                                        {
                                            NavRout.SetNewPoint(
                                                GameState.CurrentSide == CurrentGameState.FriendlySide.Left
                                                    ? GameState.GameMode.LeftFountain
                                                    : GameState.GameMode.RightFountain, false, false);
                                            //PushVectorIndex = 0; //TODO: ONLY ON ARRIVAL (!)
                                            return true;
                                        }, NavRout.Root)
                                        )
                                    )
                                ),
                            new Action(c =>
                            {
                                if (
                                    !GameState.Me.HeroData.Skills.Any(
                                        o =>
                                            o.Properties.Contains(Skill.SkillProperty.AOEHeal) ||
                                            o.Properties.Contains(Skill.SkillProperty.SoloHeal)))
                                    return RunStatus.Failure;
                                if (GameState.FriendlyHeroes.All(o => o.MatchesCount > 90)) return RunStatus.Failure;

                                var hero = GameState.FriendlyHeroes.OrderBy(o => o.MatchesCount).First();
                                foreach (var skill in GameState.Me.HeroData.Skills)
                                {
                                    if (
                                        !(skill.Properties.Contains(Skill.SkillProperty.AOEHeal) ||
                                          skill.Properties.Contains(Skill.SkillProperty.SoloHeal))) continue;
                                    if (GameState.Me.HeroScreenPosition.Distance2D(hero.Position) < skill.Range + 100
                                        && DateTime.Now > skill.OnCooldownUntil)
                                    {
                                        ControlInput.SendKey(skill.Key);
                                        ControlInput.MouseClick(ControlInput.MouseClickButton.Left, hero.Position);
                                        skill.OnCooldownUntil = DateTime.Now.AddSeconds(skill.Cooldown);
                                        Thread.Sleep(100);
                                        Thread.Sleep(skill.SkillCastTime);
                                        return RunStatus.Success;
                                    }
                                }

                                return RunStatus.Failure;
                            })
                            )
                        )
                    );
            }
        }

        public Composite TakeCareOfEnemies
        {
            get
            {
                return _careEnemies ?? (_careEnemies =
                    //Psuedo-Combat routine
                    new Decorator("Take care of enemeis", s =>
                    {
                        //if (UpdateInside) ScreenReader.UpdateHeroes(GameState);
                        //if (UpdateInside) ScreenReader.UpdateUnits(GameState);
                        return !GameState.IsPlayerSafe;
                    },
                        new PrioritySelector("We are not safe",
                            //Actual Combat With Heroes
                            new PrioritySelector("Combat stuff",
                                new Decorator("Any heroes? Nearby?", c =>
                                {
                                    if (GameState.EnemyHeroes.Count() == 0) return false;
                                    if (GameState.ClosestEnemyHero.Distance2D(GameState.Me.HeroScreenPosition) <
                                        EngageDistance)
                                        return true;
                                    return false;
                                },
                                    // ability logic goes here
                                    new PrioritySelector("Ability logic",
                                        //TODO: Implement hero AOE
                                        new Decorator("Do you even cast?", c => true,
                                            new Action(a =>
                                            {
                                                var dist =
                                                    GameState.ClosestEnemyHero.Distance2D(
                                                        GameState.Me.HeroScreenPosition);
                                                foreach (var skill in GameState.Me.HeroData.Skills)
                                                {
                                                    switch (skill.Properties[0])
                                                    {
                                                        case Skill.SkillProperty.SoloHeal:
                                                        case Skill.SkillProperty.AOEHeal:
                                                            if (GameState.Me.HeroHP > 50) break;
                                                            if (DateTime.Now < skill.OnCooldownUntil)
                                                                continue;
                                                            MainWindow.Log(MainWindow.LogType.Combat,
                                                                string.Format("HEAL Skill {0} on target at {1:F}",
                                                                    skill.Name, dist));
                                                            ControlInput.SendKey(skill.Key);
                                                            ControlInput.MouseClick(
                                                                ControlInput.MouseClickButton.Left,
                                                                GameState.Me.HeroScreenPosition);
                                                            skill.OnCooldownUntil =
                                                                skill.OnCooldownUntil.AddSeconds(skill.Cooldown);
                                                            Thread.Sleep(100);
                                                            Thread.Sleep(skill.SkillCastTime);
                                                            return RunStatus.Success;
                                                            break;

                                                        case Skill.SkillProperty.Buff:
                                                            if (
                                                                skill.Properties.Contains(Skill.SkillProperty.Ultimate) &&
                                                                GameState.Me.HeroHP > 80 ||
                                                                DateTime.Now < skill.OnCooldownUntil) continue;
                                                            MainWindow.Log(MainWindow.LogType.Combat,
                                                                string.Format("Buffing up! Skill {0}",
                                                                    skill.Name, dist));
                                                            ControlInput.SendKey(skill.Key);
                                                            ControlInput.MouseClick(
                                                                ControlInput.MouseClickButton.Left,
                                                                GameState.Me.HeroScreenPosition);
                                                            skill.OnCooldownUntil =
                                                                skill.OnCooldownUntil.AddSeconds(skill.Cooldown);
                                                            Thread.Sleep(100);
                                                            Thread.Sleep(skill.SkillCastTime);
                                                            break;
                                                        case Skill.SkillProperty.SoloDamage:
                                                        case Skill.SkillProperty.AOEDamage:
                                                            if ((skill.Range < dist || skill.Range == -1) ||
                                                                DateTime.Now < skill.OnCooldownUntil)
                                                                continue;
                                                            MainWindow.Log(MainWindow.LogType.Combat,
                                                                string.Format("Skill {0} on target at {1:F}", skill.Name,
                                                                    dist));
                                                            ControlInput.SendKey(skill.Key);
                                                            ControlInput.MouseClick(
                                                                ControlInput.MouseClickButton.Left,
                                                                GameState.ClosestEnemyHero);
                                                            skill.OnCooldownUntil =
                                                                skill.OnCooldownUntil.AddSeconds(skill.Cooldown);
                                                            Thread.Sleep(100);
                                                            Thread.Sleep(skill.SkillCastTime);
                                                            return RunStatus.Success;
                                                            break;
                                                    }
                                                }
                                                return RunStatus.Failure;
                                            })
                                            ),

                                        //Just attack
                                        new Action(a =>
                                        {
                                            MainWindow.Log(MainWindow.LogType.Combat,
                                                string.Format("Attacking hero at {0:F} units",
                                                    GameState.ClosestEnemyHero.Distance2D(
                                                        GameState.Me.HeroScreenPosition)));

                                            ControlInput.MouseClick(ControlInput.MouseClickButton.Right,
                                                GameState.ClosestEnemyHero);
                                            return RunStatus.Success;
                                        }))
                                    )
                                )
                            )));
            }
        }

        public Composite ScaredRoutine
        {
            get
            {
                return _scaredRoutine ?? (_scaredRoutine =

                    #region HandleScared

                    new Sequence("Scared check [outer] ",
                        new PrioritySelector("Scare check [inner]",
                            //hp check
                            new Decorator("HP Check", c =>
                            {
                                if (UpdateInside)
                                {
                                    ScreenReader.UpdateHeroHP(GameState);
                                    ScreenReader.UpdateTowers(GameState);
                                    ScreenReader.UpdateHeroes(GameState);
                                    ScreenReader.UpdateUnits(GameState);
                                }
                                MainWindow.SetHp(GameState.Me.HeroHP);
                                return (GameState.Me.HeroHP < 35);
                            }, new Action(a => RunStatus.Failure)),

                            #region TowerCheck

                            //Find problem from towercheck
                            new Sequence("Tower Check",
                                new Decorator("Towers around?", c =>
                                {
                                    //if (UpdateInside) ScreenReader.UpdateTowers(GameState);
                                    return GameState.TowersFound.Any();
                                },
                                    new Action(t => { return RunStatus.Success; })),
                                new Sequence("Towers around!",
                                    new Decorator("Tower is close?", c => GameState.ClosestTower.Distance2D(
                                        GameState.Me.HeroScreenPosition) < 800,
                                        new Action(t =>
                                        {
                                            MainWindow.Log(MainWindow.LogType.Combat, "TOWA IS CLOZE!");
                                            return RunStatus.Success;
                                        })),
                                    new Decorator("Tower IS CLOSE!", c =>
                                    {
                                        MainWindow.Log(MainWindow.LogType.Combat,
                                            "Creep check - " +
                                            GameState.FriendlyCreepsNearby.All(
                                                o => o.Distance2D(GameState.ClosestTower) > 800));
                                        return
                                            GameState.FriendlyCreepsNearby.All(
                                                o => o.Distance2D(GameState.ClosestTower) > 800);
                                    }
                                        , new Action(t =>
                                        {
                                            MainWindow.Log(MainWindow.LogType.Combat, "Tower hitting! NOPE OUT.");
                                            return RunStatus.Success;
                                        }))
                                    )
                                )

                            #endregion

                            #region Hero Check

                            //Find problem from hero
                            ,
                            new Sequence("Hero Check",
                                new Decorator("Any heroes?", c => { return GameState.EnemyHeroes.Any(); },
                                    new Action(t => { return RunStatus.Success; })),
                                new Decorator("More than we can handle?",
                                    c =>
                                        GameState.EnemyHeroes.Length >
                                        GameState.FriendlyHeroes.Length + 2,
                                    new Action(t => { return RunStatus.Success; })
                                    )
                                #endregion HeroCheck

                                )
                            #endregion

                            ),
                        new PrioritySelector("We are running away",
                            new Decorator("Do you even have the skillz?", a =>
                            {
                                if (
                                    !GameState.Me.HeroData.Skills.Any(
                                        o => o.Properties.Contains(Skill.SkillProperty.LocalMobility))) return false;
                                return true;
                            }, new Action(a =>
                            {
                                foreach (var skill in GameState.Me.HeroData.Skills)
                                {
                                    if (DateTime.Now > skill.OnCooldownUntil &&
                                        skill.Properties.Contains(Skill.SkillProperty.LocalMobility))
                                    {
                                        ControlInput.SendKey(Keys.Space);
                                        ControlInput.SendKey(skill.Key);
                                        ControlInput.MouseClick(ControlInput.MouseClickButton.Left,
                                            GameState.Me.HeroScreenPosition.X +
                                            (skill.Range*
                                             (GameState.CurrentSide == CurrentGameState.FriendlySide.Left ? -1 : 1)),
                                            GameState.Me.HeroScreenPosition.Y);
                                        Thread.Sleep(100);
                                        ControlInput.MouseClick(ControlInput.MouseClickButton.Left,
                                            GameState.CurrentSide == CurrentGameState.FriendlySide.Left ? 50 : 1900,
                                            GameState.Me.HeroScreenPosition.Y);
                                        skill.OnCooldownUntil = DateTime.Now.AddSeconds(skill.Cooldown);
                                        Thread.Sleep(100);
                                        Thread.Sleep(skill.SkillCastTime);
                                        MainWindow.Log(MainWindow.LogType.Combat, "Escaping with " + skill.Name);
                                        return RunStatus.Success;
                                    }
                                }
                                return RunStatus.Failure;
                            })),
                            new Action(a =>
                            {
                                var runPoint = GameState.CurrentSide == CurrentGameState.FriendlySide.Left
                                    ? GameState.GameMode.LeftFountain
                                    : GameState.GameMode.RightFountain;
                                MainWindow.Log(MainWindow.LogType.Combat, "Trying to save my ass for 1 sec");
                                ControlInput.MouseClick(ControlInput.MouseClickButton.Right, runPoint);
                                Thread.Sleep(1000);
                                return RunStatus.Success;
                            }))
                        )
                    );
            }
        }

        public Composite DefaultPushingBehavior
        {
            get
            {
                return defaultPushingBehavior ?? (defaultPushingBehavior =
                    new PrioritySelector("Pushing behaviour",
                        new Decorator("Got any creeps to kill?", c =>
                        {
                            if (!GameState.Me.HeroData.Skills.Any(o => o.ShouldCastOnCreeps)) return false;
                            if (
                                !GameState.EnemyCreepsNearby.Any(
                                    o => o.Distance2D(GameState.Me.HeroScreenPosition) < 500))
                                return false;
                            return true;
                        }, new UseSkillsOnCreeps()),
                        new Sequence("Navigation",
                            new Decorator(c => { return true; },
                                NavRout.Root)
                            , new Action(a =>
                            {
                                if (NavRout.HasNavPoint) return;
                                PushVectorIndex++;
                                if (PushVectorIndex >= CurrentPushVector.Length)
                                    PushVectorIndex = CurrentPushVector.Length - 1;
                                NavRout.SetNewPoint(CurrentPushVector[PushVectorIndex], true);
                            })
                            )
                        ));
            }
        }

        private Composite ChooseCurrentTask
        {
            get
            {
                return _choosePushingTarget ?? (_choosePushingTarget =
                    new Decorator(s =>
                    {
                        if (UpdateInside) ScreenReader.UpdateMinimap(GameState);
                        //ScreenReader.DebugFountainShenaningans(GameState);
                        return ((GameState.Me.HeroMinimapPosition.Distance2D(
                            GameState.CurrentSide == CurrentGameState.FriendlySide.Left
                                ? GameState.GameMode.LeftFountain
                                : GameState.GameMode.RightFountain) < 30) && !NavRout.HasNavPoint) ||
                               CurrentPushVector == null;
                    },
                        new PrioritySelector(
                            new Action(a =>
                            {
                                if (GameState.Me.HeroHP < 90)
                                    return RunStatus.Success;
                                return RunStatus.Failure;
                            }),
                            new Action(a => { return RunStatus.Failure; }),
                            new Decorator(c =>
                            {
                                if (UpdateInside) ScreenReader.UpdateHasTalents(GameState);
                                return GameState.Me.CanLevelTalents;
                            },
                                new LevelTalentsAction()
                                ),
                            new Decorator(c => GameState.Minimap.MapChallengeAvailable,
                                new Action(a => { throw new NotImplementedException(); })),
                            new Decorator(c => GameState.Me.CanCaptureSmallCamp
                                               && GameState.Me.HeroData.SiegeCampLevel >= GameState.Me.CharacterLevel,
                                new Action(a => { throw new NotImplementedException(); })),
                            new Action(a =>
                            {
                                var rand = new Random();
                                PushVectorIndex = 0;
                                switch (rand.Next(1, 4))
                                {
                                    case 1:
                                        MainWindow.Log(MainWindow.LogType.Log,
                                            DateTime.Now.ToShortTimeString() + "Push vector TOP selected.");
                                        CurrentPushVector = GameState.GameMode.TopPushVector;
                                        break;
                                    case 2:
                                        MainWindow.Log(MainWindow.LogType.Log,
                                            DateTime.Now.ToShortTimeString() + "Push vector MID selected.");
                                        CurrentPushVector = GameState.GameMode.MidPushVector;
                                        break;
                                    case 3:
                                    case 4:
                                        MainWindow.Log(MainWindow.LogType.Log,
                                            DateTime.Now.ToShortTimeString() + "Push vector BOT selected.");
                                        CurrentPushVector = GameState.GameMode.BotPushVector;
                                        break;
                                }
                                if (GameState.CurrentSide == CurrentGameState.FriendlySide.Right)
                                {
                                    CurrentPushVector = CurrentPushVector.Reverse().ToArray();
                                }
                                NavRout.SetNewPoint(CurrentPushVector[0], true, false);

                                return RunStatus.Failure;
                            })
                            )))
                    ;
            }
        }

        public static long TickNow(DateTime dta)
        {
            var diff = (DateTime.Now - dta).Milliseconds;
            lastMain = DateTime.Now;
            return diff;
        }
    }
}