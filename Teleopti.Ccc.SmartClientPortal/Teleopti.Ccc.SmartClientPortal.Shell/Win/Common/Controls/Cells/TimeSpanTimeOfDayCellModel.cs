using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanTimeOfDayCellModel : GridTextBoxCellModel
    {
        protected TimeSpanTimeOfDayCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanTimeOfDayCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeCellModelRenderer(control, this);
        }

        public bool AllowEmptyCell { get; set; }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            TimeSpan theTimeSpan;

            if (AllowEmptyCell && string.IsNullOrWhiteSpace(text))
            {
                style.CellValue = null;
                return true;
            }

            style.CellValue = TimeHelper.TryParse(text, out theTimeSpan) ? theTimeSpan : TimeSpan.Zero;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

			if (value == null)
			{
				style.Enabled = false;
				style.CellType = "Static";
				return string.Empty;
			}

            TimeSpan timeSpan = (TimeSpan) value;
            return TimeHelper.TimeOfDayFromTimeSpan(timeSpan, ci);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}