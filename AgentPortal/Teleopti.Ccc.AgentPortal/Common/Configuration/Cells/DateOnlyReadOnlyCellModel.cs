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
    public class DateOnlyReadOnlyCellModel : GridStaticCellModel
    {
        protected DateOnlyReadOnlyCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DateOnlyReadOnlyCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new DateOnlyCellModelRenderer(control, this);
        }

        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return true;
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            // Get culture specified in style, default if null
            //CultureInfo ci = style.GetCulture(true);

            String dateOnlyString = string.Empty;
            if(value == null)
            {
            	style.ReadOnly = true;
				style.CellType = "Static";
                style.Enabled = false;
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

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }
    }
}