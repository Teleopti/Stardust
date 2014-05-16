using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Cells
{
    [Serializable]
    public class DateOnlyCellModel : GridStaticCellModel
    {
        protected DateOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DateOnlyCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new DateOnlyCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            style.CellValue = text;
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            //CultureInfo ci = style.GetCulture(true);

            String dateOnlyString = string.Empty;
            if(value == null)
            {
                style.Enabled = false;
                style.CellType = "Static";
            }
            else if (value.GetType() == typeof(DateOnlyDto))
            {
                DateOnlyDto typedValue = (DateOnlyDto)value;
                CultureInfo ci = (StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.CultureLanguageId.HasValue
                       ? CultureInfo.GetCultureInfo(StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.CultureLanguageId.Value)
                       : CultureInfo.CurrentCulture).FixPersianCulture();

                DateTimeFormatInfo info = ci.DateTimeFormat;
                dateOnlyString = typedValue.DateTime.ToString("d", info);
            }
            return dateOnlyString;
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
    /// Renders the TimeSpanCell
    /// </summary>
    public class DateOnlyCellModelRenderer : GridStaticCellRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyCellModelRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public DateOnlyCellModelRenderer(GridControlBase grid, GridCellModelBase cellModel)
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
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Left;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}