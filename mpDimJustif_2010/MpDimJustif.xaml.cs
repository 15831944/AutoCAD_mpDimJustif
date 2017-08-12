#if ac2010
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#elif ac2013
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ModPlus;
using mpMsg;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using mpSettings;

namespace mpDimJustif
{
    /// <summary>
    /// Логика взаимодействия для MpFormats.xaml
    /// </summary>
    public partial class MpDimJustif
    {
        public MpDimJustif()
        {
            InitializeComponent();
            MpWindowHelpers.OnWindowStartUp(
                this,
                MpSettings.GetValue("Settings", "MainSet", "Theme"),
                MpSettings.GetValue("Settings", "MainSet", "AccentColor"),
                MpSettings.GetValue("Settings", "MainSet", "BordersType")
                );
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
        private void MetroWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            Focus();
        }
        private void MetroWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
        // Выравнивание выносных линий вдоль указанной прямой
        private void BtDimExtLineJustif_OnClick(object sender, RoutedEventArgs e)
        {
            // Закрываем окно
            Close();

            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            try
            {
                using (doc.LockDocument())
                {
                    // Используем транзикцию
                    var tr = db.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        var pso = new PromptSelectionOptions
                        {
                            MessageForAdding = "\nВыберите размеры для выравнивания выносных линий"
                        };

                        var sf = new SelectionFilter(
                            new[] { new TypedValue((int)DxfCode.Start, "Dimension") });
                        var psr = ed.GetSelection(pso, sf);
                        if (psr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var ppo = new PromptPointOptions("\nПервая точка линии выравнивания: ");
                        var ppr = ed.GetPoint(ppo);
                        if (ppr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var fPt = MpCadHelpers.UcsToWcs(ppr.Value); //first point to WCS
                        var i = 0;
                        var pts1 = new Point3dCollection(); // Колекция первых точек
                        var pts2 = new Point3dCollection(); // Колекция вторых точек
                        var pts3 = new Point3dCollection(); // Колекция 3 точек
                        var pts4 = new Point3dCollection(); // Колекция 4 точек
                        foreach (var objId in psr.Value.GetObjectIds())
                        {
                            var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                            if (dim.GetType().Name.Equals("RotatedDimension"))
                            {
                                var pts = new Point3dCollection();
                                dim.GetStretchPoints(pts);
                                pts1.Insert(i, pts[0]);
                                pts2.Insert(i, pts[1]);
                                pts3.Insert(i, pts[2]);
                                pts4.Insert(i, pts[3]);
                                i++;
                            }
                        }
                        var jig = new MpDimExtLineJustifJig(); // JIg
                        var rs = jig.StartJig(fPt,
                            psr.Value.GetObjectIds(),
                            pts1, pts2, pts3, pts4); // Jig result
                        if (rs.Status != PromptStatus.OK)
                        {
                            return;
                        } // If Jig not OK

                        tr.Commit();
                    } // tr
                }// LockDocument
            } // try
            catch (System.Exception ex)
            {
                MpExWin.Show(ex);
            }
        }
        // Выравнивание размерных линий вдоль указанной прямой
        private void BtDimLineJustif_OnClick(object sender, RoutedEventArgs e)
        {
            // Закрываем окно
            Close();

            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            try
            {
                using (doc.LockDocument())
                {
                    // Используем транзикцию
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var pso = new PromptSelectionOptions
                        {
                            MessageForAdding = "\nВыберите размеры для выравнивания размерных линий"
                        };

                        var sf = new SelectionFilter(
                            new[] { new TypedValue((int)DxfCode.Start, "Dimension") });
                        var psr = ed.GetSelection(pso, sf);
                        if (psr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var ppo = new PromptPointOptions("\nПервая точка линии выравнивания: ");
                        var ppr = ed.GetPoint(ppo);
                        if (ppr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var fPt = MpCadHelpers.UcsToWcs(ppr.Value); //first point to WCS
                        var i = 0;
                        var pts1 = new Point3dCollection(); // Колекция первых точек
                        var pts2 = new Point3dCollection(); // Колекция вторых точек
                        var pts3 = new Point3dCollection(); // Колекция 3 точек
                        var pts4 = new Point3dCollection(); // Колекция 4 точек
                        foreach (var objId in psr.Value.GetObjectIds())
                        {
                            var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                            if (!dim.GetType().Name.Equals("RotatedDimension") &&
                                !dim.GetType().Name.Equals("AlignedDimension")) continue;
                            var pts = new Point3dCollection();
                            dim.GetStretchPoints(pts);
                            pts1.Insert(i, pts[0]);
                            pts2.Insert(i, pts[1]);
                            pts3.Insert(i, pts[2]);
                            pts4.Insert(i, pts[3]);
                            i++;
                        }
                        var jig = new MpDimLineJustifJig(); // JIg
                        var rs = jig.StartJig(fPt,
                            psr.Value.GetObjectIds(),
                            pts1, pts2, pts3, pts4); // Jig result
                        if (rs.Status != PromptStatus.OK)
                        {
                            return;
                        } // If Jig not OK

                        tr.Commit();
                    } // tr
                }// LockDocument
            } // try
            catch (System.Exception ex)
            {
                MpExWin.Show(ex);
            }
        }
    }
    // Запуск функции и создание блока
    public class MpDimJustifStart
    {
        MpDimJustif _mpDimJustif;
        [CommandMethod("ModPlus", "mpDimJustif", CommandFlags.Modal)]
        public void StartMpFormats()
        {
            if (_mpDimJustif == null)
            {
                _mpDimJustif = new MpDimJustif();
                _mpDimJustif.Closed += win_Closed;
            }

            if (_mpDimJustif.IsLoaded)
                _mpDimJustif.Activate();
            else
                AcApp.ShowModelessWindow(AcApp.MainWindow.Handle, _mpDimJustif);
        }
        void win_Closed(object sender, EventArgs e)
        {
            _mpDimJustif = null;
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
    }
    // Jig
    public class MpDimExtLineJustifJig : DrawJig
    {
        private Point3d _prevPoint; // Предыдущая точка
        private Point3d _currPoint; // Нинешняя точка
        private Point3d _startPoint;
        private ObjectId[] _objIds;
        Point3dCollection _ptsCol1 = new Point3dCollection();// Колекция первых точек
        Point3dCollection _ptsCol2 = new Point3dCollection();// Колекция вторых точек
        Point3dCollection _ptsCol3 = new Point3dCollection();// Колекция 3 точек
        Point3dCollection _ptsCol4 = new Point3dCollection();// Колекция 4 точек
        private Line _line;
        public Point3d Point()
        {
            return _currPoint;
        }
        public PromptResult StartJig(
            Point3d fPt,
            ObjectId[] dimCol,
            Point3dCollection pts1,
            Point3dCollection pts2,
            Point3dCollection pts3,
            Point3dCollection pts4
            )
        {
            _objIds = dimCol;
            _ptsCol1 = pts1;
            _ptsCol2 = pts2;
            _ptsCol3 = pts3;
            _ptsCol4 = pts4;
            _prevPoint = new Point3d(0, 0, 0);
            _startPoint = fPt;
            _line = new Line { StartPoint = fPt };
            return AcApp.DocumentManager.MdiActiveDocument.Editor.Drag(this);
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions("\nВторая точка линии выравнивания: ")
            {
                BasePoint = _line.StartPoint,
                UseBasePoint = true,
                UserInputControls = (UserInputControls.Accept3dCoordinates
                                     | UserInputControls.NoZeroResponseAccepted
                                     | UserInputControls.AcceptOtherInputString
                                     | UserInputControls.NoNegativeResponseAccepted)
            };
            var rs = prompts.AcquirePoint(jppo);
            _currPoint = rs.Value;
            if (rs.Status != PromptStatus.OK) return SamplerStatus.Cancel;
            if (CursorHasMoved())
            {
                _prevPoint = _currPoint;
                return SamplerStatus.OK;
            }
            return SamplerStatus.NoChange;
        }
        protected override bool WorldDraw(WorldDraw draw)
        {
            _line.StartPoint = _startPoint;
            _line.EndPoint = _currPoint;
            draw.Geometry.Draw(_line);
            var i = 0;
            foreach (var objId in _objIds)
            {
                var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                if (dim.GetType().Name.Equals("RotatedDimension"))
                {
                    var pts = new Point3dCollection();
                    dim.GetStretchPoints(pts);
                    var ppp = new Point3dCollection();
                    var lin1 = new Line(_ptsCol1[i], _ptsCol3[i]);
                    var lin2 = new Line(_ptsCol2[i], _ptsCol4[i]);
                    // Только для 2010 автокада!
                    PlatformCompatibilityExtensionMethods.IntersectWith(lin1, _line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                    PlatformCompatibilityExtensionMethods.IntersectWith(lin2, _line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                    //lin1.IntersectWith(_line, Intersect.ExtendBoth, ppp, 0, 0);
                    //lin2.IntersectWith(_line, Intersect.ExtendBoth, ppp, 0, 0);
                    if (ppp.Count > 0)
                    {
                        var vec1 = pts[0].GetVectorTo(ppp[0]);
                        var vec2 = pts[1].GetVectorTo(ppp[1]);
                        dim.MoveStretchPointsAt(new IntegerCollection { 0 }, vec1);
                        dim.MoveStretchPointsAt(new IntegerCollection { 1 }, vec2);
                        draw.Geometry.Draw(dim);
                    }
                    i++;
                }// if
            }// foreach

            return true;
        }
        private bool CursorHasMoved()
        {
            return _currPoint.DistanceTo(_prevPoint) > 1e-6;
        }
    }// public class MpDimExtLineJustifJig : DrawJig
    public class MpDimLineJustifJig : DrawJig
    {
        private Point3d _prevPoint; // Предыдущая точка
        private Point3d _currPoint; // Нинешняя точка
        private Point3d _startPoint;
        private ObjectId[] _objIds;
        Point3dCollection _ptsCol1 = new Point3dCollection();// Колекция первых точек
        Point3dCollection _ptsCol3 = new Point3dCollection();// Колекция 3 точек
        private Line _line;
        public Point3d Point()
        {
            return _currPoint;
        }
        public PromptResult StartJig(
            Point3d fPt,
            ObjectId[] dimCol,
            Point3dCollection pts1,
            Point3dCollection pts2,
            Point3dCollection pts3,
            Point3dCollection pts4
            )
        {
            _objIds = dimCol;
            _ptsCol1 = pts1;
            _ptsCol3 = pts3;
            _prevPoint = new Point3d(0, 0, 0);
            _startPoint = fPt;
            _line = new Line { StartPoint = fPt };
            return AcApp.DocumentManager.MdiActiveDocument.Editor.Drag(this);
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions("\nВторая точка линии выравнивания: ")
            {
                BasePoint = _line.StartPoint,
                UseBasePoint = true,
                UserInputControls = (UserInputControls.Accept3dCoordinates
                                     | UserInputControls.NoZeroResponseAccepted
                                     | UserInputControls.AcceptOtherInputString
                                     | UserInputControls.NoNegativeResponseAccepted)
            };
            var rs = prompts.AcquirePoint(jppo);
            _currPoint = rs.Value;
            if (rs.Status != PromptStatus.OK) return SamplerStatus.Cancel;
            if (CursorHasMoved())
            {
                _prevPoint = _currPoint;
                return SamplerStatus.OK;
            }
            return SamplerStatus.NoChange;
        }
        protected override bool WorldDraw(WorldDraw draw)
        {
            _line.StartPoint = _startPoint;
            _line.EndPoint = _currPoint;
            draw.Geometry.Draw(_line);
            var i = 0;
            foreach (var objId in _objIds)
            {
                var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                if (!dim.GetType().Name.Equals("RotatedDimension") && !dim.GetType().Name.Equals("AlignedDimension"))
                    continue;
                var pts = new Point3dCollection();
                dim.GetStretchPoints(pts);
                var ppp = new Point3dCollection();
                var lin1 = new Line(_ptsCol1[i], _ptsCol3[i]);
                // Только для 2010 автокада!
                PlatformCompatibilityExtensionMethods.IntersectWith(lin1, _line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                //lin1.IntersectWith(_line, Intersect.ExtendBoth, ppp, 0, 0);
                if (ppp.Count > 0)
                {
                    var vec1 = pts[2].GetVectorTo(ppp[0]);

                    dim.MoveStretchPointsAt(new IntegerCollection { 2 }, vec1);
                    draw.Geometry.Draw(dim);
                }
                i++;
            }// foreach

            return true;
        }
        private bool CursorHasMoved()
        {
            return _currPoint.DistanceTo(_prevPoint) > 1e-6;
        }
    }// public class MpDimExtLineJustifJig : DrawJig
    /// <summary>
    /// 
    /// Platform compatibility extension methods for 
    /// Autodesk.AutoCAD.DatabaseServices.Entity
    /// 
    /// These methods make it easier to deploy a single,
    /// platform-neutral managed assembly that can run 
    /// on both 32 and 64 bit AutoCAD.
    /// 
    /// </summary>

    public static class PlatformCompatibilityExtensionMethods
    {

        /// <summary>
        /// Extension methods that act as platform-independent
        /// surrogates for the Entity.IntersectWith() method, 
        /// allowing a single managed assembly that uses them
        /// to run on both 32 or 64 bit AutoCAD.
        /// 
        /// The following extension method overloads are supported:
        /// 
        ///   IntersectWith( Entity, Intersect, Point3dCollection );
        ///   IntersectWith( Entity, Intersect, Point3dCollection, IntPtr, IntPtr );
        ///   IntersectWith( Entity, Intersect, Plane, Point3dCollection );
        ///   IntersectWith( Entity, Intersect, Plane, Point3dCollection, IntPtr, IntPtr );
        ///    
        /// The versions which do not require IntPtr as the last two arguments
        /// pass the default of 0 for the GsMarker parameters of the Entity's
        /// IntersectWith() method.
        /// 
        /// The versions that require two IntPtr arguments as their last two
        /// parameters convert the passed IntPtr to the required type (Int32
        /// or Int64, depending on the platform the code is running on), and
        /// pass those values for the GsMarker parameters.
        /// 
        /// All other parameters are equivalent to the corresponding
        /// parameters of the Entity's IntersectWith() method.
        /// 
        /// All overloads return the number of intersections found.
        /// 
        /// Use these methods in lieu of Entity.IntersectWith() to 
        /// enable your code to run on both 32 and 64 bit systems.
        /// 
        /// Performance Issues:
        /// 
        /// Because these extension methods use reflection to invoke the 
        /// underlying methods they act as surrogates for, they will not 
        /// perform as well as direct calls to those underlying methods. 
        /// This can be an issue in performance intensive applications, 
        /// and in such cases a hand-coded solution that avoids the use 
        /// of reflection may be a preferable alternative.
        /// 
        /// </summary>


        static PlatformCompatibilityExtensionMethods()
        {
            Object test32 = (Int32)0;
            Object test64 = (Int64)0;

            Console.Write(test32);
            Console.Write(test64);

            Type[] types1 = null;
            Type[] types2 = null;
            if (IntPtr.Size > 4)
            {
                types1 = new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(Int64), typeof(Int64) };
                types2 = new Type[] { typeof(Entity), typeof(Intersect), typeof(Plane), typeof(Point3dCollection), typeof(Int64), typeof(Int64) };
            }
            else
            {
                types1 = new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(Int32), typeof(Int32) };
                types2 = new Type[] { typeof(Entity), typeof(Intersect), typeof(Plane), typeof(Point3dCollection), typeof(Int32), typeof(Int32) };
            }

            intersectWithMethod1 = typeof(Entity).GetMethod("IntersectWith", BindingFlags.Public | BindingFlags.Instance, null, types1, null);
            intersectWithMethod2 = typeof(Entity).GetMethod("IntersectWith", BindingFlags.Public | BindingFlags.Instance, null, types2, null);
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Point3dCollection points)
        {
            int start = points.Count;
            object[] args = null;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, points, (Int64)0, (Int64)0 };
            else
                args = new object[] { other, intersectType, points, (Int32)0, (Int32)0 };
            intersectWithMethod1.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Point3dCollection points, IntPtr thisGsMarker, IntPtr otherGsMarker)
        {
            int start = points.Count;
            object[] args = null;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, points, thisGsMarker.ToInt64(), otherGsMarker.ToInt64() };
            else
                args = new object[] { other, intersectType, points, thisGsMarker.ToInt32(), otherGsMarker.ToInt32() };
            intersectWithMethod1.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Plane plane, Point3dCollection points)
        {
            int start = points.Count;
            object[] args = null;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, plane, points, (Int64)0, (Int64)0 };
            else
                args = new object[] { other, intersectType, plane, points, (Int32)0, (Int32)0 };
            intersectWithMethod2.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Plane plane, Point3dCollection points, IntPtr thisGsMarker, IntPtr otherGsMarker)
        {
            int start = points.Count;
            object[] args = null;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, plane, points, thisGsMarker.ToInt64(), otherGsMarker.ToInt64() };
            else
                args = new object[] { other, intersectType, plane, points, thisGsMarker.ToInt32(), otherGsMarker.ToInt32() };
            intersectWithMethod2.Invoke(entity, args);
            return points.Count - start;
        }

        static MethodInfo intersectWithMethod1 = null;
        static MethodInfo intersectWithMethod2 = null;


    }
}
