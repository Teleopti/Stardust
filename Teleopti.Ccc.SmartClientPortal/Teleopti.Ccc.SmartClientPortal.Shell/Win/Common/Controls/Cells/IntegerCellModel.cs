using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    /// <summary>
    /// Celltype for use with integers only
    /// </summary>
    [Serializable]
    public class IntegerCellModel : GridTextBoxCellModel
    {
        protected IntegerCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
            HorizontalAlignment = GridHorizontalAlignment.Right;
		}

        public IntegerCellModel(GridModel grid)
            : base(grid)
        {
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
            HorizontalAlignment = GridHorizontalAlignment.Right;
        }

        public GridHorizontalAlignment HorizontalAlignment { get; set; }

        public bool OnlyPositiveValues { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new IntegerCellRenderer(control, this);
        }
		
        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            if (string.IsNullOrEmpty(text))
            {
                style.CellValue = 0;
                return true;
            }
            // Make sure value in cell can be coverted to an int
            int d;
            if (!int.TryParse(text, out d))
                return false;

            if (d < 0 && OnlyPositiveValues)
                return false;
            if (d < MinValue || d > MaxValue)
                return false;

            style.HorizontalAlignment = HorizontalAlignment;
            style.CellValue = d;
	        
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            CultureInfo ci = style.GetCulture(true);

            // Make sure value in cell can be coverted to a double
            int d;
            if (!int.TryParse(value.ToString(), out d))
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
    public class IntegerCellRenderer : GridTextBoxCellRenderer
    {
        public IntegerCellRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = ((IntegerCellModel)Model).HorizontalAlignment;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }

	    protected override void OnBeginEdit()
	    {
		    TextBox.SelectAll();
	    }
    }
}