using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Cell type that displays time as hh:mm e.g 43:23:20
    /// </summary>
    [Serializable]
    public class TimeSpanLongHourMinuteSecondCellModel : GridTextBoxCellModel
    {
        private bool _onlyPositiveValues;

        public bool OnlyPositiveValues
        {
            get { return _onlyPositiveValues; }
            set { _onlyPositiveValues = value; }
        }
        public TimeSpanLongHourMinuteSecondCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected TimeSpanLongHourMinuteSecondCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeSpanLongHourMinutesCellRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            TimeSpan timeValue;
            if (TimeHelper.TryParseLongHourString(text, out timeValue, TimeFormatsType.HoursMinutesSeconds))
            {
                if (_onlyPositiveValues && timeValue.TotalSeconds < 0)
                    return false;
                style.CellValue = timeValue;
            }
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            String ret = string.Empty;
            if (value is TimeSpan)
            {
                ret = TimeHelper.GetLongHourMinuteSecondTimeString((TimeSpan)value, ci);
            }
            return ret;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }
    }


    /// <summary>
    /// Renders the TimeSpanTotalSecondsCell
    /// </summary>
    public class TimeSpanLongHourMinuteSecondCellRenderer : GridTextBoxCellRenderer
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
        public TimeSpanLongHourMinuteSecondCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
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