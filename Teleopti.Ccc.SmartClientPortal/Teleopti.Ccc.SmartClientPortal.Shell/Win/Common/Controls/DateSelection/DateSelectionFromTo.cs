using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
	public partial class DateSelectionFromTo : IDateSelectionControl
	{
		public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

		public event EventHandler ValueChanged;

		public event EventHandler ButtonClickedNoValidation;

		public DateSelectionFromTo()
		{
			InitializeComponent();

			var minPeriod = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
																		 new DateOnly(DateHelper.MaxSmallDateTime));
			dateTimePickerAdvWorkAStartDate.SetAvailableTimeSpan(minPeriod);
			dateTimePickerAdvWorkEndPeriod.SetAvailableTimeSpan(minPeriod);

			dateTimePickerAdvWorkAStartDate.Value = DateTime.Today;
			dateTimePickerAdvWorkEndPeriod.Value = DateTime.Today;

			if (!DesignMode) SetTexts();

			_errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetCulture(CultureInfo cultureInfo)
		{
			dateTimePickerAdvWorkAStartDate.SetCultureInfoSafe(cultureInfo);
			dateTimePickerAdvWorkEndPeriod.SetCultureInfoSafe(cultureInfo);
		}

		public bool HideNoneButtons
		{
			get => !dateTimePickerAdvWorkEndPeriod.NoneButtonVisible &&
				   !dateTimePickerAdvWorkAStartDate.NoneButtonVisible;
			set
			{
				dateTimePickerAdvWorkAStartDate.NoneButtonVisible = !value;
				dateTimePickerAdvWorkEndPeriod.NoneButtonVisible = !value;
			}
		}

		[Browsable(false)]
		public bool IsWorkPeriodValid => WorkPeriodStart <= WorkPeriodEnd;

		[Browsable(false)]
		public DateOnly WorkPeriodStart
		{
			get => new DateOnly(dateTimePickerAdvWorkAStartDate.Value.Date);
			set
			{
				dateTimePickerAdvWorkAStartDate.Value = value.Date;
				dateTimePickerAdvWorkAStartDate.RefreshFields();
			}
		}

		[Browsable(false)]
		public DateOnly WorkPeriodEnd
		{
			get => new DateOnly(dateTimePickerAdvWorkEndPeriod.Value.Date);
			set => dateTimePickerAdvWorkEndPeriod.Value = value.Date;
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string LabelDateSelectionText
		{
			get => labelTargetPeriod.Text;
			set => labelTargetPeriod.Text = value;
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string LabelDateSelectionToText
		{
			get => labelTargetPeriodTo.Text;
			set => labelTargetPeriodTo.Text = value;
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string ButtonApplyText
		{
			get => btnApplyChangedPeriod.Text;
			set => btnApplyChangedPeriod.Text = value;
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string NullString
		{
			get => dateTimePickerAdvWorkAStartDate.NullString;
			set
			{
				dateTimePickerAdvWorkAStartDate.NullString = value;
				dateTimePickerAdvWorkEndPeriod.NullString = value;
			}
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string TodayButtonText
		{
			get => dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text;
			set
			{
				dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text = value;
				dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Text = value;
			}
		}

		[Browsable(true), Category("Teleopti Texts"), Localizable(true)]
		public string NoneButtonText
		{
			get => dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text;
			set
			{
				dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text = value;
				dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Text = value;
			}
		}

		public override bool HasHelp => false;

		private void btnApplyChangedPeriodClick(object sender, EventArgs e)
		{
			triggerWorkPeriodChanged();
		}

		private void triggerWorkPeriodChanged()
		{
			var args = new DateRangeChangedEventArgs(
						new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates()));
			DateRangeChanged?.Invoke(this, args);
			ButtonClickedNoValidation?.Invoke(this, args);
		}

		public void ShowWarning()
		{
			if (IsWorkPeriodValid == false)
			{
				using (var warning = new TimedWarningDialog())
				{
					warning.WarningShownInSeconds = 2;
					warning.WarningMessageShown = "WARNING! \n the startdate is after enddate!";
					warning.WarningShownNearThisControl = labelTargetPeriodTo;
					warning.ShowDialog(this);
				}
			}
		}

		private void dateTimePickerAdvWorkEndPeriodValueChanged(object sender, EventArgs e)
		{
			invokeValueChanged(e);
		}

		private void dateTimePickerAdvWorkStartDateValueChanged(object sender, EventArgs e)
		{
			invokeValueChanged(e);
		}

		private void invokeValueChanged(EventArgs e)
		{
			ValueChanged?.Invoke(this, e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			dateTimePickerAdvWorkAStartDate.ValueChanged += dateTimePickerAdvWorkStartDateValueChanged;
			dateTimePickerAdvWorkEndPeriod.ValueChanged += dateTimePickerAdvWorkEndPeriodValueChanged;
		}

		[Browsable(true), DefaultValue(true), Category("Teleopti Behavior")]
		public bool ShowApplyButton
		{
			get => btnApplyChangedPeriod.Visible;
			set => btnApplyChangedPeriod.Visible = value;
		}

		[Browsable(false)]
		public IList<DateOnlyPeriod> GetSelectedDates()
		{
			IList<DateOnlyPeriod> selectedDates = new List<DateOnlyPeriod>();
			if (IsWorkPeriodValid)
			{
				selectedDates.Add(new DateOnlyPeriod(WorkPeriodStart, WorkPeriodEnd));
				return selectedDates;
			}
			
			return selectedDates;
		}

		public void SetErrorOnEndTime(string error)
		{
			_errorProvider.SetError(dateTimePickerAdvWorkEndPeriod, error);
			_errorProvider.SetIconPadding(dateTimePickerAdvWorkEndPeriod, -35);
		}
	}
}