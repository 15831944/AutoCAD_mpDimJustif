namespace mpDimJustif
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.GraphicsInterface;
    using ModPlusAPI;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public class DimExtLineJustifyJig : DrawJig
    {
        private const string LangItem = "mpDimJustif";

        private Point3d _prevPoint;
        private Point3d _currentPoint;
        private Point3d _startPoint;

        private List<Tuple<ObjectId, Dimension, Point3d, Point3d>> _dimensions;
        private Line _line;

        public PromptResult StartJig(
            Point3d fPt,
            List<Tuple<ObjectId, Dimension, Point3d, Point3d>> dimensions)
        {
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
            for (var i = 0; i < _dimensions.Count; i++)
            {
                var tuple = _dimensions[i];
                if (tuple.Item2 is RotatedDimension rotatedDimension)
                {
                    var sourceDim = (Dimension)tuple.Item1.Open(OpenMode.ForRead, false);
                    var sourceDimStretchPoints = new Point3dCollection();
                    sourceDim.GetStretchPoints(sourceDimStretchPoints);

                    var firstHelpLine = new Line(sourceDimStretchPoints[0], sourceDimStretchPoints[2]);
                    var secondHelpLine = new Line(sourceDimStretchPoints[1], sourceDimStretchPoints[3]);

                    var intersectPoints = new Point3dCollection();
                    firstHelpLine.IntersectWith(_line, Intersect.ExtendBoth, intersectPoints, IntPtr.Zero, IntPtr.Zero);
                    secondHelpLine.IntersectWith(_line, Intersect.ExtendBoth, intersectPoints, IntPtr.Zero, IntPtr.Zero);

                    if (intersectPoints.Count > 0)
                    {
                        var copyDimStretchPoints = new Point3dCollection();
                        rotatedDimension.GetStretchPoints(copyDimStretchPoints);
                        rotatedDimension.MoveStretchPointsAt(new IntegerCollection { 0 }, copyDimStretchPoints[0].GetVectorTo(intersectPoints[0]));
                        rotatedDimension.MoveStretchPointsAt(new IntegerCollection { 1 }, copyDimStretchPoints[1].GetVectorTo(intersectPoints[1]));
                        _dimensions[i] = new Tuple<ObjectId, Dimension, Point3d, Point3d>(tuple.Item1, tuple.Item2, rotatedDimension.XLine1Point, rotatedDimension.XLine2Point);
                        draw.Geometry.Draw(tuple.Item2);
                    }

                    sourceDim.Dispose();
                }
            }

            return true;
        }

        private bool CursorHasMoved()
        {
            return _currentPoint.DistanceTo(_prevPoint) > 1e-6;
        }
    }
}
