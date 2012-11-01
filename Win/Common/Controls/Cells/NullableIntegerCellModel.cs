﻿using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Celltype for use with nullable integers only
    /// </summary>
    [Serializable]
    class NullableIntegerCellModel : GridTextBoxCellModel
    {
        private GridHorizontalAlignment _horizontalAlignement = GridHorizontalAlignment.Right;
        private bool _onlyPositiveValues;
        private int _maxValue = int.MaxValue;
        private int _minValue = int.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableIntegerCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected NullableIntegerCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableIntegerCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public NullableIntegerCellModel(GridModel grid)
            : base(grid)
        {
        }

        /// <summary>
        /// Gets or sets the horizontal alignement.
        /// </summary>
        /// <value>The horizontal alignement.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public GridHorizontalAlignment HorizontalAlignment
        {
            get { return _horizontalAlignement; }
            set { _horizontalAlignement = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [only positive values].
        /// </summary>
        /// <value><c>true</c> if [only positive values]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-15
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool OnlyPositiveValues
        {
            get { return _onlyPositiveValues; }
            set { _onlyPositiveValues = value; }
        }
        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>The max value.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-15
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }
        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-15
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
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
            return new NullableIntegerCellRenderer(control, this);
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
            if (string.IsNullOrEmpty(text))
            {
                style.CellValue = "";
                return true;
            }
            // Make sure value in cell can be coverted to an int
            int d;
            if (!int.TryParse(text, out d))
                return false;

            if (d < 0 && _onlyPositiveValues)
                return false;
            if (d < _minValue || d > _maxValue)
                return false;

            style.HorizontalAlignment = _horizontalAlignement;
            style.CellValue = d;

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
            if (value == null)
                return "";
            // Make sure value in cell can be coverted to a double
            int d;
            if (!int.TryParse(value.ToString(), out d))
                return "";

            if(OnlyPositiveValues && d<0)
                return "";

            return d.ToString(ci);
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
    /// Renders the integer cell
    /// </summary>
    public class NullableIntegerCellRenderer : GridTextBoxCellRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerCellRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public NullableIntegerCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
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
            e.Style.HorizontalAlignment = ((NullableIntegerCellModel)Model).HorizontalAlignment;//GridHorizontalAlignment.Right;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}
