
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
            e.Style.CellValue = GetDescription(currentItem);
            e.Style.Interior = GetBrushInfo(currentItem);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            Description description = new Description((string)e.Style.CellValue);
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, new Description(description.Name, description.ShortName));
        }

        private Description GetDescription(T currentItem)
        {
            Description description;
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (value == null) return new Description();

            if (value.GetType() == typeof(StatusDisplay))
                description = new Description(((StatusDisplay)value).DisplayText);
            else
                description = new Description(value.ToString(), System.String.Empty);
            return description;
        }

        private BrushInfo GetBrushInfo(T currentItem)
        {
            BrushInfo brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(150,Color.Yellow));
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            if (value == null) return brushInfo;

            if (value.GetType() == typeof(StatusDisplay))
            {
                RequestStatusDto requestStatus = ((StatusDisplay)value).RequestStatus;
                if (requestStatus==RequestStatusDto.Approved)
                {
                    brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(180,Color.LimeGreen));
				}
				else if (requestStatus == RequestStatusDto.Denied)
                {
                    brushInfo = new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(180,Color.Crimson));
                }
            }
            return brushInfo;
        }
    }
}