using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
	 public partial class AddDatesView : BaseUserControl
	 {
		  public AddDatesView()
		  {
				InitializeComponent();
				if (!DesignMode) SetTexts();
		  }

		  public void SetInitialDate(DateTime initialDate)
		  {
				dateTimePickerFromDate.Value = initialDate;
		  }

		  private void buttonAdvAddDates_Click(object sender, EventArgs e)
		  {
				if (!ValidateDates()) return;

			var handler = DatesSelected;
				if (handler!=null)
				{
					 handler.Invoke(this,
												 new DateRangeSelectionEventArgs(dateTimePickerFromDate.Value,
																							dateTimePickerToDate.Value));
				}
		  }

		  private bool ValidateDates()
		  {
				return dateTimePickerFromDate.Value.Date <= dateTimePickerToDate.Value.Date;
		  }

		  public event EventHandler<DateRangeSelectionEventArgs> DatesSelected;
		  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
		  public event EventHandler BeforePopup;
		  public event EventHandler PopupClosed;

		  protected override void SetCommonTexts()
		  {
				base.SetCommonTexts();
			  dateTimePickerFromDate.Culture = CultureInfo.CurrentCulture;
			  dateTimePickerToDate.Culture = CultureInfo.CurrentCulture;
				dateTimePickerFromDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
				dateTimePickerToDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
		  }

		  private void dateTimePicker_BeforePopup(object sender, EventArgs e)
		  {
			var handler = BeforePopup;
				if (handler!=null)
				{
					 handler.Invoke(this,EventArgs.Empty);
				}
		  }

		  private void dateTimePicker_PopupClosed(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		  {
			var handler = PopupClosed;
				if (handler != null)
				{
					 handler.Invoke(this, EventArgs.Empty);
				}
		  }

		  private void dateTimePickerFromDate_ValueChanged(object sender, EventArgs e)
		  {
				dateTimePickerToDate.Value = dateTimePickerFromDate.Value;
				dateTimePickerToDate.MinValue = dateTimePickerFromDate.Value;
				dateTimePickerToDate.Calendar.MaxValue = dateTimePickerFromDate.Value.AddDays(31);
				dateTimePickerToDate.MaxValue = dateTimePickerFromDate.Value.AddDays(31);
				dateTimePickerToDate.Calendar.InactiveMonthColor = Color.White;
		  }
	 }
}
