﻿using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Celltype for use with numerics and percent
    /// </summary>
    [Serializable]
    public class PercentCellModel : GridTextBoxCellModel, INumericCellModelWithDecimals
    {
        private int _numberOfDecimals;
        private MinMax<double> _minMax = new MinMax<double>(0,1);
        private GridHorizontalAlignment _horizontalAlignement = GridHorizontalAlignment.Right;
        private NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected PercentCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public PercentCellModel(GridModel grid)
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
                {
                    _numberOfDecimals = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the min max. Defaults to 0 and 100.
        /// </summary>
        /// <value>The min max.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-04-18
        /// </remarks>
        public MinMax<double> MinMax
        {
            get { return _minMax; }
            set { _minMax = value;}
        }

        /// <summary>
        /// Gets or sets the horizontal alignement.
        /// </summary>
        /// <value>The horizontal alignement.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        public GridHorizontalAlignment HorizontalAlignment
        {
            get { return _horizontalAlignement; }
            set { _horizontalAlignement = value; }
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
            return new PercentCellRenderer(control, this);
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
            Percent percentage;
            if (string.IsNullOrEmpty(text))
            {
            	style.CellValue = _minMax.Minimum > 0 ? new Percent(_minMax.Minimum) : new Percent(0);
            	return true;
            }
            
			if (!Percent.TryParse(text,out percentage))
                return false;

            if (percentage.Value < _minMax.Minimum || percentage.Value > _minMax.Maximum)
                return false;

            style.HorizontalAlignment = _horizontalAlignement;
            style.CellValue = percentage;
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
            if (value is Percent)
            {
                nfi.PercentDecimalDigits = _numberOfDecimals;
                //return ((Percent)value).ToString(nfi).Replace((char)160, (char)32);
                return ((Percent) value).ToString(nfi);
            }
            else
            {
                return string.Empty;
            }
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

    /// <summary>
    /// Renders the numeric cell
    /// </summary>
    public class PercentCellRenderer : GridTextBoxCellRenderer
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
        public PercentCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
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

            e.Style.HorizontalAlignment = ((PercentCellModel)Model).HorizontalAlignment;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}