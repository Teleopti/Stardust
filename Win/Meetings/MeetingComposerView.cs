using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
{
	public partial class MeetingComposerView : BaseRibbonForm, IMeetingComposerView, INotifyComposerMeetingChanged
    {
        private readonly IList<IMeetingDetailView> _meetingDetailViews = new List<IMeetingDetailView>();
        private readonly MeetingComposerPresenter _meetingComposerPresenter;
        private readonly bool _editMeetingPermission;
        private readonly bool _viewSchedulesPermission;
	    private IMeetingDetailView _currentView;
	    private readonly IEventAggregator _eventAggregator;
	    private readonly IToggleManager _toogleManager;

	    public event EventHandler<ModifyMeetingEventArgs> ModificationOccurred;

        protected MeetingComposerView()
        {
            InitializeComponent();
            if (DesignMode) return;

            SetTexts();
        }

	    public MeetingComposerView(IMeetingViewModel meetingViewModel, ISchedulerStateHolder schedulerStateHolder, 
            bool editPermission, bool viewSchedulesPermission, IEventAggregator eventAggregator, IToggleManager toogleManager)
            : this()
        {
            _editMeetingPermission = editPermission;
            _viewSchedulesPermission = viewSchedulesPermission;
            _eventAggregator = eventAggregator;
	        _toogleManager = toogleManager;
	        _meetingComposerPresenter = new MeetingComposerPresenter(this,meetingViewModel,schedulerStateHolder);
            panelContent.Enabled = _editMeetingPermission;
            ribbonControlAdv1.Enabled = _editMeetingPermission;
            toolStripButtonSchedules.Enabled = _viewSchedulesPermission;
			_meetingDetailViews.Add(CreateMeetingGeneralView(_meetingComposerPresenter.Model));
            _meetingComposerPresenter.Initialize();
            
            ChangeView(ViewType.MeetingView);

            SetToolStripsToPreferredSize();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        // Close the meeting composer
                        Close();
                        break;

                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            _meetingComposerPresenter.OnClose();
			if (_meetingComposerPresenter.TrySave())
				SaveValidMeeting();
            e.Cancel = !_meetingComposerPresenter.CanClose();
        }

		private void MeetingComposer_Load(object sender, EventArgs e)
        {
            BackColor = ColorHelper.ControlPanelColor;
        }

		private void toolStripButtonMainSave_Click(object sender, EventArgs e)
		{
			SaveValidMeeting();
		}

		public void SetInstanceId(Guid id)
		{
			_meetingComposerPresenter.SetInstanceId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private void SaveValidMeeting()
		{
			var start = String.Empty;
			var end = String.Empty;
			if (_currentView is MeetingGeneralView)
			{
				start = (_currentView as MeetingGeneralView).GetStartTimeText;
				end = (_currentView as MeetingGeneralView).GetEndTimeText;
			}
			else if (_currentView is MeetingSchedulesView)
			{
				start = (_currentView as MeetingSchedulesView).GetStartTimeText;
				end = (_currentView as MeetingSchedulesView).GetEndTimeText;
			}
			if (_currentView is MeetingImpactView)
			{
				start = (_currentView as MeetingImpactView).GetStartTimeText;
				end = (_currentView as MeetingImpactView).GetEndTimeText;
				
			}
			TimeSpan startTime;
			TimeSpan endTime;

			if (!TimeHelper.TryParse(start, out startTime))
			{
				MessageBox.Show(this, string.Format(UserTexts.Resources.InvalidTimeValue, UserTexts.Resources.StartTime));
				return;
			}
			if (!TimeHelper.TryParse(end, out endTime))
			{
				MessageBox.Show(this, string.Format(UserTexts.Resources.InvalidTimeValue, UserTexts.Resources.EndTime));
				return;
			}
			
			_meetingComposerPresenter.Model.StartTime = startTime;
			_meetingComposerPresenter.Model.EndTime = endTime;
			if (endTime <= startTime)
				_meetingComposerPresenter.InvalidTimeInfo();
			else
			{
				_meetingComposerPresenter.SaveMeeting();
			}
		}

		public void OnModificationOccurred(IMeeting meeting, bool isDeleted)
        {
        	var handler = ModificationOccurred;
            if (handler!= null)
            {
                handler(this, new ModifyMeetingEventArgs(_meetingComposerPresenter.Model.Meeting, false));
            }
            _eventAggregator.GetEvent<MeetingModificationOccurred>().Publish(string.Empty);
        }

        private void toolStripButtonMainDelete_Click(object sender, EventArgs e)
        {
            _meetingComposerPresenter.DeleteMeeting();
        }

        private void toolStripButtonMainAddressBook_Click(object sender, EventArgs e)
        {
            _meetingComposerPresenter.ShowAddressBook();
        }

        private void Composer_ParticipantSelectionRequested(object sender, EventArgs e)
        {
            _meetingComposerPresenter.ShowAddressBook();
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            _meetingComposerPresenter.DeleteMeeting();
        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripButtonMeetings_Click(object sender, EventArgs e)
        {
			ChangeView(ViewType.MeetingView);
        }

        private void toolStripButtonSchedules_Click(object sender, EventArgs e)
        {
            ChangeView(ViewType.MeetingSchedulerView);
        }

        private void toolStripButtonRecurrentMeetings_Click(object sender, EventArgs e)
        {
            using (var meetingRecurrenceView = new MeetingRecurrenceView(_meetingComposerPresenter.Model, this))
            {
                _currentView.Presenter.UpdateView();
                if (meetingRecurrenceView.ShowDialog(this) != DialogResult.OK) return;
                _meetingComposerPresenter.RecurrentMeetingUpdated();
            }
        }

        public void ShowAddressBook(AddressBookViewModel addressBookViewModel, DateOnly startDate)
        {
            using (var addressBookView = new AddressBookView(addressBookViewModel, startDate))
            {
                if (addressBookView.ShowDialog(this) != DialogResult.OK) return;
                _meetingComposerPresenter.AddParticipantsFromAddressBook(addressBookViewModel);

                if (toolStripButtonSchedules.Checked)
                    ChangeView(ViewType.MeetingSchedulerView);

                if(toolStripButtonImpact.Checked)
                    ChangeView(ViewType.ImpactView);
            }
        }

        public void OnParticipantsSet()
        {
            foreach (IMeetingDetailView meetingDetailView in _meetingDetailViews)
            {
                meetingDetailView.OnParticipantsSet();
            }
        }

        public void SetRecurrentMeetingActive(bool recurrentMeetingActive)
        {
            toolStripButtonRecurrentMeetings.Checked = recurrentMeetingActive;
        }

        public void DisableWhileLoadingStateHolder()
        {
            toolStripButtonMainAddressBook.Enabled = false;
            toolStripButtonSchedules.Enabled = false;
            toolStripButtonImpact.Enabled = false;
	        toolStripButtonMainDelete.Enabled = false;
	        toolStripButtonMainSave.Enabled = false;
	       
            foreach (IMeetingDetailView meetingDetailView in _meetingDetailViews)
            {
                meetingDetailView.OnDisableWhileLoadingStateHolder();
            }
        }

        public void EnableAfterLoadingStateHolder()
        {
            toolStripButtonMainAddressBook.Enabled = true;
            toolStripButtonSchedules.Enabled = _viewSchedulesPermission;
            toolStripButtonImpact.Enabled = true;
			toolStripButtonMainDelete.Enabled = true;
			toolStripButtonMainSave.Enabled = true;
	
            foreach (IMeetingDetailView meetingDetailView in _meetingDetailViews)
            {
                meetingDetailView.OnEnableAfterLoadingStateHolder();
            }
        }

        public void StartLoadingStateHolder()
        {
            backgroundWorkerLoadStateHolder.RunWorkerAsync();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private void ChangeView(ViewType viewType)
		{
			if (_currentView != null)
				_currentView.ResetSelection();

			panelContent.Controls.Clear();

            toolStripButtonMeetings.Checked = false;
            toolStripButtonSchedules.Checked = false;
            toolStripButtonImpact.Checked = false;

            switch (viewType)
            {
                case ViewType.MeetingView:
                    toolStripButtonMeetings.Checked = true;
                    _currentView = _meetingDetailViews[0];
                    break;

                case ViewType.MeetingSchedulerView:
                    EnsureSchedulesViewIsLoaded();
                    toolStripButtonSchedules.Checked = true;
                    _currentView = _meetingDetailViews[1];
                    break;
				case ViewType.ImpactView:
					EnsureImpactViewIsLoaded();
					toolStripButtonImpact.Checked = true;
                    _currentView = _meetingDetailViews[2];
					break;
            }

            if (_currentView == null) return;

            panelContent.Controls.Add((Control)_currentView);
            ((Control)_currentView).Dock = DockStyle.Fill;
            _currentView.Presenter.UpdateView();
        }

        private void SetToolStripsToPreferredSize()
        {
            toolStripEx2.Size = toolStripEx2.PreferredSize;
            toolStripEx3.Size = toolStripEx3.PreferredSize;
            toolStripEx4.Size = toolStripEx4.PreferredSize;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private IMeetingGeneralView CreateMeetingGeneralView(IMeetingViewModel meetingViewModel)
        {
            var meetingGeneralView = new MeetingGeneralView(meetingViewModel, this) {Dock = DockStyle.Fill};
            meetingGeneralView.ParticipantSelectionRequested += Composer_ParticipantSelectionRequested;
            
            return meetingGeneralView;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IMeetingImpactView GetImpactView()
		{
			return new MeetingImpactView(_meetingComposerPresenter.Model, _meetingComposerPresenter.SchedulerStateHolder, this, _toogleManager);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private IMeetingDetailView CreateMeetingSchedulesView(IMeetingViewModel model, ISchedulerStateHolder holder)
        {
            var meetingSchedulesView = new MeetingSchedulesView(model, holder, this);
            return meetingSchedulesView;
        }

        private void ToolStripButtonImpact_Click(object sender, EventArgs e)
        {
			ChangeView(ViewType.ImpactView);
        }

        private void backgroundWorkerLoadStateHolder_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            _meetingComposerPresenter.OnSchedulerStateHolderRequested();
        }

        private void backgroundWorkerLoadStateHolder_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;

            _meetingComposerPresenter.OnSchedulerStateHolderLoaded();
        }

		private void EnsureSchedulesViewIsLoaded()
		{
			if (_meetingDetailViews.Count == 1)
				_meetingDetailViews.Add(CreateMeetingSchedulesView(_meetingComposerPresenter.Model, _meetingComposerPresenter.SchedulerStateHolder));
		}
		
		private void EnsureImpactViewIsLoaded()
		{
			EnsureSchedulesViewIsLoaded();
			if (_meetingDetailViews.Count == 2)
				_meetingDetailViews.Add(GetImpactView());
		}

        private void ReleaseManagedResources()
        {
            _meetingComposerPresenter.Dispose();
        }

        private void UnhookEvents()
        {
            if (_meetingDetailViews.Count==0) return;
            var generalView = _meetingDetailViews[0] as MeetingGeneralView;
            if (generalView!=null)
            {
                generalView.ParticipantSelectionRequested -= Composer_ParticipantSelectionRequested;
            }
            foreach (var meetingDetailView in _meetingDetailViews)
            {
                meetingDetailView.Presenter.CancelAllLoads();
                meetingDetailView.Presenter.Dispose();
            	var disposableView = meetingDetailView as IDisposable;
				if (disposableView != null)
					disposableView.Dispose();
            }
            _meetingDetailViews.Clear();
        }

		public void NotifyMeetingDatesChanged(object sender)
    	{
			foreach (var detailView in _meetingDetailViews)
			{
				if (detailView.Equals(sender)) continue;
				detailView.OnMeetingDatesChanged();
			}
    	}

    	public void NotifyMeetingTimeChanged(object sender)
    	{
			foreach (var detailView in _meetingDetailViews)
			{
				if (detailView == sender) continue;
				detailView.OnMeetingTimeChanged();
			}
    	}
    }

	public interface INotifyComposerMeetingChanged
	{
		void NotifyMeetingDatesChanged(object sender);
		void NotifyMeetingTimeChanged(object sender);
	}
}
