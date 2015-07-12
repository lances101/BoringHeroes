using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BoringHeroes.Interaction
{
    public class ControlInput
    {
        public enum MouseClickButton
        {
            Left,
            Right,
            Middle
        }

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        public enum VirtualKeys
        {
            A = 0x41,
            Q = 0x51,
            W = 0x57,
            E = 0x45,
            R = 0x52
        }

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const int VK_F5 = 0x74;
        private const int VK_A = 0x41;
        private const int TalentX = 42;
        private static Process HeroesProcess;
        private static readonly int[] TalentY = {762, 688, 613, 533, 460};

        private static readonly Point[] TalentLevelPoints =
        {
            new Point(73, 835), new Point(128, 840), new Point(186, 835), new Point(238, 838),
            new Point(293, 834), new Point(337, 838), new Point(401, 835)
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy,
            int uFlags);

        public static void SetupHeroesWindow()
        {
            if (HeroesProcess == null)
            {
                var proccess = Process.GetProcesses();
                HeroesProcess = proccess.Where(o => o.ProcessName.Contains("HeroesOfTheStorm")).First();
            }
            SetWindowPos(HeroesProcess.MainWindowHandle, -2, 0, 0, 1024, 768, 0);
        }

        public static void BringHeroesToFront()
        {
            while (MainWindow.IsPaused)
            {
                Thread.Sleep(1000);
            }
            if (HeroesProcess == null)
            {
                var proccess = Process.GetProcesses();
                HeroesProcess = proccess.Where(o => o.ProcessName.Contains("HeroesOfTheStorm")).First();
            }
            SetForegroundWindow(HeroesProcess.MainWindowHandle);
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public static void SendKey(Keys keys)
        {
            PostMessage(HeroesProcess.MainWindowHandle, WM_KEYDOWN, (int) keys, 0);
        }

        public static void MouseClick(MouseClickButton button, Point point)
        {
            MouseClick(button, point.X, point.Y);
        }

        public static void ChooseTalent(int talentLevel, int talentIndex)
        {
            var talentLevelIndex = 0;
            switch (talentLevel)
            {
                case 1:
                    talentLevelIndex = 0;
                    break;
                case 4:
                    talentLevelIndex = 1;
                    break;
                case 7:
                    talentLevelIndex = 2;
                    break;
                case 10:
                    talentLevelIndex = 3;
                    break;
                case 13:
                    talentLevelIndex = 4;
                    break;
                case 16:
                    talentLevelIndex = 5;
                    break;
                case 20:
                    talentLevelIndex = 6;
                    break;
            }
            MouseClick(MouseClickButton.Left, TalentLevelPoints[talentLevelIndex]);
            Thread.Sleep(150);
            MouseClick(MouseClickButton.Left, TalentX, TalentY[talentIndex - 1]);
        }

        public static void MouseClick(MouseClickButton button, int x, int y)
        {
            SetCursorPos(x, y);
            switch (button)
            {
                case MouseClickButton.Left:
                    mouse_event((int) MouseEventFlags.LeftDown, x, y, 0, 0);
                    Thread.Sleep(30);
                    mouse_event((int) MouseEventFlags.LeftUp, x, y, 0, 0);
                    break;

                case MouseClickButton.Middle:

                    break;

                case MouseClickButton.Right:
                    mouse_event((int) MouseEventFlags.RightDown, x, y, 0, 0);
                    Thread.Sleep(30);
                    mouse_event((int) MouseEventFlags.RightUp, x, y, 0, 0);

                    break;
            }
        }
    }
}