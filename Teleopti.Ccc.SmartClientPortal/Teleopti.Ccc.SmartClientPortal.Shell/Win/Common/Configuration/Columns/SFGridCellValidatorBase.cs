using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns
{
    /// <summary>
    /// Represents a .
    /// </summary>
    public abstract class SFGridCellValidatorBase<T> : ISFGridCellValidatorBase<T>
    {
        /// <summary>
        /// Gets the <see cref="System.Drawing.Color" /> reference for error cells.
        /// </summary>
        public virtual Color ErrorBackColor
        {
            get { return Color.Red; }
        }

        /// <summary>
        /// Gets or sets the status of the validation to be canceled or not.
        /// </summary>
        public bool Canceled { get; set; }
        /// <summary>
        /// Gets or sets the error message when validation canceled.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Gets the row index of the cell been validated.
        /// </summary>
        public int RowIndex { get; private set; }
        /// <summary>
        /// Gets the column index of the cell been validated.
        /// </summary>
        public int ColIndex { get; private set; }
        /// <summary>
        /// Gets information about current culture.
        /// </summary>
        protected abstract CultureInfo CurrentCulture { get; }

        protected SFGridCellValidatorBase()
        {
            Canceled = false;
            Message = string.Empty;
        }

        public void ValidateCell(GridCellHandledEventArgs e, GridStyleInfo style, T dataItem)
        {
            ColIndex = e.ColIndex;
            RowIndex = e.RowIndex;

            ValidateCell(dataItem);
        	if (!Canceled) return;
        	style.Error = style.CellValue.ToString();
        	style.BackColor = ErrorBackColor;
        	style.CellTipText = Message;
        }

        public abstract bool ValidateCell(T dataItem);
    }
}
