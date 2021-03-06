﻿using System;
using System.Linq;
using SciChart.Examples.Demo.Data;
using SciChart.Examples.Demo.Fragments.Base;
using SciChart.iOS.Charting;
using UIKit;
using Foundation;
using Xamarin.Examples.Demo.iOS.Views.Base;

namespace Xamarin.Examples.Demo.iOS.Views.Examples
{
    [ExampleDefinition("Multi-Pane Stock Charts", description: "Creates an example with static multi-pane stock chart with Indicator and Volume panes", icon: ExampleIcon.CandlestickChart)]
    public class CreateMultiPaneStockChartsViewController : ExampleBaseViewController
    {
        private static readonly string VOLUME = "Volume";
        private static readonly string PRICES = "Prices";
        private static readonly string RSI = "RSI";
        private static readonly string MACD = "MACD";

        public override Type ExampleViewType => typeof(MultiplePaneStockChartsLayout);

        public SCIChartSurface PriceChart => ((MultiplePaneStockChartsLayout)View).PriceSurfaceView;
        public SCIChartSurface MacdChart => ((MultiplePaneStockChartsLayout)View).MacdSurfaceView;
        public SCIChartSurface RsiChart => ((MultiplePaneStockChartsLayout)View).RsiSurfaceView;
        public SCIChartSurface VolumeChart => ((MultiplePaneStockChartsLayout)View).VolumeChartView;

        private readonly SCIAxisRangeSynchronization _axisRangeSync = new SCIAxisRangeSynchronization();
        private readonly SCIAxisAreaSizeSynchronization _axisAreaSizeSync = new SCIAxisAreaSizeSynchronization();

        protected override void InitExample()
        {
            _axisAreaSizeSync.SyncMode = SCIAxisSizeSyncMode.Right;

            var priceData = DataManager.Instance.GetPriceDataEurUsd();

            var pricePaneModel = new PricePaneModel(priceData);
            var macdPaneModel = new MacdPaneModel(priceData);
            var rsiPaneModel = new RsiPaneModel(priceData);
            var volumePaneModel = new VolumePaneModel(priceData);

            InitChart(PriceChart, pricePaneModel, true);
            InitChart(MacdChart, macdPaneModel, false);
            InitChart(RsiChart, rsiPaneModel, false);
            InitChart(VolumeChart, volumePaneModel, false);
        }

        private void InitChart(SCIChartSurface surface, BasePaneModel model, bool isMainPain)
        {
            _axisAreaSizeSync.AttachSurface(surface);

            var xAxis = new SCICategoryDateTimeAxis { IsVisible = isMainPain, GrowBy = new SCIDoubleRange(0, 0.05) };
            _axisRangeSync.AttachAxis(xAxis);

            using (surface.SuspendUpdates())
            {
                surface.XAxes.Add(xAxis);
                surface.YAxes.Add(model.YAxis);
                surface.RenderableSeries = model.RenderableSeries;
                surface.Annotations = model.Annotations;

                surface.ChartModifiers = new SCIChartModifierCollection
                {
                    new SCIXAxisDragModifier { DragMode = SCIAxisDragMode.Pan, ClipModeX = SCIClipMode.StretchAtExtents },
                    new SCIPinchZoomModifier { Direction = SCIDirection2D.XDirection },
                    new SCIZoomPanModifier(),
                    new SCIZoomExtentsModifier(),
                    new SCILegendModifier { ShowCheckBoxes = false, StyleOfItemCell = new SCILegendCellStyle() { SeriesNameFont = UIFont.FromName("Helvetica", 10f) } }
                };
            }
        }

        private abstract class BasePaneModel
        {
            public SCIRenderableSeriesCollection RenderableSeries = new SCIRenderableSeriesCollection();
            public readonly SCIAnnotationCollection Annotations = new SCIAnnotationCollection();
            public readonly SCINumericAxis YAxis;
            public readonly string Title;

            protected BasePaneModel(string title, string yAxisTextFormatting, bool isFirstPane)
            {
                Title = title;
                YAxis = new SCINumericAxis
                {
                    AxisId = title,
                    AutoRange = SCIAutoRange.Always,
                    MinorsPerMajor = isFirstPane ? 4 : 2,
                    MaxAutoTicks = isFirstPane ? 8 : 4,
                    GrowBy = isFirstPane ? new SCIDoubleRange(0.05d, 0.05d) : new SCIDoubleRange(0d, 0d)
                };
                if (!string.IsNullOrWhiteSpace(yAxisTextFormatting))
                {
                    YAxis.TextFormatting = yAxisTextFormatting;
                }
            }

            protected void AddRenderableSeries(SCIRenderableSeriesBase renderableSeries)
            {
                RenderableSeries.Add(renderableSeries);
            }
        }

