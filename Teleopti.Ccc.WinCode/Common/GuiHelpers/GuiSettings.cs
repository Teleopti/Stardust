using System.Drawing;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    public static class GuiSettings
    {
        private static readonly ThemeSettings settings = ThemeSettings.Default;

        public static FontStyle ClosedCellFontStyle()
        {
            return FontStyle.Regular;
        }

        public static Color ClosedCellFontColor()
        {
            return settings.GridClosedCellFontColor;
        }
    }
}
