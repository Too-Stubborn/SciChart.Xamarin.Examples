using Java.Lang;

namespace SciChart.Charting.Visuals.RenderableSeries.Tooltips
{
    public partial class HorizontallyStackedSeriesRolloverTooltip
    {
        public override Object Func(Object arg)
        {
            return (Object) Func((IStackedRenderableSeries) arg);
        }
    }
}