        private class PricePaneModel : BasePaneModel
        {
            public PricePaneModel(PriceSeries prices) : base(PRICES, "$%.4f", true)
            {
                var stockPrices = new OhlcDataSeries<DateTime, double> { SeriesName = "EUR/USD" };
                stockPrices.Append(prices.TimeData, prices.OpenData, prices.HighData, prices.LowData, prices.CloseData);
                AddRenderableSeries(new SCIFastCandlestickRenderableSeries
                {
                    DataSeries = stockPrices,
                    YAxisId = PRICES,
                    StrokeUpStyle = new SCISolidPenStyle(0xff52cc54, 1f),
                    FillUpBrushStyle = new SCISolidBrushStyle(0xa052cc54),
                    StrokeDownStyle = new SCISolidPenStyle(0xffe26565, 1f),
                    FillDownBrushStyle = new SCISolidBrushStyle(0xd0e26565)
                });

                var maLow = new XyDataSeries<DateTime, double> { SeriesName = "Low Line" };
                maLow.Append(prices.TimeData, prices.CloseData.MovingAverage(50));
                AddRenderableSeries(new SCIFastLineRenderableSeries { DataSeries = maLow, StrokeStyle = new SCISolidPenStyle(0xFFFF3333, 1f), YAxisId = PRICES });

                var maHigh = new XyDataSeries<DateTime, double> { SeriesName = "High Line" };
                maHigh.Append(prices.TimeData, prices.CloseData.MovingAverage(200));
                AddRenderableSeries(new SCIFastLineRenderableSeries { DataSeries = maHigh, StrokeStyle = new SCISolidPenStyle(0xFF33DD33, 1f), YAxisId = PRICES });

                var priceMarker = new SCIAxisMarkerAnnotation
                {
                    Position = stockPrices.YValues.ValueAt(stockPrices.Count - 1).ToComparable(),
                    Style = { BackgroundColor = 0xFFFF3333.ToColor() },
                    YAxisId = PRICES
                };

                var maLowMarker = new SCIAxisMarkerAnnotation
                {
                    Position = maLow.YValues.ValueAt(maLow.Count - 1).ToComparable(),
                    Style = { BackgroundColor = 0xFFFF3333.ToColor() },
                    YAxisId = PRICES
                };

                var maHighMarker = new SCIAxisMarkerAnnotation
                {
                    Position = maHigh.YValues.ValueAt(maHigh.Count - 1).ToComparable(),
                    Style = { BackgroundColor = 0xFF33DD33.ToColor() },
                    YAxisId = PRICES
                };

                Annotations.Add(priceMarker);
                Annotations.Add(maLowMarker);
                Annotations.Add(maHighMarker);
            }
        }

        private class VolumePaneModel : BasePaneModel
        {
            public VolumePaneModel(PriceSeries prices) : base(VOLUME, "", false)
            {
                YAxis.NumberFormatter = new NSNumberFormatter
                {
                    MaximumIntegerDigits = 3,
                    NumberStyle = NSNumberFormatterStyle.Scientific,
                    ExponentSymbol = @"E+"
                };

                var volumePrices = new XyDataSeries<DateTime, double> { SeriesName = "Volume" };
                volumePrices.Append(prices.TimeData, prices.VolumeData.Select(x => (double)x));
                AddRenderableSeries(new SCIFastColumnRenderableSeries
                {
                    DataSeries = volumePrices,
                    YAxisId = VOLUME,
                    FillBrushStyle = new SCISolidBrushStyle(UIColor.White),
                    StrokeStyle = new SCISolidPenStyle(UIColor.White, 1f)
                });

                Annotations.Add(new SCIAxisMarkerAnnotation
                {
                    Position = volumePrices.YValues.ValueAt(volumePrices.Count - 1).ToComparable(),
                    YAxisId = VOLUME,
                });
            }
        }

        private class RsiPaneModel : BasePaneModel
        {
            public RsiPaneModel(PriceSeries prices) : base(RSI, "%.1f", false)
            {
                var rsiSeries = new XyDataSeries<DateTime, double> { SeriesName = "RSI" };
                var xData = prices.TimeData;
                var yData = prices.Rsi(14);

                rsiSeries.Append(xData, yData);
                AddRenderableSeries(new SCIFastLineRenderableSeries { DataSeries = rsiSeries, YAxisId = RSI, StrokeStyle = new SCISolidPenStyle(0xFFC6E6FF, 1f) });

                Annotations.Add(new SCIAxisMarkerAnnotation
                {
                    Position = rsiSeries.YValues.ValueAt(rsiSeries.Count - 1).ToComparable(),
                    YAxisId = RSI
                });
            }
        }

        private class MacdPaneModel : BasePaneModel
        {
            public MacdPaneModel(PriceSeries prices) : base(MACD, "%.2f", false)
            {
                var macdPoints = prices.CloseData.Macd(12, 25, 9);

                var histogramSeries = new XyDataSeries<DateTime, double> { SeriesName = "Histogram" };
                histogramSeries.Append(prices.TimeData, macdPoints.Select(x => x.Divergence));
                AddRenderableSeries(new SCIFastColumnRenderableSeries { DataSeries = histogramSeries, YAxisId = MACD, StrokeStyle = new SCISolidPenStyle(UIColor.White, 1f) });

                var macdSeries = new XyyDataSeries<DateTime, double> { SeriesName = "MACD" };
                macdSeries.Append(prices.TimeData, macdPoints.Select(x => x.Macd), macdPoints.Select(x => x.Signal));
                AddRenderableSeries(new SCIFastBandRenderableSeries
                {
                    DataSeries = macdSeries,
                    YAxisId = MACD,
                    FillBrushStyle = new SCISolidBrushStyle(UIColor.Clear),
                    StrokeStyle = new SCISolidPenStyle(0xffe26565, 1f),
                    FillY1BrushStyle = new SCISolidBrushStyle(UIColor.Clear),
                    StrokeY1Style = new SCISolidPenStyle(0xff52cc54, 1f)
                });

                Annotations.Add(new SCIAxisMarkerAnnotation
                {
                    Position = histogramSeries.YValues.ValueAt(histogramSeries.Count - 1).ToComparable(),
                    YAxisId = MACD
                });
                Annotations.Add(new SCIAxisMarkerAnnotation
                {
                    Position = macdSeries.YValues.ValueAt(macdSeries.Count - 1).ToComparable(),
                    YAxisId = MACD
                });
            }
        }
    }
}