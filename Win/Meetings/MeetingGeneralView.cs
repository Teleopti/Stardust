using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
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

            SetColors();
            SetTexts();
            outlookTimePickerEndTime.CreateAndBindList();
            outlookTimePickerStartTime.CreateAndBindList();
            outlookTimePickerStartTime.MaxValue = TimeSpan.FromDays(1);
            outlookTimePickerEndTime.MaxValue = TimeSpan.FromDays(1);
        }

        private void SetColors()
        {
            BackColor = ColorHelper.ControlPanelColor;
        }

        public MeetingGeneralView(IMeetingViewModel meetingViewModel, INotifyComposerMeetingChanged composer) : this()
        {
        	if (composer == null) throw new ArgumentNullException("composer");
        	_composer = composer;
        	_presenter = new MeetingGeneralPresenter(this, meetingViewModel);
            _presenter.Initialize();

            buttonAdvParticipants.Focus();
            comboBoxAdvActivity.SelectedIndexChanged += comboBoxAdvActivity_SelectedIndexChanged;
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            dateTimePickerAdvEndDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
            dateTimePickerAdvStartDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
        }

        private void buttonAdvParticipants_Click(object sender, EventArgs e)
        {
        	var handler = ParticipantSelectionRequested;
            if (handler!= null)
            {
                handler.Invoke(sender, e);
            }
        }

        private void textBoxExtSubject_TextChanged(object sender, EventArgs e)
        {
            _presenter.SetSubject(textBoxExtSubject.Text);
        }

        private void textBoxExtLocation_TextChanged(object sender, EventArgs e)
        {
            _presenter.SetLocation(textBoxExtLocation.Text);
        }

        public void NotifyMeetingTimeChanged()
        {
        	_composer.NotifyMeetingTimeChanged(this);
        }

        private void NotifyMeetingDatesChanged()
        {
        	_composer.NotifyMeetingDatesChanged(this);
        }

        private void comboBoxAdvActivity_SelectedIndexChanged(object sender, EventArgs e)
        {
            _presenter.SetActivity((IActivity)comboBoxAdvActivity.SelectedItem);
        }

        private void textBoxExtDescription_TextChanged(object sender, EventArgs e)
        {
            _presenter.SetDescription(textBoxExtDescription.Text);
        }

        private void meetingPersonTextBoxParticipant_TextChanged(object sender, EventArgs e)
        {
            _presenter.ParseParticipants(textBoxExtParticipant.Text);
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

        public void SetTimeZoneList(IList<ICccTimeZoneInfo> timeZoneList)
        {
        }

        public void SetSelectedTimeZone(ICccTimeZoneInfo timeZone)
        {
        }

        public void SetStartDate(DateOnly startDate)
        {
            dateTimePickerAdvStartDate.Value = startDate;
        }

        public void SetEndDate(DateOnly endDate)
        {
            dateTimePickerAdvEndDate.Value = endDate;
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
            dateTimePickerAdvStartDate.Value = _presenter.Model.StartDate;
            dateTimePickerAdvEndDate.Value = _presenter.Model.EndDate;
        }

        public void OnMeetingTimeChanged()
        {
			//outlookTimePickerStartTime.TextChanged -= outlookTimePickerStartTime_TextChanged;
            //outlookTimePickerEndTime.TextChanged -= outlookTimePickerEndTime_TextChanged;
            outlookTimePickerStartTime.SetTimeValue(_presenter.Model.StartTime);
            outlookTimePickerEndTime.SetTimeValue(_presenter.Model.EndTime);
			//outlookTimePickerStartTime.TextChanged += outlookTimePickerStartTime_TextChanged;
            dateTimePickerAdvEndDate.Value = _presenter.Model.EndDate;
            //outlookTimePickerEndTime.TextChanged += outlookTimePickerEndTime_TextChanged;
        }

    	public IMeetingDetailPresenter Presenter
    	{
			get { return _presenter; }
    	}

		public string GetStartTimeText
		{
			get { return outlookTimePickerStartTime.Text; }
		}

		public string GetEndTimeText
		{
			get { return outlookTimePickerEndTime.Text; }
		}

		private void OutlookTimePickerStartTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
            if (outlookTimePickerStartTime.Disposing)
                return;
			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
		}

		private void OutlookTimePickerStartTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeKeyDown(e.KeyCode, outlookTimePickerStartTime.Text);
		}

		private void OutlookTimePickerEndTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
            if (outlookTimePickerEndTime.Disposing)
                return;
			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
		}

		void OutlookTimePickerEndTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeKeyDown(e.KeyCode, outlookTimePickerEndTime.Text);
		}
		
    	private void outlookTimePickerStartTime_SelectedIndexChanged(object sender, EventArgs e)
    	{
			if (outlookTimePickerStartTime.SelectedIndex > -1)
				_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Items[outlookTimePickerStartTime.SelectedIndex].ToString());
    	}

    	private void outlookTimePickerEndTime_SelectedIndexChanged(object sender, EventArgs e)
    	{
			if (outlookTimePickerEndTime.SelectedIndex > -1)
				_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Items[outlookTimePickerEndTime.SelectedIndex].ToString());
    	}

    	private void dateTimePickerAdvStartDate_PopupClosed(object sender, PopupClosedEventArgs e)
    	{
			_presenter.SetStartDate(new DateOnly(dateTimePickerAdvStartDate.Value));
			NotifyMeetingDatesChanged();
    	}

        private void dateTimePickerAdvStartDate_ValueChanged(object sender, EventArgs e)
        {
            _presenter.SetStartDate(new DateOnly(dateTimePickerAdvStartDate.Value));
            NotifyMeetingDatesChanged();
        }

        private void textBoxExtParticipant_MouseUp(object sender, MouseEventArgs e)
        {
            TextBoxNameExtender.GetSelected(textBoxExtParticipant);
        }

        private void textBoxExtParticipant_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers != Keys.Control)
                _lastSelectionWas = TextBoxNameExtender.KeyDown(textBoxExtParticipant, e, _lastSelectionWas);
            
            e.SuppressKeyPress = true;
        }
    }
}