
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    public interface IDrawingBehavior
    {
        #region Methods - Instance Member

        void DrawProgressGraphs(IDrawProperties drawProperties);

        void DrawNames(IDrawProperties drawProperties);

        ToolTipInfo SetTooltip(IDrawPositionAndWidth drawPositionAndWidth, int cursorX);

        #endregion

    }
}