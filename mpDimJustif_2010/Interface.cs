using System.Collections.Generic;
using ModPlusAPI.Interfaces;

namespace mpDimJustif
{
    public class Interface : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.AutoCAD;
        public string Name => "mpDimJustif";
        public string AvailProductExternalVersion => "2010";
        public string ClassName => string.Empty;
        public string LName => "Выравнивание размеров";
        public string Description => "Выравнивание выносных или размерных линий вдоль указанной прямой";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
        public bool CanAddToRibbon => true;
        public string FullDescription => string.Empty;
        public string ToolTipHelpImage => string.Empty;
        public List<string> SubFunctionsNames => new List<string>();
        public List<string> SubFunctionsLames => new List<string>();
        public List<string> SubDescriptions => new List<string>();
        public List<string> SubFullDescriptions => new List<string>();
        public List<string> SubHelpImages => new List<string>();
        public List<string> SubClassNames => new List<string>();
    }
}
