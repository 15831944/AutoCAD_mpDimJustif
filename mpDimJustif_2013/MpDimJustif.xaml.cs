using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using System;
using System.Windows;
using System.Windows.Input;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mpDimJustif
{
    using System.Collections.Generic;

    public partial class MpDimJustif
    {
        private const string LangItem = "mpDimJustif";

        public MpDimJustif()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
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
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var pso = new PromptSelectionOptions
                        {
                            MessageForAdding = "\n" + ModPlusAPI.Language.GetItem(LangItem, "msg1")
                        };

                        var sf = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "Dimension") });

                        var psr = ed.GetSelection(pso, sf);
                        if (psr.Status != PromptStatus.OK)
                            return;

                        var ppo = new PromptPointOptions("\n" + ModPlusAPI.Language.GetItem(LangItem, "msg2"));
                        var ppr = ed.GetPoint(ppo);
                        if (ppr.Status != PromptStatus.OK)
                            return;

                        var fPt = ModPlus.Helpers.AutocadHelpers.UcsToWcs(ppr.Value); //first point to WCS
                        var i = 0;
                        var pts1 = new Point3dCollection(); // Коллекция первых точек
                        var pts2 = new Point3dCollection(); // Коллекция вторых точек
                        var pts3 = new Point3dCollection(); // Коллекция 3 точек
                        var pts4 = new Point3dCollection(); // Коллекция 4 точек
                        foreach (var objId in psr.Value.GetObjectIds())
                        {
                            var o = objId.GetObject(OpenMode.ForRead);
                            doc.Editor.WriteMessage("\nType of dim: " + o.GetType().Name);

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
                            return;

                        tr.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionBox.Show(ex);
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
                    // Используем транзакцию
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var pso = new PromptSelectionOptions
                        {
                            MessageForAdding = "\n" + ModPlusAPI.Language.GetItem(LangItem, "msg3")
                        };

                        var sf = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "Dimension") });
                        var psr = ed.GetSelection(pso, sf);
                        if (psr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var ppo = new PromptPointOptions("\n" + ModPlusAPI.Language.GetItem(LangItem, "msg2"));
                        var ppr = ed.GetPoint(ppo);
                        if (ppr.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        var fPt = ModPlus.Helpers.AutocadHelpers.UcsToWcs(ppr.Value); //first point to WCS
                        var i = 0;
                        var pts1 = new Point3dCollection(); // Коллекция первых точек
                        var pts2 = new Point3dCollection(); // Коллекция вторых точек
                        var pts3 = new Point3dCollection(); // Коллекция 3 точек
                        var pts4 = new Point3dCollection(); // Коллекция 4 точек
                        ////foreach (var objId in psr.Value.GetObjectIds())
                        ////{
                        ////    var o = objId.GetObject(OpenMode.ForRead);
                        ////    if (o is RotatedDimension rotatedDimension)
                        ////    {
                        ////        //rotatedDimension.DimLinePoint
                        ////    }

                        ////    if (o is AlignedDimension alignedDimension)
                        ////    {
                        ////        //alignedDimension.DimLinePoint
                        ////    }

                        ////    var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                        ////    ////if (!dim.GetType().Name.Equals("RotatedDimension") &&
                        ////    ////    !dim.GetType().Name.Equals("AlignedDimension")) continue;
                        ////    var pts = new Point3dCollection();
                        ////    dim.GetStretchPoints(pts);
                        ////    pts1.Insert(i, pts[0]);
                        ////    pts2.Insert(i, pts[1]);
                        ////    pts3.Insert(i, pts[2]);
                        ////    pts4.Insert(i, pts[3]);
                        ////    i++;
                        ////}
                        List<Dimension> dimensions = new List<Dimension>();
                        foreach (var objectId in psr.Value.GetObjectIds())
                        {
                            if(tr.GetObject(objectId, OpenMode.ForWrite, false) is Dimension dimension)
                                dimensions.Add(dimension);
                        }
                        var jig = new MpDimLineJustifJig(); // JIg
                        var rs = jig.StartJig(
                            fPt,
                            dimensions
                            ////psr.Value.GetObjectIds(),
                            ////pts1, pts2, pts3, pts4
                            ); // Jig result
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
                ExceptionBox.Show(ex);
            }
        }
    }

    // Запуск функции
    public class MpDimJustifStart
    {
        MpDimJustif _mpDimJustif;
        [CommandMethod("ModPlus", "mpDimJustif", CommandFlags.Modal)]
        public void StartMpFormats()
        {
            Statistic.SendCommandStarting(new Interface());
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

        private void win_Closed(object sender, EventArgs e)
        {
            _mpDimJustif = null;
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
    }

    // Jig
    public class MpDimExtLineJustifJig : DrawJig
    {
        private const string LangItem = "mpDimJustif";

        private Point3d _prevPoint; // Предыдущая точка
        private Point3d _currentPoint; // Нынешняя точка
        private Point3d _startPoint;
        private ObjectId[] _objIds;
        Point3dCollection _ptsCol1 = new Point3dCollection();// Коллекция первых точек
        Point3dCollection _ptsCol2 = new Point3dCollection();// Коллекция вторых точек
        Point3dCollection _ptsCol3 = new Point3dCollection();// Коллекция 3 точек
        Point3dCollection _ptsCol4 = new Point3dCollection();// Коллекция 4 точек
        private Line _line;

        public Point3d Point()
        {
            return _currentPoint;
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
            var jppo = new JigPromptPointOptions("\n" + Language.GetItem(LangItem, "msg4"))
            {
                BasePoint = _line.StartPoint,
                UseBasePoint = true,
                UserInputControls = (UserInputControls.Accept3dCoordinates
                                     | UserInputControls.NoZeroResponseAccepted
                                     | UserInputControls.AcceptOtherInputString
                                     | UserInputControls.NoNegativeResponseAccepted)
            };
            var rs = prompts.AcquirePoint(jppo);
            _currentPoint = rs.Value;
            if (rs.Status != PromptStatus.OK) return SamplerStatus.Cancel;
            if (CursorHasMoved())
            {
                _prevPoint = _currentPoint;
                return SamplerStatus.OK;
            }
            return SamplerStatus.NoChange;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            _line.StartPoint = _startPoint;
            _line.EndPoint = _currentPoint;
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
                    lin1.IntersectWith(_line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                    lin2.IntersectWith(_line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                    if (ppp.Count > 0)
                    {
                        var vec1 = pts[0].GetVectorTo(ppp[0]);
                        var vec2 = pts[1].GetVectorTo(ppp[1]);
                        dim.MoveStretchPointsAt(new IntegerCollection { 0 }, vec1);
                        dim.MoveStretchPointsAt(new IntegerCollection { 1 }, vec2);
                        draw.Geometry.Draw(dim);
                    }
                    i++;
                }
            }

            return true;
        }
        private bool CursorHasMoved()
        {
            return _currentPoint.DistanceTo(_prevPoint) > 1e-6;
        }
    }

    public class MpDimLineJustifJig : DrawJig
    {
        private const string LangItem = "mpDimJustif";

        private Point3d _prevPoint; // Предыдущая точка
        private Point3d _currentPoint; // Нынешняя точка
        private Point3d _startPoint;

        private List<Dimension> _dimensions;
        ////private ObjectId[] _objIds;
        ////Point3dCollection _ptsCol1 = new Point3dCollection();// Коллекция первых точек
        ////Point3dCollection _ptsCol3 = new Point3dCollection();// Коллекция 3 точек
        private Line _line;

        public Point3d Point()
        {
            return _currentPoint;
        }

        public PromptResult StartJig(
            Point3d fPt,
            List<Dimension> dimensions
            ////ObjectId[] dimCol,
            ////Point3dCollection pts1,
            ////Point3dCollection pts2,
            ////Point3dCollection pts3,
            ////Point3dCollection pts4
            )
        {
            ////_objIds = dimCol;
            ////_ptsCol1 = pts1;
            ////_ptsCol3 = pts3;
            _prevPoint = new Point3d(0, 0, 0);
            _startPoint = fPt;
            _line = new Line { StartPoint = fPt };
            _dimensions = dimensions;
            return AcApp.DocumentManager.MdiActiveDocument.Editor.Drag(this);
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions("\n" + Language.GetItem(LangItem, "msg4"))
            {
                BasePoint = _line.StartPoint,
                UseBasePoint = true,
                UserInputControls = (UserInputControls.Accept3dCoordinates
                                     | UserInputControls.NoZeroResponseAccepted
                                     | UserInputControls.AcceptOtherInputString
                                     | UserInputControls.NoNegativeResponseAccepted)
            };
            var rs = prompts.AcquirePoint(jppo);
            _currentPoint = rs.Value;
            if (rs.Status != PromptStatus.OK) return SamplerStatus.Cancel;
            if (CursorHasMoved())
            {
                _prevPoint = _currentPoint;
                return SamplerStatus.OK;
            }
            return SamplerStatus.NoChange;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            _line.StartPoint = _startPoint;
            _line.EndPoint = _currentPoint;
            draw.Geometry.Draw(_line);
            var i = 0;
            foreach (var dimObj in _dimensions)
            {
                ////var dim = (Dimension)objId.GetObject(OpenMode.ForWrite, false);
                ////////if (!dim.GetType().Name.Equals("RotatedDimension") && !dim.GetType().Name.Equals("AlignedDimension"))
                ////////    continue;
                ////var pts = new Point3dCollection();
                ////dim.GetStretchPoints(pts);
                ////var ppp = new Point3dCollection();
                ////var lin1 = new Line(_ptsCol1[i], _ptsCol3[i]);
                ////lin1.IntersectWith(_line, Intersect.ExtendBoth, ppp, IntPtr.Zero, IntPtr.Zero);
                ////if (ppp.Count > 0)
                ////{
                ////    var vec1 = pts[2].GetVectorTo(ppp[0]);

                ////    dim.MoveStretchPointsAt(new IntegerCollection { 2 }, vec1);
                ////    draw.Geometry.Draw(dim);
                ////}

                ////var dimObj = objId.GetObject(OpenMode.ForWrite, false);
                if (dimObj is RotatedDimension rotatedDimension)
                {
                    var newPt = _line.GetClosestPointTo(rotatedDimension.DimLinePoint, true);
                    rotatedDimension.DimLinePoint = newPt;
                    draw.Geometry.Draw(dimObj);
                }

                i++;
            }

            return true;
        }
        private bool CursorHasMoved()
        {
            return _currentPoint.DistanceTo(_prevPoint) > 1e-6;
        }
    }
}
