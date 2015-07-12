using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using BoringHeroes.GameLogic;
using BoringHeroes.Interaction;
using Point = AForge.Point;

namespace BoringHeroes
{
    public static class ScreenReader
    {
        public enum HeroScanType
        {
            All,
            Friendly,
            Enemy
        }

        private static readonly Rectangle gameScreen = new Rectangle(0, 0, 1920, 1080);
        public static bool ShouldBringToFront = true;

        private static readonly Bitmap templateMe =
            ChangePixelFormat(new Bitmap("./images/ingame_heroes_me_healthbar.png"),
                PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateEnemy =
            ChangePixelFormat(new Bitmap("./images/ingame_heroes_hostile_healthbar.png"), PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateFriend =
            ChangePixelFormat(new Bitmap("./images/ingame_heroes_friendly_healthbar.png"), PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTower = ChangePixelFormat(new Bitmap("./images/healthbar_tower.png"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateMounted = ChangePixelFormat(new Bitmap("./images/heromounted.png"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateHasTalents = ChangePixelFormat(new Bitmap("./images/hasTalents.png"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents20 = ChangePixelFormat(new Bitmap("./images/talents20.bmp"),
            PixelFormat.Format24bppRgb);

        private static Bitmap templateTalents1 = ChangePixelFormat(new Bitmap("./images/talents01.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents4 = ChangePixelFormat(new Bitmap("./images/talents04.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents7 = ChangePixelFormat(new Bitmap("./images/talents07.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents10 = ChangePixelFormat(new Bitmap("./images/talents10.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents13 = ChangePixelFormat(new Bitmap("./images/talents13.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateTalents16 = ChangePixelFormat(new Bitmap("./images/talents16.bmp"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateCampSmall = ChangePixelFormat(new Bitmap("./images/camp_small.png"),
            PixelFormat.Format24bppRgb);

        private static readonly Bitmap templateCampBig = ChangePixelFormat(new Bitmap("./images/camp_big.png"),
            PixelFormat.Format24bppRgb);

        private static Bitmap templateCampPirate = ChangePixelFormat(new Bitmap("./images/camp_pirate.png"),
            PixelFormat.Format24bppRgb);

        private static readonly List<OnScreenHero>[] heroPoints =
        {
            new List<OnScreenHero>(), new List<OnScreenHero>(),
            new List<OnScreenHero>()
        };

        private static DateTime dt = DateTime.Now;

        private static Bitmap GrabScreenGDI(Rectangle rect)
        {
            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(bmp);
            try
            {
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
            }
            catch (Exception e)
            {
            }
            graphics.Dispose();

            return bmp;
        }

        public static TemplateMatch FindOnScreen(string path)
        {
            return FindOnScreen(path, 0.9f);
        }

        public static TemplateMatch FindOnScreen(string path, float percentage)
        {
            return FindOnScreen(path, percentage, false, new Rectangle());
        }

        public static TemplateMatch FindOnScreen(string path, float percentage, bool shouldResize)
        {
            return FindOnScreen(path, percentage, shouldResize, new Rectangle());
        }

        public static TemplateMatch FindOnScreen(string path, float percentage, bool shouldResize, Rectangle cropRect)
        {
            Console.Beep();
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var template = ChangePixelFormat(new Bitmap(path), PixelFormat.Format24bppRgb);
            var src = GrabScreenGDI(gameScreen);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
            src.Dispose();
            if (shouldResize)
            {
                var rb = new ResizeBilinear(sourceImage.Width/4, sourceImage.Height/4);
                sourceImage = rb.Apply(sourceImage);
            }
            //sourceImage.Save("./images/resized.png");
            var ed = new HomogenityEdgeDetector();

            if (!cropRect.Size.IsEmpty)
            {
                var cb = new Crop(cropRect);
                sourceImage = cb.Apply(sourceImage);
                //sourceImage.Save("./images/resized_cropped.png");
            }

            //ed.Apply(Grayscale.CommonAlgorithms.BT709.Apply(sourceImage)).Save("./images/EdgeDetection.png");

            var tm = new ExhaustiveTemplateMatching(percentage);
            var matchings = tm.ProcessImage(sourceImage, template);
            var data = sourceImage.LockBits(
                new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                ImageLockMode.ReadWrite, sourceImage.PixelFormat);

            TemplateMatch topMatch = null;
            foreach (var m in matchings)
            {
                if (topMatch == null) topMatch = m;
                if (m.Similarity > topMatch.Similarity) topMatch = m;
                var col = Color.Red;
                col = Color.FromArgb((byte) ((int) (m.Similarity*255)), col);
                Drawing.Rectangle(data, m.Rectangle, col);
            }
            sourceImage.UnlockBits(data);
            //sourceImage.Save("./images/res.png");
            sourceImage.Dispose();
            template.Dispose();
            GC.Collect();
            return topMatch;
        }

        public static long TickNow()
        {
            var diff = (DateTime.Now - dt).Milliseconds;
            dt = DateTime.Now;
            return diff;
        }

        private static bool InRange(float val, float bot, float top)
        {
            return bot < val && val <= top;
        }

        private static Point? FindCamera(Point p, List<Blob> blobs)
        {
            var diff = 2;
            var antidiff = diff*-1;
            foreach (var blob in blobs)
            {
                if (p.DistanceTo(blob.CenterOfGravity) < 1) continue;
                if (InRange(((int) (blob.CenterOfGravity.Y - p.Y)), antidiff, diff))
                {
                    foreach (var blob2 in blobs)
                    {
                        if (blob.CenterOfGravity.DistanceTo(blob2.CenterOfGravity) < 1) continue;
                        if (InRange(((int) (blob2.CenterOfGravity.X - blob.CenterOfGravity.X)), antidiff, diff))
                        {
                            foreach (var blob3 in blobs)
                            {
                                if (blob2.CenterOfGravity.DistanceTo(blob3.CenterOfGravity) < 1) continue;
                                if (InRange(((int) (blob3.CenterOfGravity.Y - blob2.CenterOfGravity.Y)), antidiff, diff))
                                {
                                    if (InRange(((int) (blob3.CenterOfGravity.X - p.X)), antidiff, diff))
                                    {
                                        float xCent = 0, yCent = 0;
                                        var sum = p + blob.CenterOfGravity + blob2.CenterOfGravity +
                                                  blob3.CenterOfGravity;
                                        xCent = sum.X/4;
                                        yCent = sum.Y/4;
                                        return new Point(xCent, yCent);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static System.Drawing.Point[] ToPointsArray(List<IntPoint> corners)
        {
            var points = new System.Drawing.Point[corners.Count];
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new System.Drawing.Point(corners[i].X, corners[i].Y);
            }
            return points;
        }

        public static float TestTemplateOnScreen(string path, float percentage)
        {
            var template = ChangePixelFormat(new Bitmap(path), PixelFormat.Format24bppRgb);
            var sourceImage = ChangePixelFormat(GrabScreenGDI(gameScreen), PixelFormat.Format24bppRgb);
            var rb = new ResizeBilinear(sourceImage.Width/4, sourceImage.Height/4);
            sourceImage = rb.Apply(sourceImage);
            var tm = new ExhaustiveTemplateMatching(percentage);
            var matchings = tm.ProcessImage(sourceImage, template);
            var data = sourceImage.LockBits(
                new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                ImageLockMode.ReadWrite, sourceImage.PixelFormat);

            MainWindow.Log("%" + percentage + " got " + matchings.Length + " matches");
            if (matchings.Length != 1)
            {
                if (matchings.Length == 0)
                {
                    percentage -= 0.005f;
                }
                if (matchings.Length > 1)
                {
                    percentage += 0.0025f;
                }
                percentage = TestTemplateOnScreen(path, percentage);
            }
            foreach (var m in matchings)
            {
                Drawing.Rectangle(data, m.Rectangle, Color.LimeGreen);
            }
            sourceImage.UnlockBits(data);
            //sourceImage.Save("./images/restest.png");
            return percentage;
        }

        private static Bitmap ChangePixelFormat(Bitmap inputImage, PixelFormat newFormat)
        {
            return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
        }

        public static void DetectSide(CurrentGameState state)
        {
            while (state.CurrentSide == CurrentGameState.FriendlySide.Undecided)
            {
                if (FindOnScreen("./images/ingame_blackheart_red_crown.png", 0.91f, false,
                    new Rectangle(730, 540, 150, 300)) != null)
                {
                    state.CurrentSide = CurrentGameState.FriendlySide.Right;
                }
                else if (FindOnScreen("./images/ingame_blackheart_red_crown.png", 0.91f, false,
                    new Rectangle(880, 540, 150, 300)) != null)
                {
                    state.CurrentSide = CurrentGameState.FriendlySide.Left;
                }
            }
        }

        public static void UpdateEverythingOptimized(CurrentGameState state)
        {
            Bitmap src;
            Bitmap sourceImage;
            Bitmap croppedImage;
            Bitmap grayScale;
            Bitmap thresholded;
            ControlInput.SendKey(Keys.Space);
            ShouldBringToFront = false;
            while (true)
            {
                #region HPCheck

                try
                {
                    ControlInput.BringHeroesToFront();
                    src = GrabScreenGDI(gameScreen);
                    sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
                    Tools.Cropper.Rectangle = new Rectangle(200, 970, 265, 30);
                    croppedImage = Tools.Cropper.Apply(sourceImage);
                    Tools.ColorFilter.Red = new IntRange(50, 255);
                    Tools.ColorFilter.Green = new IntRange(50, 255);
                    Tools.ColorFilter.Blue = new IntRange(0, 50);
                    Tools.ColorFilter.ApplyInPlace(croppedImage);
                    grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                    Tools.Thresholder.ThresholdValue = 1;
                    Tools.Thresholder.ApplyInPlace(grayScale);
                    thresholded = Tools.GSToRgb.Apply(grayScale);

                    var bc = new BlobCounter();
                    bc.ProcessImage(thresholded);
                    var blobs = bc.GetObjectsInformation().ToList();
                    state.Me.HeroHP = 0;
                    try
                    {
                        blobs =
                            blobs.Where(o => o.Rectangle.Width > 1 && o.Rectangle.Height > 5)
                                .OrderBy(o => o.Rectangle.Width)
                                .ToList();
                        var i = 0;
                        foreach (var blob in blobs)
                        {
                            state.Me.HeroHP = (blob.Rectangle.Width/220f*100f);
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    #endregion

                    if (state.Me.HeroHP <= 0) break;

                    #region UnitCheck

                    var unitPoints = new List<System.Drawing.Point>();
                    try
                    {
                        if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                        Tools.Cropper.Rectangle = new Rectangle(0, 100, 1920, 740);
                        croppedImage = Tools.Cropper.Apply(sourceImage);
                        Tools.HSLFiltering.Hue = new IntRange(0, 3);
                        Tools.HSLFiltering.Saturation = new Range(0.8f, 1);
                        Tools.HSLFiltering.Luminance = new Range(0, 0.4f);
                        Tools.HSLFiltering.ApplyInPlace(croppedImage);
                        grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                        Tools.Thresholder.ThresholdValue = 1;
                        Tools.Thresholder.ApplyInPlace(grayScale);
                        Tools.HLengthSmoothing.MaxGapSize = 2;
                        Tools.VLengthSmoothing.MaxGapSize = 2;
                        Tools.HLengthSmoothing.ApplyInPlace(grayScale);
                        Tools.VLengthSmoothing.ApplyInPlace(grayScale);
                        thresholded = Tools.GSToRgb.Apply(grayScale);
                        bc.ProcessImage(thresholded);
                        blobs = bc.GetObjectsInformation().ToList();

                        blobs =
                            blobs.Where(
                                o =>
                                    !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) &&
                                    o.Rectangle.Width > 8 &&
                                    o.Rectangle.Width < 50 && o.Rectangle.Height >= 2 && o.Rectangle.Height < 4)
                                .ToList();
                        var i = 0;
                        foreach (var blob in blobs)
                        {
                            unitPoints.Add(new System.Drawing.Point((int) (blob.CenterOfGravity.X),
                                (int) (100 + blob.CenterOfGravity.Y)));
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    state.EnemyCreepsNearby = unitPoints.ToArray();

                    unitPoints.Clear();
                    if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                    try
                    {
                        Tools.Cropper.Rectangle = new Rectangle(0, 100, 1920, 740);
                        croppedImage = Tools.Cropper.Apply(sourceImage);
                        Tools.HSLFiltering.Hue = new IntRange(113, 122);
                        Tools.HSLFiltering.Saturation = new Range(0.8f, 1);
                        Tools.HSLFiltering.Luminance = new Range(0, 0.4f);
                        Tools.HSLFiltering.ApplyInPlace(croppedImage);

                        grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                        Tools.Thresholder.ThresholdValue = 1;
                        Tools.Thresholder.ApplyInPlace(grayScale);
                        Tools.HLengthSmoothing.MaxGapSize = 2;
                        Tools.VLengthSmoothing.MaxGapSize = 2;
                        Tools.HLengthSmoothing.ApplyInPlace(grayScale);
                        Tools.VLengthSmoothing.ApplyInPlace(grayScale);
                        thresholded = Tools.GSToRgb.Apply(grayScale);

                        bc = new BlobCounter();
                        bc.ProcessImage(thresholded);
                        //thresholded.Save("./images/unitdebug.png");
                        blobs = bc.GetObjectsInformation().ToList();

                        blobs =
                            blobs.Where(
                                o =>
                                    !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) &&
                                    o.Rectangle.Width > 8 &&
                                    o.Rectangle.Width < 50 && o.Rectangle.Height >= 2 && o.Rectangle.Height < 4)
                                .ToList();
                        var i = 0;
                        foreach (var blob in blobs)
                        {
                            unitPoints.Add(new System.Drawing.Point((int) (blob.CenterOfGravity.X - 12),
                                (int) (100 + blob.CenterOfGravity.Y)));
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    state.FriendlyCreepsNearby = unitPoints.ToArray();

                    #endregion

                    #region TowerCheck

                    unitPoints = new List<System.Drawing.Point>();
                    try
                    {
                        //TickNow("unitSearchStart", dt);
                        if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                        Tools.Cropper.Rectangle = new Rectangle(0, 100, 1920, 540);
                        croppedImage = Tools.Cropper.Apply(sourceImage);
                        Tools.HSLFiltering.Hue = new IntRange(36, 40);
                        Tools.HSLFiltering.Saturation = new Range(0.3f, 0.5f);
                        Tools.HSLFiltering.Luminance = new Range(0.3f, 0.55f);
                        Tools.HSLFiltering.ApplyInPlace(croppedImage);
                        grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                        Tools.Thresholder.ThresholdValue = 1;
                        Tools.Thresholder.ApplyInPlace(grayScale);
                        Tools.HLengthSmoothing.MaxGapSize = 6;
                        Tools.HLengthSmoothing.ApplyInPlace(grayScale);
                        thresholded = Tools.GSToRgb.Apply(grayScale);


                        bc.ProcessImage(thresholded);
                        blobs = bc.GetObjectsInformation().ToList();

                        blobs =
                            blobs.Where(
                                o =>
                                    !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) &&
                                    o.Rectangle.Width > 20 &&
                                    o.Rectangle.Width < 180 && o.Rectangle.Height > 6 && o.Rectangle.Height < 16)
                                .ToList();
                        var i = 0;
                        foreach (var blob in blobs)
                        {
                            var rc = new Rectangle(blob.Rectangle.X, blob.Rectangle.Y + 80, blob.Rectangle.Width,
                                blob.Rectangle.Height + 20);

                            Tools.Cropper.Rectangle = rc;
                            var cut = Tools.Cropper.Apply(sourceImage);
                            var res = Tools.TemplateMatcher.ProcessImage(cut, templateTower);
                            foreach (var r in res)
                            {
                                unitPoints.Add(new System.Drawing.Point(rc.X - 12, 100 + rc.Y + 350));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    state.TowersFound = unitPoints.ToArray();

                    #endregion

                    #region Hero Check

                    heroPoints[0].Clear();
                    heroPoints[1].Clear();
                    try
                    {
                        heroPoints[2].Clear();
                        if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                        Tools.Cropper.Rectangle = new Rectangle(0, 100, 1920, 740);
                        croppedImage = Tools.Cropper.Apply(sourceImage);

                        grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                        Tools.Thresholder.ThresholdValue = 128;
                        Tools.Thresholder.ApplyInPlace(grayScale);

                        thresholded = Tools.GSToRgb.Apply(grayScale);
                        Tools.TemplateMatcher.SimilarityThreshold = 0.9f;


                        bc.ProcessImage(thresholded);
                        blobs = bc.GetObjectsInformation().ToList();
                        blobs =
                            blobs.Where(
                                o =>
                                    o.Rectangle.Width > 115 && o.Rectangle.Width < 130 && o.Rectangle.Height < 20 &&
                                    o.Rectangle.Height > 10).ToList();
                        var i = 0;
                        foreach (var blob in blobs)
                        {
                            var rect = blob.Rectangle;
                            rect.Offset(0, 100);
                            Tools.Cropper.Rectangle = blob.Rectangle;
                            var matches1 = Tools.TemplateMatcher.ProcessImage(Tools.Cropper.Apply(croppedImage),
                                templateMe);

                            if (matches1.Length > 1)
                            {
                                state.Me.HeroScreenPosition = new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                    (int) blob.CenterOfGravity.Y + 250);
                                //state.Me.HeroHP = (float) (matches1.Length/120.0*100.0);
                                heroPoints[0].Add(
                                    new OnScreenHero(
                                        new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                            (int) blob.CenterOfGravity.Y + 200),
                                        matches1.Length));
                                continue;
                            }
                            var matches2 = Tools.TemplateMatcher.ProcessImage(Tools.Cropper.Apply(croppedImage),
                                templateEnemy);

                            if (matches2.Length > 1)
                            {
                                heroPoints[1].Add(
                                    new OnScreenHero(
                                        new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                            (int) blob.CenterOfGravity.Y + 200),
                                        matches2.Length));
                                continue;
                            }
                            var matches3 = Tools.TemplateMatcher.ProcessImage(Tools.Cropper.Apply(croppedImage),
                                templateFriend);
                            if (matches3.Length > 1)
                            {
                                heroPoints[2].Add(
                                    new OnScreenHero(
                                        new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                            (int) blob.CenterOfGravity.Y + 200),
                                        matches3.Length));
                            }
                        }
                        state.FriendlyHeroes = heroPoints[2].ToArray();
                        state.EnemyHeroes = heroPoints[1].ToArray();
                    }
                    catch (Exception e)
                    {
                    }

                    #endregion

                    #region UpdateMinimap

                    var debugPoints = new List<System.Drawing.Point>();
                    try
                    {
                        Tools.Cropper.Rectangle = state.GameMode.MinimapRectangle;
                        Tools.ColorFilter.Red = new IntRange(180, 255);
                        Tools.ColorFilter.Green = new IntRange(180, 255);
                        Tools.ColorFilter.Blue = new IntRange(180, 255);
                        croppedImage = Tools.Cropper.Apply(sourceImage);
                        Tools.ColorFilter.ApplyInPlace(croppedImage);
                        croppedImage.Save("./debug/hsl.png");
                        grayScale = Grayscale.CommonAlgorithms.BT709.Apply(croppedImage);
                        Tools.Thresholder.ThresholdValue = 1;
                        Tools.Thresholder.ApplyInPlace(grayScale);
                        thresholded = Tools.GSToRgb.Apply(grayScale);

                        bc.ProcessImage(thresholded);
                        blobs = bc.GetObjectsInformation().ToList();

                        blobs = blobs.Where(o => o.Rectangle.Width > 4 && o.Rectangle.Width < 11
                                                 && o.Rectangle.Height > 4 && o.Rectangle.Height < 11).ToList();
                        Point? cam = null;
                        Point[] cams = {new Point()};
                        for (var j = 0; j < blobs.Count; j++)
                        {
                            cam = FindCamera(blobs[j].CenterOfGravity, blobs);
                            if (cam != null)
                            {
                                break;
                            }
                        }

                        foreach (var c in cams)
                        {
                            debugPoints.Add(new System.Drawing.Point((int) c.X, (int) c.Y));
                        }
                        if (cam.HasValue)
                            state.Me.HeroMinimapPosition = new System.Drawing.Point(
                                (int) (cam.Value.X + state.GameMode.MinimapRectangle.X),
                                (int) (cam.Value.Y + state.GameMode.MinimapRectangle.Y));
                    }
                    catch (Exception e)
                    {
                    }

                    #endregion

                    if (!state.IsPlayerSafe) break;

                    #region Check Mounted

                    if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                    try
                    {
                        Tools.Cropper.Rectangle = new Rectangle(0, 700, 70, 250);
                        croppedImage = Tools.Cropper.Apply(sourceImage);

                        Tools.TemplateMatcher.SimilarityThreshold = 0.9f;


                        var matches1 = Tools.TemplateMatcher.ProcessImage(croppedImage, templateMounted);

                        if (matches1.Length > 0)
                        {
                            state.Me.IsMounted = true;
                        }
                        state.Me.IsMounted = false;
                    }
                    catch (Exception e)
                    {
                    }

                    #endregion

                    #region Has talents?

                    var canLevel = false;
                    try
                    {
                        if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                        Tools.Cropper.Rectangle = new Rectangle(70, 1020, 105, 30);
                        croppedImage = Tools.Cropper.Apply(sourceImage);

                        Tools.TemplateMatcher.SimilarityThreshold = 0.9f;

                        var matches1 = Tools.TemplateMatcher.ProcessImage(croppedImage, templateHasTalents);

                        if (matches1.Length > 0)
                        {
                            canLevel = true;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    state.Me.CanLevelTalents = canLevel;

                    #endregion

                    if (!state.Me.CanLevelTalents) break;

                    #region Talent Scan

                    /*
                    try
                    {
                        Tools.Cropper.Rectangle = new Rectangle(48, 800, 390, 70);
                        croppedImage = Tools.Cropper.Apply(sourceImage);
                        state.Me.CharacterLevel = 1;
                        var patternFinder = new ExhaustiveTemplateMatching(0.9f);

                        var matches = patternFinder.ProcessImage(croppedImage, templateTalents4);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 4;
                            return;
                        }
                        matches = patternFinder.ProcessImage(croppedImage, templateTalents7);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 7;
                            return;
                        }
                        matches = patternFinder.ProcessImage(croppedImage, templateTalents10);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 10;
                            return;
                        }
                        matches = patternFinder.ProcessImage(croppedImage, templateTalents13);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 13;
                            return;
                        }
                        matches = patternFinder.ProcessImage(croppedImage, templateTalents16);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 16;
                            return;
                        }
                        matches = patternFinder.ProcessImage(croppedImage, templateTalents20);
                        if (matches.Length == 1)
                        {
                            state.Me.CharacterLevel = 20;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                    */

                    #endregion
                }
                catch (Exception e)
                {
                }
                break;
            }

            #region Dispose EVERYTHING

            #endregion
        }

        public static void UpdateMinimap(CurrentGameState state)
        {
            TickNow();
            // if (heroPoints[0].Count == 0) return;
            var src = GrabScreenGDI(state.GameMode.MinimapRectangle);
            //var thresholded = ThresholdCropBitmap(src, new Rectangle(1450, 775, 450, 350), 200);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
            src.Dispose();
            Tools.ColorFilter.Red = new IntRange(180, 255);
            Tools.ColorFilter.Green = new IntRange(180, 255);
            Tools.ColorFilter.Blue = new IntRange(180, 255);
            Tools.ColorFilter.ApplyInPlace(sourceImage);
            var grayScale = Grayscale.CommonAlgorithms.BT709.Apply(sourceImage);
            Tools.Thresholder.ThresholdValue = 1;
            Tools.Thresholder.ApplyInPlace(grayScale);
            var thresholded = Tools.GSToRgb.Apply(grayScale);
            var bc = new BlobCounter();
            bc.ProcessImage(thresholded);
            var blobs = bc.GetObjectsInformation().ToList();
            var debugPoints = new List<System.Drawing.Point>();
            try
            {
                blobs = blobs.Where(o => o.Rectangle.Width > 3 && o.Rectangle.Width < 7
                                         && o.Rectangle.Height > 3 && o.Rectangle.Height < 7).ToList();
                Point? cam = null;
                Point[] cams = {new Point()};
                for (var j = 0; j < blobs.Count; j++)
                {
                    cam = FindCamera(blobs[j].CenterOfGravity, blobs);
                    if (cam != null)
                    {
                        break;
                    }
                }

                if (cam.HasValue)
                    state.Me.HeroMinimapPosition =
                        new System.Drawing.Point((int) (cam.Value.X + state.GameMode.MinimapRectangle.X),
                            (int) (cam.Value.Y + state.GameMode.MinimapRectangle.Y));
            }
            catch (Exception e)
            {
            }
            src.Dispose();
            sourceImage.Dispose();
            MainWindow.DebugReaderStats("Minimap:" + TickNow());
            //DebugFountainShenaningans(state, debugPoints);
        }

        public static void UpdateUnits(CurrentGameState state)
        {
            TickNow();
            var unitPoints = new List<System.Drawing.Point>();
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(new Rectangle(0, 100, 1024, 400));
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            if (false)
            {
                Tools.HSLFiltering.Hue = new IntRange(358, 5);
                Tools.HSLFiltering.Saturation = new Range(0.3f, 1);
                Tools.HSLFiltering.Luminance = new Range(0, 0.4f);
                Tools.HSLFiltering.ApplyInPlace(sourceImage);
            }
            Tools.ColorFilter.Red = new IntRange(140, 180);
            Tools.ColorFilter.Green = new IntRange(0, 50);
            Tools.ColorFilter.Blue = new IntRange(0, 50);
            Tools.ColorFilter.ApplyInPlace(sourceImage);

            var grayScale = Grayscale.CommonAlgorithms.BT709.Apply(sourceImage);
            /*Tools.Thresholder.ThresholdValue = 1;
            Tools.Thresholder.ApplyInPlace(grayScale);
            Tools.HLengthSmoothing.MaxGapSize = 2;
            Tools.VLengthSmoothing.MaxGapSize = 2;
            Tools.HLengthSmoothing.ApplyInPlace(grayScale);
            Tools.VLengthSmoothing.ApplyInPlace(grayScale);*/
            var thresholded = Tools.GSToRgb.Apply(grayScale);
            var bc = new BlobCounter();
            bc.ProcessImage(thresholded);
            var blobs = bc.GetObjectsInformation().ToList();
            var gc = Graphics.FromImage(sourceImage);
            try
            {
                blobs =
                    blobs.Where(
                        o =>
                            !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) && o.Rectangle.Width > 4 &&
                            o.Rectangle.Width < 35 && o.Rectangle.Height == 1).ToList();
                var i = 0;
                foreach (var blob in blobs)
                {
                    unitPoints.Add(new System.Drawing.Point((int) (blob.CenterOfGravity.X),
                        (int) (100 + blob.CenterOfGravity.Y)));
                    //gc.DrawEllipse(new Pen(Color.White), blob.Rectangle.X - 4, blob.Rectangle.Y - 4, 8, 8 );
                }
            }
            catch (Exception e)
            {
            }

            //gc.Flush();
            state.EnemyCreepsNearby = unitPoints.ToArray();
            //sourceImage.Save("./debug/go.png");

            unitPoints.Clear();

            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            try
            {
                sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
                if (false)
                {
                    Tools.HSLFiltering.Hue = new IntRange(112, 128);
                    Tools.HSLFiltering.Saturation = new Range(0.8f, 1);
                    Tools.HSLFiltering.Luminance = new Range(0, 0.4f);
                    Tools.HSLFiltering.ApplyInPlace(sourceImage);
                }
                Tools.ColorFilter.Red = new IntRange(0, 50);
                Tools.ColorFilter.Green = new IntRange(120, 160);
                Tools.ColorFilter.Blue = new IntRange(0, 50);
                Tools.ColorFilter.ApplyInPlace(sourceImage);
                grayScale = Grayscale.CommonAlgorithms.BT709.Apply(sourceImage);
                /*Tools.Thresholder.ThresholdValue = 1;
                Tools.Thresholder.ApplyInPlace(grayScale);
                Tools.HLengthSmoothing.MaxGapSize = 2;
                Tools.VLengthSmoothing.MaxGapSize = 2;
                Tools.HLengthSmoothing.ApplyInPlace(grayScale);
                Tools.VLengthSmoothing.ApplyInPlace(grayScale);*/
                thresholded = Tools.GSToRgb.Apply(grayScale);

                bc = new BlobCounter();
                bc.ProcessImage(thresholded);
                //thresholded.Save("./images/unitdebug.png");
                blobs = bc.GetObjectsInformation().ToList();

                blobs =
                    blobs.Where(
                        o =>
                            !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) && o.Rectangle.Width > 4 &&
                            o.Rectangle.Width < 35 && o.Rectangle.Height >= 2 && o.Rectangle.Height < 4).ToList();
                var i = 0;
                foreach (var blob in blobs)
                {
                    unitPoints.Add(new System.Drawing.Point((int) (blob.CenterOfGravity.X - 12),
                        (int) (100 + blob.CenterOfGravity.Y)));
                }
            }
            catch (Exception e)
            {
            }
            state.FriendlyCreepsNearby = unitPoints.ToArray();
            MainWindow.DebugReaderStats("Units:" + TickNow());
        }

        public static bool UpdateHeroMounted(CurrentGameState state)
        {
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            Bitmap src;
            Bitmap sourceImage;
            Bitmap croppedImage;

            try
            {
                src = GrabScreenGDI(gameScreen);
                sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

                Tools.Cropper.Rectangle = new Rectangle(0, 700, 70, 250);
                croppedImage = Tools.Cropper.Apply(sourceImage);

                var patternFinder = new ExhaustiveTemplateMatching(0.9f);

                var matches1 = patternFinder.ProcessImage(croppedImage, templateMounted);

                if (matches1.Length > 0)
                {
                    state.Me.IsMounted = true;
                }
                state.Me.IsMounted = false;
                src.Dispose();
                sourceImage.Dispose();
                croppedImage.Dispose();
            }
            catch (Exception e)
            {
            }
            finally
            {
            }
            return false;
        }

        public static void UpdateHeroes(CurrentGameState state) // beta
        {
            TickNow();
            heroPoints[0].Clear();
            heroPoints[1].Clear();
            heroPoints[2].Clear();
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(new Rectangle(0, 78, 1024, 550));
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            var grayScale = Grayscale.CommonAlgorithms.BT709.Apply(sourceImage);
            Tools.Thresholder.ThresholdValue = 128;
            Tools.Thresholder.ApplyInPlace(grayScale);

            var thresholded = Tools.GSToRgb.Apply(grayScale);

            var bc = new BlobCounter();
            bc.ProcessImage(thresholded);
            var blobs = bc.GetObjectsInformation().ToList();
            try
            {
                blobs =
                    blobs.Where(
                        o =>
                            o.Rectangle.Width > 80 && o.Rectangle.Width < 90 && o.Rectangle.Height < 20 &&
                            o.Rectangle.Height > 10).ToList();
                var i = 0;
                Bitmap bitmap = null;
                foreach (var blob in blobs)
                {
                    Tools.Cropper.Rectangle = new Rectangle(blob.Rectangle.X + 1,
                        blob.Rectangle.Y + 3,
                        blob.Rectangle.Width - 1,
                        4
                        );
                    var cropped = Tools.Cropper.Apply(src);
                    var imstat = new ImageStatistics(cropped);
                    //Console.WriteLine("R {0} G {1}", imstat.RedWithoutBlack.Median, imstat.GreenWithoutBlack.Median);
                    if (Math.Abs(imstat.RedWithoutBlack.Median - imstat.GreenWithoutBlack.Median) < 4)
                    {
                        state.Me.HeroScreenPosition = new System.Drawing.Point((int) blob.CenterOfGravity.X,
                            (int) blob.CenterOfGravity.Y + 150);
                        //state.Me.HeroHP = (float) (matches1.Length/120.0*100.0);
                        heroPoints[0].Add(
                            new OnScreenHero(
                                new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                    (int) blob.CenterOfGravity.Y + 150),
                                0));
                    }
                    else if (imstat.RedWithoutBlack.Median > imstat.GreenWithoutBlack.Median)
                    {
                        heroPoints[1].Add(
                            new OnScreenHero(
                                new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                    (int) blob.CenterOfGravity.Y + 150),
                                0));
                    }
                    else
                    {
                        heroPoints[2].Add(
                            new OnScreenHero(
                                new System.Drawing.Point((int) blob.CenterOfGravity.X,
                                    (int) blob.CenterOfGravity.Y + 150),
                                0));
                    }
                }
                state.FriendlyHeroes = heroPoints[2].ToArray();
                state.EnemyHeroes = heroPoints[1].ToArray();
            }
            catch (Exception e)
            {
            }


            MainWindow.DebugReaderStats("Heroes:" + TickNow());
            //DebugPlotVectorOnMap(state, null);
        }

        public static void UpdateTowers(CurrentGameState state)
        {
            TickNow();
            var unitPoints = new List<System.Drawing.Point>();
            try
            {
                //TickNow("unitSearchStart", dt);
                if (ShouldBringToFront) ControlInput.BringHeroesToFront();
                var src = GrabScreenGDI(new Rectangle(0, 100, 1024, 450));
                var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
                Tools.ColorFilter.Red = new IntRange(100, 255);
                Tools.ColorFilter.Green = new IntRange(0, 10);
                Tools.ColorFilter.Blue = new IntRange(0, 10);
                var filtered = Tools.ColorFilter.Apply(sourceImage);
                var grayScale = Grayscale.CommonAlgorithms.BT709.Apply(filtered);
                var bc = new BlobCounter();
                bc.ProcessImage(filtered);
                var blobs = bc.GetObjectsInformation().ToList();

                blobs =
                    blobs.Where(
                        o =>
                            !o.Rectangle.IntersectsWith(state.GameMode.MinimapRectangle) && o.Rectangle.Width > 10 &&
                            o.Rectangle.Width < 100 && o.Rectangle.Height >= 3 && o.Rectangle.Height <= 5).ToList();
                var i = 0;
                var gc = Graphics.FromImage(sourceImage);
                foreach (var blob in blobs)
                {
                    var rc = new Rectangle(blob.Rectangle.X, blob.Rectangle.Y, 100, 20);
                    Tools.Cropper.Rectangle = rc;
                    var cut = Tools.Cropper.Apply(sourceImage);
                    var res = Tools.TemplateMatcher.ProcessImage(cut, templateTower);
                    foreach (var r in res)
                    {
                        gc.DrawRectangle(new Pen(Color.Magenta), rc.X, rc.Y, 100, 20);
                        unitPoints.Add(new System.Drawing.Point(rc.X - 12, 100 + rc.Y + 350));
                    }
                }
                sourceImage.Save("./debug/towaz.png");
            }
            catch (Exception e)
            {
            }
            state.TowersFound = unitPoints.ToArray();

            if (state.TowersFound.Length > 0) Console.WriteLine("TOWERS " + state.TowersFound.Length);
            MainWindow.DebugReaderStats("Towers:" + TickNow());
        }

        public static void UpdateHeroHP(CurrentGameState state)
        {
            TickNow();
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(new Rectangle(160, 695, 155, 1));
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            Tools.ColorFilter.Red = new IntRange(50, 255);
            Tools.ColorFilter.Green = new IntRange(50, 255);
            Tools.ColorFilter.Blue = new IntRange(0, 50);
            Tools.ColorFilter.ApplyInPlace(sourceImage);
            var grayScale = Grayscale.CommonAlgorithms.BT709.Apply(sourceImage);
            Tools.Thresholder.ThresholdValue = 1;
            Tools.Thresholder.ApplyInPlace(grayScale);
            state.Me.HeroHP = Tools.FindHorizontalLineLength(grayScale)/145f*100f;
            src.Dispose();
            MainWindow.DebugReaderStats("HeroHP:" + TickNow());
        }

        public static void UpdateHasTalents(CurrentGameState state)
        {
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(gameScreen);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            Tools.Cropper.Rectangle = new Rectangle(70, 1020, 105, 30);
            var croppedImage = Tools.Cropper.Apply(sourceImage);

            var patternFinder = new ExhaustiveTemplateMatching(0.9f);
            var canLevel = false;
            try
            {
                var matches1 = patternFinder.ProcessImage(croppedImage, templateHasTalents);

                if (matches1.Length > 0)
                {
                    canLevel = true;
                }
            }
            catch (Exception e)
            {
            }
            state.Me.CanLevelTalents = canLevel;
        }

        public static bool AreTalentsOpen()
        {
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(gameScreen);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            Tools.Cropper.Rectangle = new Rectangle(48, 800, 390, 70);
            var croppedImage = Tools.Cropper.Apply(sourceImage);

            var patternFinder = new ExhaustiveTemplateMatching(0.9f);
            try
            {
                var matches = patternFinder.ProcessImage(croppedImage, templateTalents20);
                if (matches.Length == 1)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
            }
            return false;
        }

        public static void UpdateCurrentTalentLevel(CurrentGameState state)
        {
            /*
            if (ShouldBringToFront) ControlInput.BringHeroesToFront();
            var src = GrabScreenGDI(gameScreen);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);

            Tools.Cropper.Rectangle = new Rectangle(48, 800, 390, 70);
            var croppedImage = Tools.Cropper.Apply(sourceImage);
            state.Me.CharacterLevel = 1;
            var patternFinder = new ExhaustiveTemplateMatching(0.9f);
            try
            {
                var matches = patternFinder.ProcessImage(croppedImage, templateTalents4);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 4;
                    return;
                }
                matches = patternFinder.ProcessImage(croppedImage, templateTalents7);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 7;
                    return;
                }
                matches = patternFinder.ProcessImage(croppedImage, templateTalents10);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 10;
                    return;
                }
                matches = patternFinder.ProcessImage(croppedImage, templateTalents13);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 13;
                    return;
                }
                matches = patternFinder.ProcessImage(croppedImage, templateTalents16);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 16;
                    return;
                }
                matches = patternFinder.ProcessImage(croppedImage, templateTalents20);
                if (matches.Length == 1)
                {
                    state.Me.CharacterLevel = 20;
                }
            }
            catch (Exception e)
            {
            }
             
        }

        public static void UpdateCamps(CurrentGameState state)
        {
            var src = GrabScreenGDI(gameScreen);
            //var thresholded = ThresholdCropBitmap(src, new Rectangle(1450, 775, 450, 350), 200);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
            src.Dispose();
            Tools.Cropper.Rectangle = state.GameMode.MinimapRectangle;
            Tools.HSLFiltering.Hue = new IntRange(30, 63);
            Tools.HSLFiltering.Luminance = new Range(0, 1);
            Tools.HSLFiltering.Saturation = new Range(0.9f, 1);
            var croppedImage = Tools.Cropper.Apply(sourceImage);
            Tools.HSLFiltering.ApplyInPlace(croppedImage);
            try
            {
                var matches = Tools.TemplateMatcher.ProcessImage(croppedImage, templateCampSmall);
                var bigmatches = Tools.TemplateMatcher.ProcessImage(croppedImage, templateCampBig);
                if (matches.Length > 0)
                {
                }
            }
            catch (Exception e)
            {
            }
             * */
        }

        public static void DebugPlotVectorOnMap(CurrentGameState state, System.Drawing.Point[] points = null)
        {
            if (state.FriendlyHeroes == null) return;
            var src = GrabScreenGDI(gameScreen);
            //var thresholded = ThresholdCropBitmap(src, new Rectangle(1450, 775, 450, 350), 200);
            var sourceImage = ChangePixelFormat(src, PixelFormat.Format24bppRgb);
            src.Dispose();
            var gc = Graphics.FromImage(sourceImage);
            var greenBrush = new SolidBrush(Color.Lime);
            var redBrush = new SolidBrush(Color.Red);
            var font = new Font(SystemFonts.DefaultFont.FontFamily, 50);


            foreach (var point in state.TowersFound)
            {
                gc.DrawString("T", font, redBrush, point.X, point.Y);
            }
            foreach (var point in state.EnemyHeroes)
            {
                gc.DrawString("H", font, redBrush, point.Position.X, point.Position.Y);
            }
            foreach (var point in state.FriendlyHeroes)
                gc.DrawString("H", font, greenBrush, point.Position.X, point.Position.Y);

            foreach (var point in state.EnemyCreepsNearby)
                gc.DrawString("C", font, redBrush, point.X, point.Y);
            foreach (var point in state.FriendlyCreepsNearby)
                gc.DrawString("C", font, greenBrush, point.X, point.Y);

            try
            {
                var pp = new System.Drawing.Point();
                foreach (var p in points)
                {
                    gc.DrawEllipse(new Pen(Color.Magenta), p.X - 4, p.Y - 4, 8, 8);
                    if (!pp.IsEmpty)
                        gc.DrawLine(new Pen(Color.Lime), pp, p);
                    pp = p;
                }
            }
            catch (Exception e)
            {
            }

            gc.Dispose();
            sourceImage.Save("./debug/ss.png");
            sourceImage.Dispose();
        }

        public static class Tools
        {
            public static Crop Cropper = new Crop(new Rectangle(0, 0, 0, 0));
            public static Threshold Thresholder = new Threshold();
            public static GrayscaleToRGB GSToRgb = new GrayscaleToRGB();
            public static BlobCounter BlobCounter = new BlobCounter();
            public static ColorFiltering ColorFilter = new ColorFiltering();
            public static ExhaustiveTemplateMatching TemplateMatcher = new ExhaustiveTemplateMatching();
            public static HorizontalRunLengthSmoothing HLengthSmoothing = new HorizontalRunLengthSmoothing();
            public static VerticalRunLengthSmoothing VLengthSmoothing = new VerticalRunLengthSmoothing();
            public static HSLFiltering HSLFiltering = new HSLFiltering();

            public static int FindHorizontalLineLength(Bitmap bmp)
            {
                var count = 0;
                for (var x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, 0).ToArgb() == Color.Black.ToArgb())
                    {
                        if (count == 0) continue;
                        return count;
                    }
                    count++;
                }
                return count;
            }
        }

        public class OnScreenHero
        {
            public OnScreenHero(System.Drawing.Point position, int matchesCount)
            {
                Position = position;
                MatchesCount = matchesCount;
            }

            public System.Drawing.Point Position { get; set; }
            public int MatchesCount { get; set; }

            public static implicit operator System.Drawing.Point(OnScreenHero d)
            {
                return d.Position;
            }
        }
    }
}