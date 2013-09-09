using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Enables to erase the value and leave cell blank
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-05-06
    /// </remarks>
    [Serializable]
    public class TimeSpanHourMinuteCanBeEmptyCellModel : GridTextBoxCellModel
    {
        protected TimeSpanHourMinuteCanBeEmptyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanHourMinuteCanBeEmptyCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeSpanEmptyCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
			if (!string.IsNullOrEmpty(text))
			{
				TimeSpan parsedTimeSpan;
				if (!TimeHelper.TryParse(text, out parsedTimeSpan))
				{
					return false;
				}
				style.CellValue = text;

			}
			else
				style.CellValue = null;
			return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
			// Get culture specified in style, default if null
			var ci = style.GetCulture(true);

			var ret = string.Empty;

			TimeSpan parsedTimeSpan;
			if (!TimeHelper.TryParse(Convert.ToString(value, ci), out parsedTimeSpan))
				return ret;

			if (value == null)
			{
				style.Enabled = false;
				style.CellType = "Static";
			}
			else
			{
				var min = Convert.ToString(parsedTimeSpan.Minutes, ci);
				if (min.Length == 1)
				{
					min = "0" + min;
				}
				ret = Convert.ToString(parsedTimeSpan.Hours, ci) + ci.DateTimeFormat.TimeSeparator + min;

			}

			return ret;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}