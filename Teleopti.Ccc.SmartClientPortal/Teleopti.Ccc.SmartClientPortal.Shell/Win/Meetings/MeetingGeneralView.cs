using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	public partial class MeetingGeneralView : BaseUserControl, IMeetingGeneralView
	{
		private readonly INotifyComposerMeetingChanged _composer;
		private MeetingGeneralPresenter _presenter;
		private LastSelectionWas _lastSelectionWas;

		public event EventHandler ParticipantSelectionRequested;
		
		protected MeetingGeneralView()
		{
			InitializeComponent();
			if (DesignMode) return;

			SetTexts();
			outlookTimePickerEndTime.CreateAndBindList();
			outlookTimePickerStartTime.CreateAndBindList();
			outlookTimePickerStartTime.MaxValue = TimeSpan.FromDays(1);
			outlookTimePickerEndTime.MaxValue = TimeSpan.FromDays(1);

			dateTimePickerAdvStartDate.SetSafeBoundary();
			dateTimePickerAdvEndDate.SetSafeBoundary();
			
			dateTimePickerAdvStartDate.SetCultureInfoSafe(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
			dateTimePickerAdvEndDate.SetCultureInfoSafe(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);

			dateTimePickerAdvStartDate.ValueChanged += dateTimePickerAdvStartDateValueChanged;
		}

		public MeetingGeneralView(IMeetingViewModel meetingViewModel, INotifyComposerMeetingChanged composer) : this()
		{
			if (composer == null) throw new ArgumentNullException("composer");
			_composer = composer;
			_presenter = new MeetingGeneralPresenter(this, meetingViewModel);
			_presenter.Initialize();

			buttonAdvParticipants.Focus();
			comboBoxAdvActivity.SelectedIndexChanged += comboBoxAdvActivitySelectedIndexChanged;
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			dateTimePickerAdvEndDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
			dateTimePickerAdvStartDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
		}

		private void buttonAdvParticipantsClick(object sender, EventArgs e)
		{
			var handler = ParticipantSelectionRequested;
			if (handler!= null)
			{
				handler.Invoke(sender, e);
			}
		}

		private void textBoxExtSubjectTextChanged(object sender, EventArgs e)
		{
			_presenter.SetSubject(textBoxExtSubject.Text);
		}

		private void textBoxExtLocationTextChanged(object sender, EventArgs e)
		{
			_presenter.SetLocation(textBoxExtLocation.Text);
		}

		public void NotifyMeetingTimeChanged()
		{
			_composer.NotifyMeetingTimeChanged(this);
		}

		private void notifyMeetingDatesChanged()
		{
			_composer.NotifyMeetingDatesChanged(this);
		}

		private void comboBoxAdvActivitySelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SetActivity((IActivity)comboBoxAdvActivity.SelectedItem);
		}

		private void textBoxExtDescriptionTextChanged(object sender, EventArgs e)
		{
			_presenter.SetDescription(textBoxExtDescription.Text);
		}

		private void meetingPersonTextBoxParticipantTextChanged(object sender, EventArgs e)
		{
			//_presenter.ParseParticipants(textBoxExtParticipant.Text);
		}

		public void SetOrganizer(string organizer)
		{
		}

		public void SetParticipants(string participants)
		{
			textBoxExtParticipant.Text = participants;
		}

		public void SetActivityList(IList<IActivity> activities)
		{
			comboBoxAdvActivity.DataSource = activities;
			comboBoxAdvActivity.DisplayMember = "Name";
		}

		public void SetSelectedActivity(IActivity activity)
		{
			comboBoxAdvActivity.SelectedItem = activity;
		}

		public void SetTimeZoneList(IList<TimeZoneInfo> timeZoneList)
		{
		}

		public void SetSelectedTimeZone(TimeZoneInfo timeZone)
		{
		}

		public void SetStartDate(DateOnly startDate)
		{
			dateTimePickerAdvStartDate.Value = startDate.Date;
		}

		public void SetEndDate(DateOnly endDate)
		{
			dateTimePickerAdvEndDate.Value = endDate.Date;
			
		}

		public void SetStartTime(TimeSpan startTime)
		{
			outlookTimePickerStartTime.SetTimeValue(startTime);
		}

		public void SetEndTime(TimeSpan endTime)
		{
			outlookTimePickerEndTime.SetTimeValue(endTime);
		}

		public void SetRecurringEndDate(DateOnly recurringEndDate)
		{
		}

		public void SetSubject(string subject)
		{
			textBoxExtSubject.Text = subject;
		}

		public void SetLocation(string location)
		{
			textBoxExtLocation.Text = location;
		}

		public void SetDescription(string description)
		{
			textBoxExtDescription.Text = description;
		}

		public void OnParticipantsSet()
		{
			_presenter.OnParticipantsSet();
		}

		public void OnDisableWhileLoadingStateHolder()
		{
			buttonAdvParticipants.Enabled = false;
		}

		public void OnEnableAfterLoadingStateHolder()
		{
			buttonAdvParticipants.Enabled = true;
		}

		public void OnMeetingDatesChanged()
		{
			dateTimePickerAdvStartDate.Value = _presenter.Model.StartDate.Date;
			dateTimePickerAdvEndDate.Value = _presenter.Model.EndDate.Date;
		}

		public void OnMeetingTimeChanged()
		{
			outlookTimePickerStartTime.SetTimeValue(_presenter.Model.StartTime);
			outlookTimePickerEndTime.SetTimeValue(_presenter.Model.EndTime);
			dateTimePickerAdvEndDate.Value = _presenter.Model.EndDate.Date;
			//outlookTimePickerStartTime.TextChanged += outlookTimePickerStartTime_TextChanged;
		}

		public IMeetingDetailPresenter Presenter
		{
			get { return _presenter; }
		}

		public void ResetSelection()
		{
			 textBoxExtLocation.Select();
		}

		public string GetStartTimeText
		{
			get { return outlookTimePickerStartTime.Text; }
		}

		public string GetEndTimeText
		{
			get { return outlookTimePickerEndTime.Text; }
		}

		private void outlookTimePickerStartTimeLeave(object sender, EventArgs e)
		{
			setStartTime();
		}

		private void outlookTimePickerStartTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeKeyDown(e.KeyCode, outlookTimePickerStartTime.Text);
		}

		private void outlookTimePickerEndTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (outlookTimePickerEndTime.Disposing)
				return;
			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
		}

		void outlookTimePickerEndTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeKeyDown(e.KeyCode, outlookTimePickerEndTime.Text);
		}

		private void dateTimePickerAdvStartDatePopupClosed(object sender, PopupClosedEventArgs e)
		{
			_presenter.SetStartDate(new DateOnly(dateTimePickerAdvStartDate.Value));
			notifyMeetingDatesChanged();
		}

		private void dateTimePickerAdvStartDateValueChanged(object sender, EventArgs e)
		{
			_presenter.SetStartDate(new DateOnly(dateTimePickerAdvStartDate.Value));
			notifyMeetingDatesChanged();
		}

		private void textBoxExtParticipantMouseUp(object sender, MouseEventArgs e)
		{
			TextBoxNameExtender.GetSelected(textBoxExtParticipant);
		}

		private void textBoxExtParticipantKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
			{
				var textBox = textBoxExtParticipant;
				var first = textBox.Text.Substring(0, textBox.SelectionStart);
				var allSelectedIndexes = TextBoxNameExtender.SelectedIndexes(textBox);
				_presenter.Remove(allSelectedIndexes);
				textBox.Select(first.Length, 0);
				TextBoxNameExtender.GetSelected(textBox);
			}

			else if (e.Modifiers != Keys.Control)
			{
				_lastSelectionWas = TextBoxNameExtender.KeyDown(textBoxExtParticipant, e, _lastSelectionWas, true);
			}

			e.SuppressKeyPress = true;
		}

		public void DescriptionFocus()
		{
			textBoxExtDescription.Focus();
		}

		private void setStartTime()
		{
			if (_presenter == null) return;
			if (outlookTimePickerStartTime.Disposing)
				return;
			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
		}

		private void outlookTimePickerStartTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			setStartTime();
		}
	}
}