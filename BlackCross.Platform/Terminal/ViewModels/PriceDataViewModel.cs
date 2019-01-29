using OxyPlot;
using OxyPlot.Axes;

namespace BlackCross.Platform.Terminal.ViewModels
{
    public class PriceDataViewModel : ViewModelBase
    {
        public PlotModel PriceChartModel { get; private set; }

        public PriceDataViewModel()
        {
            PriceChartModel = new PlotModel();
            PriceChartModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
            });

            PriceChartModel.Title = "Price chart";
        }
    }
}
