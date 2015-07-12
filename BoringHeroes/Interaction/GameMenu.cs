using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.GameLogic.Modes;

namespace BoringHeroes.Interaction
{
    internal class GameMenu
    {
        public static bool AreWeInMainMenu()
        {
            if (ScreenReader.FindOnScreen("./images/loading_mainmenu.png", 0.90f, true,
                new Rectangle(50, 200, 150, 100)) != null)
            {
                return true;
            }
            return false;
        }

        public static bool AreWeInPlayMenu()
        {
            if (ScreenReader.FindOnScreen("./images/loading_playmenu.png", 0.90f, true,
                new Rectangle(150, 0, 200, 50)) != null)
            {
                return true;
            }
            return false;
        }

        public static void StartSearchingPractice(Hero hero)
        {
            MainWindow.Log("Starting practice game");
            if (AreWeInMainMenu())
            {
                MainWindow.Log("We are in main menu");
                ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 403, 1053);
                Thread.Sleep(1000);
            }
            if (!AreWeInPlayMenu()) throw new KeyNotFoundException();

            ChooseGameMode("practice");
            Thread.Sleep(1000);
            ChooseCharacter(hero);
            Thread.Sleep(1000);
            ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 848, 944);
            Thread.Sleep(1000);
            while (true)
            {
                Thread.Sleep(1000);
                if (ScreenReader.FindOnScreen("./images/loading_searching.png", 0.9f, true) == null)
                    break;
            }
        }

        public static GameMode FindGameMode()
        {
            while (true)
            {
                MainWindow.Log("GAMEMODE LOOP" + DateTime.Now);
                Thread.Sleep(4000);
                foreach (var mode in GameModes.ModesList)
                {
                    var match = ScreenReader.FindOnScreen(mode.LoadingMatchPath, 0.90f, true,
                        new Rectangle(30, 20, 170, 40));
                    if (match != null)
                    {
                        MainWindow.Log("Match for " + mode.Name + " was " + match.Similarity);
                        return mode;
                    }
                }

                MainWindow.Log("GAMEMODE LOOP" + DateTime.Now);
            }
        }

        private static void ChooseCharacter(Hero hero)
        {
            MainWindow.Log("Choosing hero - " + hero.Name);
            ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 958, 97);
            Thread.Sleep(1000);
            ControlInput.MouseClick(ControlInput.MouseClickButton.Left, hero.ChooseX, hero.ChooseY);
        }

        private static void ChooseGameMode(string mode)
        {
            MainWindow.Log("Choosing mode - " + mode);
            ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 1074, 944);
            switch (mode)
            {
                case "practice":
                    ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 383, 597);
                    Thread.Sleep(100);
                    ControlInput.MouseClick(ControlInput.MouseClickButton.Left, 871, 788);
                    Thread.Sleep(100);
                    break;
            }
        }
    }
}