using System.Reflection;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common
{
    public static class GridStyleInfoExtensions
    {
        public static void ResetDefault()
        {
            var defaultField = typeof(GridStyleInfo).GetField("defaultStyle", BindingFlags.Static | BindingFlags.NonPublic);
            defaultField.SetValue(null, null);
        }
    }
}