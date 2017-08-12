using mpPInterface;

namespace mpDimJustif
{
    public class Interface : IPluginInterface
    {
        public string Name => "mpDimJustif";
        public string AvailCad => "2017";
        public string LName => "Выравнивание размеров";
        public string Description => "Выравнивание выносных или размерных линий вдоль указанной прямой";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
    }
}
