﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
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
			outlookTimePickerStart.Leave += OutlookTimePickerStartLeave;
			outlookTimePickerStart.KeyDown += OutlookTimePickerStartKeyDown;
			outlookTimePickerStart.SelectedIndexChanged += OutlookTimePickerStartSelectedIndexChanged;
			outlookTimePickerEnd.Leave += OutlookTimePickerEndLeave;
			outlookTimePickerEnd.KeyDown += OutlookTimePickerEndKeyDown;
			outlookTimePickerEnd.SelectedIndexChanged +=  OutlookTimePickerEndSelectedIndexChanged;
			dateTimePickerAdvStart.Calendar.TodayButton.Text = UserTexts.Resources.Today;
			dateTimePickerAdvEnd.Calendar.TodayButton.Text = UserTexts.Resources.Today;
			UseOffice2007SchemeBackColor = false;

		}

		/// <summary>
        /// Initializes a new instance of the <see cref="MeetingRecurrenceView"/> class.
        /// </summary>
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
				BackColor = Color.FromArgb(216,228,246);
	
        }

        private void ButtonAdvOkClick(object sender, EventArgs e)
        {
            _meetingRecurrencePresenter.SaveRecurrenceForMeeting();
            NotifyMeetingDatesChanged();
            NotifyMeetingTimeChanged();
        }

        private void ButtonAdvRemoveClick(object sender, EventArgs e)
        {
            _meetingRecurrencePresenter.RemoveRecurrence();
            NotifyMeetingDatesChanged();
        }

    	public void NotifyMeetingTimeChanged()
        {
        	_composer.NotifyMeetingTimeChanged(this);
        }

		private void NotifyMeetingDatesChanged()
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
            dateTimePickerAdvStart.Value = startDate;
        }

        public void SetRecurringEndDate(DateOnly endDate)
        {
            dateTimePickerAdvEnd.Value = endDate;
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
                    monthlyRecurrenceView.OtherRecurrentMeetingTypeRequested += MonthlyRecurrenceViewOtherRecurrentMeetingTypeRequested;
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

    	public void SetMeetingDuration(TimeSpan duration)
    	{
    		throw new NotImplementedException();
    	}

    	public void AcceptAndClose()
        {
            DialogResult = DialogResult.OK;
        }

        private void MonthlyRecurrenceViewOtherRecurrentMeetingTypeRequested(object sender, CustomEventArgs<RecurrentMeetingType> e)
        {
            if (_meetingRecurrencePresenter != null)
            _meetingRecurrencePresenter.ChangeRecurringType(e.Value);
        }

        private void RadioButtonAdvRecurringTypeCheckChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButtonAdv;
            if (radioButton==null) return;
            if (!radioButton.Checked) return;

            if (_meetingRecurrencePresenter != null)
            _meetingRecurrencePresenter.ChangeRecurringType((RecurrentMeetingType)radioButton.CheckedInt);
        }

        private void DateTimePickerAdvEndValueChanged(object sender, EventArgs e)
        {
            if (_meetingRecurrencePresenter!=null)
                _meetingRecurrencePresenter.SetRecurringEndDate(new DateOnly(dateTimePickerAdvEnd.Value));
        }

        private void DateTimePickerAdvStartValueChanged(object sender, EventArgs e)
        {
            if (_meetingRecurrencePresenter != null)
            _meetingRecurrencePresenter.SetStartDate(new DateOnly(dateTimePickerAdvStart.Value));
        }

		void OutlookTimePickerStartLeave(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter != null)
				_meetingRecurrencePresenter.OnOutlookTimePickerStartLeave(outlookTimePickerStart.Text);
		}

		void OutlookTimePickerEndLeave(object sender, EventArgs e)
		{
			if (_meetingRecurrencePresenter != null)
				_meetingRecurrencePresenter.OnOutlookTimePickerEndLeave(outlookTimePickerEnd.Text);
		}

    	void OutlookTimePickerStartKeyDown(object sender, KeyEventArgs e)
    	{
    		_meetingRecurrencePresenter.OnOutlookTimePickerStartKeyDown(e.KeyCode, outlookTimePickerStart.Text);
    	}

		void OutlookTimePickerEndKeyDown(object sender, KeyEventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerEndKeyDown(e.KeyCode, outlookTimePickerEnd.Text);
		}

		void OutlookTimePickerStartSelectedIndexChanged(object sender, EventArgs e)
		{
			_meetingRecurrencePresenter.OnOutlookTimePickerStartSelectedIndexChanged(outlookTimePickerStart.Text);
		}

		void OutlookTimePickerEndSelectedIndexChanged(object sender, EventArgs e)
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

    	public DialogResult ShowOkCancelMessage(string text, string caption)
    	{
    		throw new NotImplementedException();
    	}

    	public DialogResult ShowWarningMessage(string text, string caption)
    	{
    		throw new NotImplementedException();
    	}
    }
}
