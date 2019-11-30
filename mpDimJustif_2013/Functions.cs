namespace mpDimJustif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public static class Functions
    {
        private const string LangItem = "mpDimJustif";

        public static void DimLineJustify()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            try
            {
                using (doc.LockDocument())
                {
                    var pso = new PromptSelectionOptions
                    {
                        MessageForAdding = "\n" + Language.GetItem(LangItem, "msg3")
                    };

                    var sf = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "Dimension") });
                    var psr = ed.GetSelection(pso, sf);
                    if (psr.Status != PromptStatus.OK)
                        return;

                    var ppo = new PromptPointOptions("\n" + Language.GetItem(LangItem, "msg2"));
                    var ppr = ed.GetPoint(ppo);
                    if (ppr.Status != PromptStatus.OK)
                        return;

                    var dimensions = new List<Tuple<ObjectId, Dimension, Point3d>>();

                    foreach (var objectId in psr.Value.GetObjectIds())
                    {
                        using (var dimension = objectId.Open(OpenMode.ForWrite) as Dimension)
                        {
                            if (dimension != null && (dimension is RotatedDimension || dimension is AlignedDimension))
                            {
                                var dimClone = (Dimension)dimension.Clone();
                                dimension.Visible = false;
                                dimensions.Add(new Tuple<ObjectId, Dimension, Point3d>(objectId, dimClone, Point3d.Origin));
                            }
                        }
                    }

                    if (!dimensions.Any())
                        return;

                    var jig = new DimLineJustifyJig();
                    var rs = jig.StartJig(ppr.Value.TransformBy(ed.CurrentUserCoordinateSystem), dimensions);

                    var changePoints = rs.Status == PromptStatus.OK;
                    foreach (var tuple in dimensions)
                    {
                        using (var dimension = tuple.Item1.Open(OpenMode.ForWrite) as Dimension)
                        {
                            if (dimension != null)
                            {
                                dimension.Visible = true;
                                if (changePoints)
                                {
                                    if (dimension is RotatedDimension rotatedDimension)
                                        rotatedDimension.DimLinePoint = tuple.Item3;
                                    if (dimension is AlignedDimension alignedDimension)
                                        alignedDimension.DimLinePoint = tuple.Item3;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }

        public static void DimExtLineJustify()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            try
            {
                using (doc.LockDocument())
                {
                    var pso = new PromptSelectionOptions
                    {
                        MessageForAdding = "\n" + Language.GetItem(LangItem, "msg1")
                    };

                    var sf = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "Dimension") });
                    var psr = ed.GetSelection(pso, sf);
                    if (psr.Status != PromptStatus.OK)
                        return;

                    var ppo = new PromptPointOptions("\n" + Language.GetItem(LangItem, "msg2"));
                    var ppr = ed.GetPoint(ppo);
                    if (ppr.Status != PromptStatus.OK)
                        return;

                    var dimensions = new List<Tuple<ObjectId, Dimension, Point3d, Point3d>>();

                    foreach (var objectId in psr.Value.GetObjectIds())
                    {
                        using (var dimension = objectId.Open(OpenMode.ForWrite) as Dimension)
                        {
                            if (dimension != null && dimension is RotatedDimension)
                            {
                                var dimClone = (Dimension)dimension.Clone();
                                dimension.Visible = false;
                                dimensions.Add(new Tuple<ObjectId, Dimension, Point3d, Point3d>(objectId, dimClone, Point3d.Origin, Point3d.Origin));
                            }
                        }
                    }

                    if (!dimensions.Any())
                        return;

                    var jig = new DimExtLineJustifyJig();
                    var rs = jig.StartJig(ppr.Value.TransformBy(ed.CurrentUserCoordinateSystem), dimensions);

                    var changePoints = rs.Status == PromptStatus.OK;
                    foreach (var tuple in dimensions)
                    {
                        using (var dimension = tuple.Item1.Open(OpenMode.ForWrite) as Dimension)
                        {
                            if (dimension != null)
                            {
                                dimension.Visible = true;
                                if (changePoints && dimension is RotatedDimension rotatedDimension)
                                {
                                    rotatedDimension.XLine1Point = tuple.Item3;
                                    rotatedDimension.XLine2Point = tuple.Item4;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
    }
}
