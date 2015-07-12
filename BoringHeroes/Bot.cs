using System;
using System.Threading;
using System.Timers;
using BoringHeroes.GameLogic;
using BoringHeroes.GameLogic.Behaviourism;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.GameLogic.Modes;
using TreeSharp;
using Timer = System.Timers.Timer;

namespace BoringHeroes
{
    internal class Bot
    {
        private int counterCallsPerSecond;
        private CurrentGameState GameState;

        public Bot()
        {
            var ticksTimer = new Timer();
            ticksTimer.Elapsed += ticksTimer_Elapsed;
            ticksTimer.Interval = 1000;
            ticksTimer.Start();
        }

        public int CallsPerSecond { get; private set; }

        private void ticksTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CallsPerSecond = counterCallsPerSecond;
            counterCallsPerSecond = 0;
        }

        public void Start(Hero hero, GameMode mode)
        {
            GameState = new CurrentGameState {Me = {HeroData = hero}, GameMode = mode};
            var thread = new Thread(o => { MainBotLoop(); });
            thread.Start();
        }

        private void MainBotLoop()
        {
            var main = new MainRoutine(GameState);
            ScreenReader.DetectSide(GameState);
            main.Root.Start(GameState);
            
            while (true)
                if (!MainWindow.IsPaused)
                {
                    counterCallsPerSecond++;
                    try
                    {
                        var tack = main.Root.Tick(GameState);
                        if (tack != RunStatus.Running)
                        {
                            main.Root.Stop(GameState);

                            main.Root.Start(GameState);
                        }
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
        }
    }
}