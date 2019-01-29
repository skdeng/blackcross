using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BlackCross.Platform.Terminal.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BlackCross.Platform.Terminal.Views
{
    /// <summary>
    /// Interaction logic for PriceChart.xaml
    /// </summary>
    public partial class PriceChart : UserControl
    {
        public PriceDataViewModel ChartModel { get; set; }

        public PriceChart()
        {
            InitializeComponent();
            ChartModel = new PriceDataViewModel();
            DataContext = ChartModel;
        }

        public void AddPriceData(IEnumerable<(DateTime time, double price)> priceData)
        {
            var priceDataSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(0, 0, 200),
                Title = "Price"
            };
            priceDataSeries.Points.AddRange(priceData.Select(d => new DataPoint(DateTimeAxis.ToDouble(d.time), d.price)));
            ChartModel.PriceChartModel.Series.Add(priceDataSeries);
            ChartModel.PriceChartModel.InvalidatePlot(true);
        }

        public void AddBuySignals(IEnumerable<(DateTime time, double price)> buySignals)
        {
            var buySignalSeries = new ScatterSeries
            {
                MarkerFill = OxyColor.FromArgb(255, 0, 255, 0),
                MarkerType = MarkerType.Circle,
                Title = "Buy trades"
            };
            buySignalSeries.Points.AddRange(buySignals.Select(d => new ScatterPoint(DateTimeAxis.ToDouble(d.time), d.price)));
            ChartModel.PriceChartModel.Series.Add(buySignalSeries);
            ChartModel.PriceChartModel.InvalidatePlot(true);
        }

        public void AddSellSignals(IEnumerable<(DateTime time, double price)> sellSignals)
        {
            var sellSignalSeries = new ScatterSeries
            {
                MarkerFill = OxyColor.FromArgb(255, 255, 0, 0),
                MarkerType = MarkerType.Circle,
                Title = "Sell trades"
            };
            ChartModel.PriceChartModel.Series.Add(sellSignalSeries);
            sellSignalSeries.Points.AddRange(sellSignals.Select(d => new ScatterPoint(DateTimeAxis.ToDouble(d.time), d.price)));
            ChartModel.PriceChartModel.InvalidatePlot(true);
        }

        public void Clear()
        {
            ChartModel.PriceChartModel.Series.Clear();
            ChartModel.PriceChartModel.InvalidatePlot(true);
        }
    }
}
