using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BoringHeroes.GameLogic.Behaviourism;
using BoringHeroes.GameLogic.HeroRelated;
using BoringHeroes.GameLogic.Modes;
using BoringHeroes.Interaction;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

//using RazorGDIControlWPF;

namespace BoringHeroes
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum LogType
        {
            Debug,
            Navigation,
            Combat,
            HP,
            Log
        }

        private static string lastText = "";
        public static int counterCallsPerSecond;
        private readonly Bot bot = new Bot();
        private readonly KeyboardHook keyboardHook;
        private readonly List<Thread> threads = new List<Thread>();

        public MainWindow()
        {
            
            var main = new MainRoutine(null);
            TreeDebug td = new TreeDebug();
            td.AnalyzeComposite(main.Root, null);
            td.ShowDialog();
            InitializeComponent();
            Instance = this;
            keyboardHook = new KeyboardHook();
            keyboardHook.RegisterHotKey(0, Keys.F1);
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            IsPaused = false;
            var myTimer = new Timer();
            myTimer.Elapsed += DisplayTimeEvent;
            myTimer.Interval = 1000; // 1000 ms is one second
            myTimer.Start();
            ControlInput.SetupHeroesWindow();
        }

        public static MainWindow Instance { get; set; }
        public static bool IsPaused { get; set; }

        public void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            Instance.Dispatcher.Invoke((MethodInvoker) delegate
            {
                try
                {
                    lblStatRunsPerSecond.Content = bot.CallsPerSecond + " runs/s";
                    counterCallsPerSecond = 0;
                }
                catch
                {
                }
            },
                DispatcherPriority.Background);
        }

        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            Button_Click_1(null, null);
        }

        public static void DebugReaderStats(string text)
        {
            if (text == "") return;
            Instance.Dispatcher.Invoke((MethodInvoker) delegate
            {
                var type = text.Split(':')[0];
                switch (type)
                {
                    case "HeroHP":
                        Instance.lblStatHeroHP.Content = text;
                        break;
                    case "Heroes":
                        Instance.lblStatHeroes.Content = text;
                        break;
                    case "Units":
                        Instance.lblStatUnits.Content = text;
                        break;
                    case "Towers":
                        Instance.lblStatTowers.Content = text;
                        break;
                    case "Minimap":
                        Instance.lblStatMinimap.Content = text;
                        break;
                    case "Total":
                        Instance.lblStatTotal.Content = text;
                        break;
                    case "Screenshot":
                        Instance.lblStatScreenRead.Content = text;
                        break;
                }
                Instance.rtbDebug.AppendText(text + "\r");
                Instance.rtbDebug.ScrollToEnd();
            },
                DispatcherPriority.Background);
        }

        public static void SetHp(double value)
        {
            Instance.Dispatcher.Invoke((MethodInvoker) delegate
            {
                if (value > 100) value = 100;
                Instance.HPBar.Value = value;
            },
                DispatcherPriority.Background);
        }

        public static void Log(string text)
        {
            Log(LogType.Log, text);
        }

        public static void Log(LogType logType, string text)
        {
            if (text == "") return;
            Instance.Dispatcher.Invoke((MethodInvoker) delegate
            {
                switch (logType)
                {
                    case LogType.Log:
                        Instance.rtbLog.AppendText(text);
                        Instance.rtbLog.ScrollToEnd();
                        break;

                    case LogType.Debug:
                        Instance.rtbDebug.AppendText(text);
                        Instance.rtbDebug.ScrollToEnd();
                        break;
                    case LogType.Combat:
                        Instance.rtbCombatLog.AppendText(text);
                        Instance.rtbCombatLog.ScrollToEnd();
                        break;
                    case LogType.Navigation:
                        Instance.rtbNavigationLog.AppendText(text);
                        Instance.rtbNavigationLog.ScrollToEnd();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("logType");
                }
            },
                DispatcherPriority.Background);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            bot.Start(Heroes.Valla, GameModes.BlackHeart);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var t in threads)
            {
                t.Abort();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsPaused)
            {
                IsPaused = false;
                btnPause.Content = "Pause";
            }
            else
            {
                IsPaused = true;
                btnPause.Content = "Go";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}