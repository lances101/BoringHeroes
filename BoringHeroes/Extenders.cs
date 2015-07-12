using System;
using System.Drawing;

namespace BoringHeroes
{
    public static class Extenders
    {
        public static double Distance2D(this Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static double Distance2D(this AForge.Point one, AForge.Point two)
        {
            return Distance2D(new Point((int) one.X, (int) one.Y), new Point((int) two.X, (int) two.Y));
        }
    }
}