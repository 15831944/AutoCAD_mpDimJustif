namespace mpDimJustif
{
    using System.Windows;
    using System.Windows.Input;

    public partial class MpDimJustif
    {
        private const string LangItem = "mpDimJustif";

        public MpDimJustif()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
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
            Functions.DimExtLineJustify();
        }

        // Выравнивание размерных линий вдоль указанной прямой
        private void BtDimLineJustif_OnClick(object sender, RoutedEventArgs e)
        {
            // Закрываем окно
            Close();
            Functions.DimLineJustify();
        }
    }
}
