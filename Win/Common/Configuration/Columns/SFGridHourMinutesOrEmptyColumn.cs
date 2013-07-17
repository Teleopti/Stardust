using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridHourMinutesOrEmptyColumn<T> : SFGridColumnBase<T>
    {
        public SFGridHourMinutesOrEmptyColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

		public RightToLeft RightToLeft { set; get; }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            object value = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
			e.Style.CellType = "HourMinutesEmpty"; //"TimeOfDayCell";
            e.Style.CellValue = value ?? null;
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
					string header = UserTexts.Resources.Illegal + HeaderText;

					ViewBase.ShowErrorMessage(message, header);
				}
			}
        }
    }
}