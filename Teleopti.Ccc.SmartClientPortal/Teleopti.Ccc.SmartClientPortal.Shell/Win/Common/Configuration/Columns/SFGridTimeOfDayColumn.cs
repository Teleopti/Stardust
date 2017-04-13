using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridTimeOfDayColumn<T> : SFGridColumnBase<T>
    {
        public SFGridTimeOfDayColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Style.CellType = "TimeOfDayCell";
            
            e.Style.CellValue = value;
        }

        
        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
			try
			{
				PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
            }
			catch (System.Reflection.TargetInvocationException ex)
			{
				if (ex.InnerException is ArgumentOutOfRangeException)
				{
					e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);

					string message = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.TheEnteredIsIllegal, HeaderText);
					string header = UserTexts.Resources.Invalid + HeaderText;

					ViewBase.ShowErrorMessage(message, header);
				}
			}
        }
    }
}