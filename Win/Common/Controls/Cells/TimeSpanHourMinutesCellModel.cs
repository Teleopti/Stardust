﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class TimeSpanHourMinutesCellModel : GridTextBoxCellModel
    {

        protected TimeSpanHourMinutesCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TimeSpanHourMinutesCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeSpanCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            TimeSpan parsedTimeSpan = TimeSpan.MinValue;
            if (!TimeHelper.TryParse(text, out parsedTimeSpan))
            {
                return false;
            }

			if (parsedTimeSpan.Ticks < 0)
				return false;

            style.CellValue = parsedTimeSpan;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            var ci = style.GetCulture(true);

            var ret = string.Empty;
            var day = string.Empty;
            if(value == null)
            {
                style.Enabled = false;
                style.CellType = "Static";
            }
            else if (value.GetType() == typeof(TimeSpan))
            {
                var typedValue = (TimeSpan) value;

				if (typedValue.Ticks < 0)
					return ret;
                
                var min = Convert.ToString(typedValue.Minutes, ci);
                if (min.Length == 1)
                {
                    min = "0" + min;
                }
                if (typedValue.Days > 0)
                {
                    if (typedValue.Days == 24)
                        day = " +1";
                    else
                        day = " +" + typedValue.Days;
                }
                ret = Convert.ToString(typedValue.Hours, ci) + ci.DateTimeFormat.TimeSeparator + min + day;
            }
            return ret;
        }
        
        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-08
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Renders the TimeSpanCell
    /// </summary>
    public class TimeSpanCellModelRenderer : GridTextBoxCellRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public TimeSpanCellModelRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        /// <summary>
        /// Allows custom formatting of a cell by changing its style object.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// 	<see cref="M:Syncfusion.Windows.Forms.Grid.GridCellRendererBase.OnPrepareViewStyleInfo(Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventArgs)"/> is called from <see cref="E:Syncfusion.Windows.Forms.Grid.GridControlBase.PrepareViewStyleInfo"/>
        /// in order to allow custom formatting of
        /// a cell by changing its style object.
        /// <para/>
        /// Set the cancel property true if you want to avoid
        /// the assoicated cell renderers object <see cref="M:Syncfusion.Windows.Forms.Grid.GridCellRendererBase.OnPrepareViewStyleInfo(Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventArgs)"/>
        /// method to be called.<para/>
        /// Changes made to the style object will not be saved in the grid nor cached. This event
        /// is called every time a portion of the grid is repainted and the specified cell belongs
        /// to the invalidated region of the window that needs to be redrawn.<para/>
        /// Changes to the style object done at this time will also not be reflected when accessing
        /// cells though the models indexer. See <see cref="E:Syncfusion.Windows.Forms.Grid.GridModel.QueryCellInfo"/>.<para/>
        /// 	<note type="note">Do not change base style or cell type at this time.</note>
        /// </remarks>
        /// <seealso cref="T:Syncfusion.Windows.Forms.Grid.GridPrepareViewStyleInfoEventHandler"/>
        /// <seealso cref="M:Syncfusion.Windows.Forms.Grid.GridControlBase.GetViewStyleInfo(System.Int32,System.Int32)"/>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            // This is the place to override settings deririved from the grid
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}