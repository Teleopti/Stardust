

using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common
{
    public static class GuiHelper
    {

        public static void SetStyle(GridStyleInfo gridStyleInfo)
        {

            switch (gridStyleInfo.CellType)
            {
                case "TimeSpanTotalSecondsReadOnlyCell":
                    gridStyleInfo.Font.FontStyle = GuiSettings.ClosedCellFontStyle();
                    gridStyleInfo.TextColor = GuiSettings.ClosedCellFontColor();

                    break;
                case "NumericReadOnlyCell":
                    gridStyleInfo.Font.FontStyle = GuiSettings.ClosedCellFontStyle();
                    gridStyleInfo.TextColor = GuiSettings.ClosedCellFontColor();
                    break;
                case "PercentReadOnlyCell":
                    gridStyleInfo.Font.FontStyle = GuiSettings.ClosedCellFontStyle();
                    gridStyleInfo.TextColor = GuiSettings.ClosedCellFontColor();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Sets a tool tip on a control.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="text">The text.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-26
        /// </remarks>
        public static void SetToolTip(Control button, string text)
        {
            ToolTip tip = new ToolTip();
            tip.SetToolTip(button, text);
        }
    }
}