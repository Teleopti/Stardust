using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
	public partial class DateNavigateControl : BaseUserControl
	{
		public DateNavigateControl()
		{
			InitializeComponent();
			dateTimePickerAdv1.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdv1.PopupClosed += PopupClosed;
		}

		public event EventHandler<CustomEventArgs<DateOnly>> SelectedDateChanged;
		public event EventHandler ClosedPopup;
		
		private void ForwardDayButtonClicked(object sender, EventArgs e)
		{
			dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(1d);
			InvokeClosedPopup();
		}

		private void BackDayButtonClick(object sender, EventArgs e)
		{
			dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-1d);
			InvokeClosedPopup();
		}

		private void ForwardWeekButtonClick(object sender,EventArgs e)
		{
			dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(7d);
			InvokeClosedPopup();
		}

		private void BackwardWeekButtonClick(object sender, EventArgs e)
		{
			dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-7d);
			InvokeClosedPopup();
		}

		private void DropDownDateSelected(object sender, EventArgs e)
		{
			InvokeDateChanged();
		}

		private void PopupClosed(object sender, EventArgs e)
		{
			InvokeClosedPopup();
		}

		private void InvokeDateChanged()
		{
			var handler = SelectedDateChanged;
			if (handler!=null)
			{
				handler.Invoke(this, new CustomEventArgs<DateOnly>(new DateOnly(dateTimePickerAdv1.Value)));
			}
		}

		private void InvokeClosedPopup()
		{
			var handler = ClosedPopup;
			if (handler != null)
			{
				handler.Invoke(this, new EventArgs());
			}
		}

		public void SetAvailableTimeSpan(DateOnlyPeriod dateTimePair)
		{
			dateTimePickerAdv1.SetAvailableTimeSpan(dateTimePair);
			dateTimePickerAdv1.Value = dateTimePickerAdv1.MinValue;
		}

		public void SetSelectedDateNoInvoke(DateOnly dateTime)
		{
			dateTimePickerAdv1.ValueChanged -= DropDownDateSelected;
			dateTimePickerAdv1.Value = dateTime.Date;
			dateTimePickerAdv1.ValueChanged += DropDownDateSelected;
		}

		public void SetSelectedDate(DateOnly dateTime )
		{
			dateTimePickerAdv1.ValueChanged -= DropDownDateSelected;
			dateTimePickerAdv1.Value = dateTime.Date;
			dateTimePickerAdv1.ValueChanged += DropDownDateSelected;

			InvokeDateChanged();
		}

		public DateOnly SelectedDate
		{
			get { return new DateOnly(dateTimePickerAdv1.Value); }
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}
	}
}