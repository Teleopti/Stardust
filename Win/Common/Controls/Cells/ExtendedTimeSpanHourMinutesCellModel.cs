using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class ExtendedTimeSpanHourMinutesCellModel : GridTextBoxCellModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTimeSpanHourMinutesCellModel"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ExtendedTimeSpanHourMinutesCellModel(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTimeSpanHourMinutesCellModel"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ExtendedTimeSpanHourMinutesCellModel(GridModel model) : base(model)
        {
            
        }

        /// <summary>
        /// Creates the grid cell renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeSpanCellModelRenderer(control, this);
        }

        /// <summary>
        /// Applies the formatted text.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="text">The text.</param>
        /// <param name="textInfo">The text info.</param>
        /// <returns></returns>
        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            var timeValue = TimeSpan.MinValue;
            var ci = style.GetCulture(true);
                        
            var parts = Convert.ToString(text, ci).Split('+');
            if(parts.Length > 0)
            {
                if (!TimeHelper.TryParse(parts[0], out timeValue))
                    return false;

				if (timeValue.Ticks < 0)
					return false;

                if(parts.Length >= 2)
                {
                    int numericValue;
                    if(int.TryParse(parts[1], out numericValue))
                    {
                        if (numericValue == 1)
                            timeValue = new TimeSpan(1, timeValue.Hours, timeValue.Minutes, timeValue.Seconds);                        
                        else
                            timeValue = new TimeSpan(timeValue.Hours, timeValue.Minutes, timeValue.Seconds);                        
                    }
                }
            }
            style.CellValue = timeValue;
            return true;
        }

        /// <summary>
        /// Gets the formatted text.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="value">The value.</param>
        /// <param name="textInfo">The text info.</param>
        /// <returns></returns>
        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            var ci = style.GetCulture(true);

            var ret = string.Empty;
            if (value == null)
            {
                style.Enabled = false;
                style.CellType = "Static";
            }
            else if (value.GetType() == typeof(TimeSpan))
            {
                var typedValue = (TimeSpan)value;

				if (typedValue.Ticks < 0)
					return ret;

                string min = Convert.ToString(typedValue.Minutes, ci);
                if (min.Length == 1)
                {
                    min = "0" + min;
                }
                ret = Convert.ToString(typedValue.Hours, ci) + ci.DateTimeFormat.TimeSeparator + min;
                if (typedValue.Days == 1)
                    ret += "+1";

            }
            return ret;
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}
