namespace mpDimJustif
{
    using System;
    using Autodesk.AutoCAD.Runtime;
    using ModPlusAPI;
    using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    // Запуск функции
    public class MpDimJustifStart
    {
        MpDimJustif _mpDimJustif;
        [CommandMethod("ModPlus", "mpDimJustif", CommandFlags.Modal)]
        public void StartFunction()
        {
            Statistic.SendCommandStarting(new ModPlusConnector());
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
}
