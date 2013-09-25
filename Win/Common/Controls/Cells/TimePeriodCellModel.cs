using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    public class TimePeriodCellButton : GridCellButton
    {
        static readonly GridIconPaint iconPainter = new GridIconPaint("CellButtons.", typeof(TimePeriodCellButton).Assembly);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriodCellButton"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        public TimePeriodCellButton(GridStaticCellRenderer control)
            : base(control)
        {
        }
        /// <summary>
        /// Draws the cell button element at the specified row and column index.
        /// </summary>
        /// <param name="g">The <see cref="T:System.Drawing.Graphics"/> context of the canvas.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="bActive">True if this is the active current cell; False otherwise.</param>
        /// <param name="style">The <see cref="T:Syncfusion.Windows.Forms.Grid.GridStyleInfo"/> object that holds cell information.</param>
        /// <override/>
        public override void Draw(Graphics g, int rowIndex, int colIndex, bool bActive, GridStyleInfo style)
        {
            base.Draw(g, rowIndex, colIndex, bActive, style);

            // draw the button
            bool hovering = IsHovering(rowIndex, colIndex);
            bool mouseDown = IsMouseDown(rowIndex, colIndex);
            bool disabled = !style.Clickable;

            ButtonState buttonState = ButtonState.Normal;
            if (disabled)
                buttonState |= ButtonState.Inactive | ButtonState.Flat;

            else if (!hovering && !mouseDown)
                buttonState |= ButtonState.Flat;

            Point ptOffset = Point.Empty;
            if (mouseDown)
            {
                ptOffset = new Point(1, 1);
                buttonState |= ButtonState.Pushed;
            }
            DrawButton(g, Bounds, buttonState, style);

            // Instead of using GridIconPaint you could simple use Image.Draw here
            // with an existing bitmap. GridIconPaint is convenient because it lets
            // us easily draw over existing background and replace the black color
            // in the bitmap with a different color.
            iconPainter.PaintIcon(g, Bounds, ptOffset, Resources.ccc_BrowseDots, Color.Black);
        }
    }

    /// <summary>
    /// Implements the data/model part 
    /// </summary>
    [Serializable]
    public class TimePeriodCellModel : GridStaticCellModel
    {
        /// <overload>
        /// Initializes a new <see cref="TimePeriodCellModel"/> object.
        /// </overload>
        /// <summary>
        /// Initializes a new <see cref="TimePeriodCellModel"/> from a serialization stream.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info. </param>
        protected TimePeriodCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ButtonBarSize = new Size(20, 20);
        }

        /// <summary>
        /// Initializes a new <see cref="TimePeriodCellModel"/> object 
        /// and stores a reference to the <see cref="GridModel"/> this cell belongs to.
        /// </summary>
        /// <param name="grid">The <see cref="GridModel"/> for this cell model.</param>	
        /// <remarks>
        /// You typically access cell models through the <see cref="GridModel.CellModels"/>
        /// property of the <see cref="GridModel"/> class.
        /// </remarks>
        public TimePeriodCellModel(GridModel grid)
            : base(grid)
        {
        }


        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new TimePeriodCellRenderer(control, this);
        }

        /// <summary>
        /// Called when [query preffered client size].
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="style">The style.</param>
        /// <param name="queryBounds">The grid query bounds.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        protected override Size OnQueryPrefferedClientSize(Graphics g, int rowIndex, int colIndex, GridStyleInfo style, GridQueryBounds queryBounds)
        {
            Size size = base.OnQueryPrefferedClientSize(g, rowIndex, colIndex, style, queryBounds);
            return new Size(size.Width + 5, size.Height); // base method already consides ButtonBarSize, but let's add some more pixels here.
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
        /// Created date: 2008-01-03
        /// </remarks>
        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            IList<TimePeriod> typedValue = new List<TimePeriod>();
            if (TimeHelper.TryParseTimePeriods(text, out typedValue))
            {
                style.CellValue = typedValue;
                return true;
            }

            if (text.Trim() == UserTexts.Resources.Closed)
            {
                style.CellValue = typedValue;
                return true;
            }

            return false;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            StringBuilder ret = new StringBuilder();
            if (value.GetType() == typeof(List<TimePeriod>)|| value.GetType() == typeof(System.Collections.ObjectModel.ReadOnlyCollection<TimePeriod>))
            {
                IList<TimePeriod> typedValue = (IList<TimePeriod>)value;
                if (typedValue.Count == 0)
                {
                    return UserTexts.Resources.Closed;
                }

                ret.Append(typedValue[0].ToShortTimeString());
                //Multiple openHours should not be implemented yet
                // Yes it should /Micke
                for (int i = 1; i < typedValue.Count; i++)
                {
                    ret.Append(string.Concat(";", typedValue[i].ToShortTimeString()));
                }
            }
            else
            {
                ret.Append(UserTexts.Resources.Closed);
            }

            return ret.ToString();
        }
    }

    /// <summary>
    /// Implements the renderer part
    /// </summary>
    public class TimePeriodCellRenderer : GridStaticCellRenderer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriodCellRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        public TimePeriodCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
            AddButton(new TimePeriodCellButton(this));
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
            // Here you can adjust settings that override the default settings from the grid
            e.Style.ShowButtons = GridShowButtons.Show;
            e.Style.Clickable = true;
            e.Style.TextAlign = GridTextAlign.Left;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }

        /// <summary>
        /// This method is called from PerformLayout to calculate the client rectangle given
        /// the inner rectangle of a cell and any boundaries of cell buttons.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="style">The <see cref="T:Syncfusion.Windows.Forms.Grid.GridStyleInfo"/> object that holds cell information.</param>
        /// <param name="innerBounds">The <see cref="T:System.Drawing.Rectangle"/> with the inner bounds of a cell.</param>
        /// <param name="buttonsBounds">An array of <see cref="T:System.Drawing.Rectangle"/> with bounds for each cell button element.</param>
        /// <returns>
        /// A <see cref="T:System.Drawing.Rectangle"/> with the bounds.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-03
        /// </remarks>
        protected override Rectangle OnLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds, Rectangle[] buttonsBounds)
        {
            Rectangle rightArea;
            //TraceUtil.TraceCurrentMethodInfo(rowIndex, colIndex, style, innerBounds, buttonsBounds);
            // Here you specify where the button should be drawn within the cell.
            if (Grid.RightToLeft == RightToLeft.Yes)
            {
                rightArea =
                    Rectangle.FromLTRB(innerBounds.Left, innerBounds.Top, innerBounds.Left + 20, innerBounds.Bottom);
            }
            else
            {
                rightArea =
                    Rectangle.FromLTRB(innerBounds.Right - 20, innerBounds.Top, innerBounds.Right, innerBounds.Bottom);
            }
            buttonsBounds[0] = GridUtil.CenterInRect(rightArea, new Size(20, 20));

            // Here you return the area where the text should be drawn/edited.
            innerBounds.Width -= 10;
            return innerBounds;
        }

    }
}