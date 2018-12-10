using System;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	/// <summary>
	/// Manages recurrent meetings
	/// </summary>
	public partial class MeetingRecurrenceView : BaseDialogForm, IMeetingRecurrenceView
	{
		private readonly INotifyComposerMeetingChanged _composer;
		private readonly MeetingRecurrencePresenter _meetingRecurrencePresenter;
		
		protected MeetingRecurrenceView()
		{
			InitializeComponent();

			if (DesignMode) return;
			outlookTimePickerStart.CreateAndBindList();
			outlookTimePickerEnd.CreateAndBindList();
			outlookTimePickerStart.MaxValue = TimeSpan.FromDays(1);
			outlookTimePickerEnd.MaxValue = TimeSpan.FromDays(1);
			SetTexts();
			outlookTimePickerStart.Leave += outlookTimePickerStartLeave;
			outlookTimePickerStart.KeyDown += outlookTimePickerStartKeyDown;
			outlookTimePickerStart.SelectedIndexChanged += outlookTimePickerStartSelectedIndexChanged;
			outlookTimePickerEnd.Leave += outlookTimePickerEndLeave;
			outlookTimePickerEnd.KeyDown += outlookTimePickerEndKeyDown;
			outlookTimePickerEnd.SelectedIndexChanged +=  outlookTimePickerEndSelectedIndexChanged;
			dateTimePickerAdvStart.Calendar.TodayButton.Text = UserTexts.Resources.Today;
			dateTimePickerAdvEnd.Calendar.TodayButton.Text = UserTexts.Resources.Today;

			dateTimePickerAdvStart.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdvEnd.SetCultureInfoSafe(CultureInfo.CurrentCulture);

			dateTimePickerAdvStart.ValueChanged += dateTimePickerAdvStartValueChanged;
			dateTimePickerAdvEnd.ValueChanged += dateTimePickerAdvEndValueChanged;
			
		}
		
		public MeetingRecurrenceView(IMeetingViewModel meetingViewModel, INotifyComposerMeetingChanged composer) : this()
		{
			if (composer == null) throw new ArgumentNullException("composer");
			_composer = composer;
			if (DesignMode) return;
			_meetingRecurrencePresenter = new MeetingRecurrencePresenter(this, meetingViewModel);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DesignMode)return;
				
			_meetingRecurrencePresenter.Initialize();
	
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_meetingRecurrencePresenter.SaveRecurrenceForMeeting();
			notifyMeetingDatesChanged();
			NotifyMeetingTimeChanged();
		}

		private void buttonAdvRemoveClick(object sender, EventArgs e)
		{
			_meetingRecurrencePresenter.RemoveRecurrence();
			notifyMeetingDatesChanged();
		}

		public void NotifyMeetingTimeChanged()
		{
			
		}

		private void notifyMeetingDatesChanged()
		{
			_composer.NotifyMeetingDatesChanged(this);
		}

		public void OnParticipantsSet()
		{
		}

		public void OnDisableWhileLoadingStateHolder()
		{
		}

		public void OnEnableAfterLoadingStateHolder()
		{
		}

		public void OnMeetingDatesChanged()
		{
		}

		public void OnMeetingTimeChanged()
		{
		}

		public IMeetingDetailPresenter Presenter
		{
			get { return null; }
		}

		public void ResetSelection()
		{
			buttonAdvOK.Select();
		}

		public void SetEndTime(TimeSpan endTime)
		{
			outlookTimePickerEnd.SetTimeValue(endTime);
		}

		public void SetStartTime(TimeSpan startTime)
		{
			outlookTimePickerStart.SetTimeValue(startTime);
		}

		public void SetStartDate(DateOnly startDate)
		{
			dateTimePickerAdvStart.Value = startDate.Date;
		}

		public void SetRecurringEndDate(DateOnly endDate)
		{
			dateTimePickerAdvEnd.Value = endDate.Date;
		}

		public void SetRecurringOption(RecurrentMeetingType recurrentMeetingType)
		{
			switch(recurrentMeetingType)
			{
				case RecurrentMeetingType.Daily:
					radioButtonAdvDailyRecurring.Checked = true;
					break;
				case RecurrentMeetingType.Weekly:
					radioButtonAdvWeeklyRecurring.Checked = true;
					break;
				case RecurrentMeetingType.MonthlyByDay:
				case RecurrentMeetingType.MonthlyByWeek:
					radioButtonAdvMonthlyRecurring.CheckedInt = (int) recurrentMeetingType;
					radioButtonAdvMonthlyRecurring.Checked = true;
					break;
			}
		}

		public void RefreshRecurrenceOption(RecurrentMeetingOptionViewModel meetingOptionViewModel)
		{
			Control control = null;
			switch (meetingOptionViewModel.RecurrentMeetingType)
			{
				case RecurrentMeetingType.Daily:
					control = new DailyRecurrenceView(meetingOptionViewModel);
					break;
				case RecurrentMeetingType.Weekly:
					control = new WeeklyRecurrenceView(meetingOptionViewModel);
					break;
				case RecurrentMeetingType.MonthlyByDay:
				case RecurrentMeetingType.MonthlyByWeek:
					var monthlyRecurrenceView = new MonthlyRecurrenceView(meetingOptionViewModel);
					monthlyRecurrenceView.OtherRecurrentMeetingTypeRequested += monthlyRecurrenceViewOtherRecurrentMeetingTypeRequested;
					control = monthlyRecurrenceView;
					break;
			}
			if (control != null)
			{
				splitContainerAdv1.Panel2.Controls.Clear();
				control.Dock = DockStyle.Fill;
				splitContainerAdv1.Panel2.Controls.Add(control);
			}
		}

		public void SetRecurringExists(bool recurringExists)
		{
			buttonAdvRemove.Enabled = recurringExists;
		}

		public void AcceptAndClose()
		{
			DialogResult = DialogResult.OK;
		}

		private void monthlyRecurrenceViewOtherRecurrentMeetingTypeRequested(object sender, CustomEventArgs<RecurrentMeetingType> e)
		{
			if (_meetingRecurrencePresenter != null)
			_meetingRecurrencePresenter.ChangeRecurringType(e.Value);
		}

		private void radioButtonAdvRecurringTypeCheckChanged(object sender, EventArgs e)
		{
			var radioButton = sender as RadioButtonAdv;
			if (radioButton==null) return;
			if (!radioButton.Checked) return;

			if (_meetingRecurrencePresenter != null)
			_meetingRecurrencePresenter.ChangeRecurringType((RecurrentMeetingType)radioButton.CheckedInt);
		}

		private void dateTimePickerAdvEndValueChanged(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter!=null)
				_meetingRecurrencePresenter.SetRecurringEndDate(new DateOnly(dateTimePickerAdvEnd.Value));
		}

		private void dateTimePickerAdvStartValueChanged(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter != null)
			_meetingRecurrencePresenter.SetStartDate(new DateOnly(dateTimePickerAdvStart.Value));
		}

		void outlookTimePickerStartLeave(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter != null)
				_meetingRecurrencePresenter.OnOutlookTimePickerStartLeave(outlookTimePickerStart.Text);
		}

		void outlookTimePickerEndLeave(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter != null)
				_meetingRecurrencePresenter.OnOutlookTimePickerEndLeave(outlookTimePickerEnd.Text);
		}

		void outlookTimePickerStartKeyDown(object sender, KeyEventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerStartKeyDown(e.KeyCode, outlookTimePickerStart.Text);
		}

		void outlookTimePickerEndKeyDown(object sender, KeyEventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerEndKeyDown(e.KeyCode, outlookTimePickerEnd.Text);
		}

		void outlookTimePickerStartSelectedIndexChanged(object sender, EventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerStartSelectedIndexChanged(outlookTimePickerStart.Text);
		}

		void outlookTimePickerEndSelectedIndexChanged(object sender, EventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerEndSelectedIndexChanged(outlookTimePickerEnd.Text);
		}


		public DialogResult ShowConfirmationMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public void ShowInformationMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}

		public DialogResult ShowWarningMessage(string text, string caption)
		{
			throw new NotImplementedException();
		}
	}
}
