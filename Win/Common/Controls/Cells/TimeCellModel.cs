using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    /// <summary>
    /// Cell type that displays time as hh:mm e.g 47:23
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-01-09
    /// </remarks>
    [Serializable]
    public class TimeCellModel : GridStaticCellModel
    {
        /// <overload>
        /// Initializes a new <see cref="TimeCellModel"/> object.
        /// </overload>
        /// <summary>
        /// Initializes a new <see cref="TimeCellModel"/> from a serialization stream.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info. </param>
        protected TimeCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TimeCellModel"/> object 
        /// and stores a reference to the <see cref="GridModel"/> this cell belongs to.
        /// </summary>
        /// <param name="grid">The <see cref="GridModel"/> for this cell model.</param>	
        /// <remarks>
        /// You typically access cell models through the <see cref="GridModel.CellModels"/>
        /// property of the <see cref="GridModel"/> class.
        /// </remarks>
        public TimeCellModel(GridModel grid)
            : base(grid)
        {
        }

        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-09
        /// </remarks>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimeCellRenderer(control, this);
        }

        /// <summary>
        /// This is called from GridStyleInfo.GetFormattedText.
        /// GridStyleInfo.CultureInfo is used for conversion to string.
        /// </summary>
        /// <param name="style">The <see cref="T:Syncfusion.Windows.Forms.Grid.GridStyleInfo"/> object that holds cell information.</param>
        /// <param name="value">The value to format.</param>
        /// <param name="textInfo">TextInfo is a hint of who is calling, default is GridCellBaseTextInfo.DisplayText.</param>
        /// <returns>The formatted text for the given value.</returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {

            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            // Make sure value in cell is TimeSpan
            if (typeof(TimeSpan) != value.GetType())
                return "";

            String ret = string.Empty;
            if (value.GetType() == typeof(TimeSpan))
            {
                TimeSpan typedValue = (TimeSpan)value;

                if (typedValue.TotalMilliseconds <= 0)
                    return " - ";

                int minutes = Math.Abs(typedValue.Minutes);
                
                string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
                if (min.Length == 1)
                {
                    min = "0" + min;
                }

                ret = Convert.ToString((int)typedValue.TotalHours, CultureInfo.CurrentCulture) + ci.DateTimeFormat.TimeSeparator + min;
            }
            return ret;
        }
    }

    /// <summary>
    /// Implements the renderer part
    /// </summary>
    public class TimeCellRenderer : GridStaticCellRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCellRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-09
        /// </remarks>
        public TimeCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
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
        /// Created date: 2008-01-03
        /// </remarks>
        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}