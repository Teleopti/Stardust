﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Cell that holds a timespan and displays the totalseconds
    /// </summary>
    [Serializable]
    public class TimeSpanTotalSecondsCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;
        private bool _onlyPositiveValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanTotalSecondsCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected TimeSpanTotalSecondsCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanTotalSecondsCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public TimeSpanTotalSecondsCellModel(GridModel grid)
            : base(grid)
        {
        }

        /// <summary>
        /// Gets or sets the num decimals.
        /// </summary>
        /// <value>The num decimals.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set
            {
                if (value < 0)
                {
                    _numberOfDecimals = 0;
                }
                else
                    if (value > 3)
                    {
                        _numberOfDecimals = 3;
                    }
                    else
                    {
                        _numberOfDecimals = value;
                    }
            }
        }

        public bool OnlyPositiveValues
        {
            get { return _onlyPositiveValues; }
            set { _onlyPositiveValues = value; }
        }

        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeSpanTotalSecondsCellRenderer(control, this);
        }

        /// <summary>
        /// Parses the display text and converts it into a cell value to be stored in the style object.
        /// GridStyleInfo.CultureInfo is used for parsing the string.
        /// </summary>
        /// <param name="style">The <see cref="T:Syncfusion.Windows.Forms.Grid.GridStyleInfo"/> object that holds cell information.</param>
        /// <param name="text">The input text to be parsed.</param>
        /// <param name="textInfo">TextInfo is a hint of who is calling, default is GridCellBaseTextInfo.DisplayText</param>
        /// <returns>
        /// True if value was parsed correctly and saved in style object as <see cref="P:Syncfusion.Windows.Forms.Grid.GridStyleInfo.CellValue"/>; False otherwise.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            double maxValue = TimeSpan.MaxValue.TotalSeconds;
            double minValue = TimeSpan.MinValue.TotalSeconds;

            if (string.IsNullOrEmpty(text))
            {
                style.CellValue = TimeSpan.Zero;
                return true;
            }

            // Make sure text can be coverted to a double
            double d;
            if (!double.TryParse(text, out d))
                return false;

            if (d < 0 && _onlyPositiveValues)
                return false;

            if (d > maxValue || d < minValue)
                return false;

            style.CellValue = TimeSpan.FromSeconds(d);
            return true;
        }

        /// <summary>
        /// Gets the formatted text.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="value">The value.</param>
        /// <param name="textInfo">The text info.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            // Make sure value in cell is TimeSpan
            if (typeof(TimeSpan)!=value.GetType())
                return "";

            double d = ((TimeSpan)value).TotalSeconds;
            
            NumberFormatInfo nfi = (NumberFormatInfo)ci.NumberFormat.Clone();
            nfi.NumberDecimalDigits = NumberOfDecimals;

            //return ((decimal)d).ToString("N", nfi).Replace((char)160, (char)32);
            return ((decimal) d).ToString("N", nfi);

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
		[SecurityCritical]
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
    /// Renders the TimeSpanTotalSecondsCell
    /// </summary>
    public class TimeSpanTotalSecondsCellRenderer : GridTextBoxCellRenderer
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
        public TimeSpanTotalSecondsCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
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