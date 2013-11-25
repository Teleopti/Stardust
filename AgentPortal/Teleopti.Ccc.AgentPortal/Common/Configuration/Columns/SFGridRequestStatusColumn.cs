using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridRequestStatusColumn<T> : SFGridColumnBase<T>
    {
        public SFGridRequestStatusColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "DescriptionNameCell";
            
			object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
	        var status = value as StatusDisplay;

			e.Style.CellValue = GetDescription(status, value);
            e.Style.Interior = GetBrushInfo(status);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
			PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }

        private string GetDescription(StatusDisplay currentItem,object value)
        {
	        if (value == null) return string.Empty;

            return currentItem!=null ? currentItem.DisplayText : value.ToString();
        }

	    private BrushInfo GetBrushInfo(StatusDisplay currentItem)
	    {
		    var brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(150, Color.Yellow));
		    if (currentItem == null) return brushInfo;

		    var requestStatus = currentItem.RequestStatus;
		    if (requestStatus == RequestStatusDto.Approved)
		    {
			    brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(180, Color.LimeGreen));
		    }
		    else if (requestStatus == RequestStatusDto.Denied)
		    {
			    brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(180, Color.Crimson));
		    }
		    return brushInfo;
	    }
    }
}