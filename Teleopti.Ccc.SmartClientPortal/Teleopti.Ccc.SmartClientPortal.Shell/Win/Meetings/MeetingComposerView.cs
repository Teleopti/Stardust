using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	public partial class MeetingComposerView : BaseRibbonForm, IMeetingComposerView, INotifyComposerMeetingChanged
	{
		private readonly IList<IMeetingDetailView> _meetingDetailViews = new List<IMeetingDetailView>();
		private readonly MeetingComposerPresenter _meetingComposerPresenter;
		private readonly bool _viewSchedulesPermission;
		private IMeetingDetailView _currentView;
		private readonly IEventAggregator _eventAggregator;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;

		public event EventHandler<ModifyMeetingEventArgs> ModificationOccurred;

		protected MeetingComposerView()
		{
			InitializeComponent();
			if (DesignMode) return;

			SetTexts();
		}

		public MeetingComposerView(IMeetingViewModel meetingViewModel, SchedulingScreenState schedulingScreenState, 
			bool editPermission, bool viewSchedulesPermission, IEventAggregator eventAggregator,
			IResourceCalculation resourceOptimizationHelper, ISkillPriorityProvider skillPriorityProvider, 
			IScheduleStorageFactory scheduleStorageFactory, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
			: this()
		{
			bool editMeetingPermission = editPermission;
			_viewSchedulesPermission = viewSchedulesPermission;
			_eventAggregator = eventAggregator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_skillPriorityProvider = skillPriorityProvider;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_meetingComposerPresenter = new MeetingComposerPresenter(this, meetingViewModel, new DisableDeletedFilter(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make())), schedulingScreenState, scheduleStorageFactory);
			panelContent.Enabled = editMeetingPermission;
			ribbonControlAdv1.Enabled = editMeetingPermission;
			toolStripButtonSchedules.Enabled = _viewSchedulesPermission;
			_meetingDetailViews.Add(createMeetingGeneralView(_meetingComposerPresenter.Model));
			_meetingComposerPresenter.Initialize();
			
			changeView(ViewType.MeetingView);

			setToolStripsToPreferredSize();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int wmKeydown = 0x100;
			const int wmSyskeydown = 0x104;

			if ((msg.Msg == wmKeydown) || (msg.Msg == wmSyskeydown))
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
				saveValidMeeting();
			e.Cancel = !_meetingComposerPresenter.CanClose();
		}

		private void toolStripButtonMainSaveClick(object sender, EventArgs e)
		{
			saveValidMeeting();
		}

		public void SetInstanceId(Guid id)
		{
			_meetingComposerPresenter.SetInstanceId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private void saveValidMeeting()
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

		private void toolStripButtonMainDeleteClick(object sender, EventArgs e)
		{
			_meetingComposerPresenter.DeleteMeeting();
		}

		private void toolStripButtonMainAddressBookClick(object sender, EventArgs e)
		{
			_meetingComposerPresenter.ShowAddressBook();
		}

		private void composerParticipantSelectionRequested(object sender, EventArgs e)
		{
			_meetingComposerPresenter.ShowAddressBook();
		}

		private void toolStripButtonDeleteClick(object sender, EventArgs e)
		{
			_meetingComposerPresenter.DeleteMeeting();
		}

		private void toolStripButtonCloseClick(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripButtonMeetingsClick(object sender, EventArgs e)
		{
			changeView(ViewType.MeetingView);
		}

		private void toolStripButtonSchedulesClick(object sender, EventArgs e)
		{
			changeView(ViewType.MeetingSchedulerView);
		}

		private void toolStripButtonRecurrentMeetingsClick(object sender, EventArgs e)
		{
			using (var meetingRecurrenceView = new MeetingRecurrenceView(_meetingComposerPresenter.Model, this))
			{
				_currentView.Presenter.UpdateView();
				if (meetingRecurrenceView.ShowDialog(this) != DialogResult.OK) return;
				_meetingComposerPresenter.RecurrentMeetingUpdated();
			}

			_currentView.Presenter.UpdateView();
		}

		public void ShowAddressBook(AddressBookViewModel addressBookViewModel, DateOnly startDate)
		{
			using (var addressBookView = new AddressBookView(addressBookViewModel, startDate))
			{
				if (addressBookView.ShowDialog(this) != DialogResult.OK) return;
				_meetingComposerPresenter.AddParticipantsFromAddressBook(addressBookViewModel);

				if (toolStripButtonSchedules.Checked)
					changeView(ViewType.MeetingSchedulerView);

				if(toolStripButtonImpact.Checked)
					changeView(ViewType.ImpactView);
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
		private void changeView(ViewType viewType)
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
					ensureSchedulesViewIsLoaded();
					toolStripButtonSchedules.Checked = true;
					_currentView = _meetingDetailViews[1];
					break;
				case ViewType.ImpactView:
					ensureImpactViewIsLoaded();
					toolStripButtonImpact.Checked = true;
					_currentView = _meetingDetailViews[2];
					break;
			}

			if (_currentView == null) return;

			panelContent.Controls.Add((Control)_currentView);
			((Control)_currentView).Dock = DockStyle.Fill;
			_currentView.Presenter.UpdateView();
		}

		private void setToolStripsToPreferredSize()
		{
			toolStripEx2.Size = toolStripEx2.PreferredSize;
			toolStripEx3.Size = toolStripEx3.PreferredSize;
			toolStripEx4.Size = toolStripEx4.PreferredSize;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IMeetingGeneralView createMeetingGeneralView(IMeetingViewModel meetingViewModel)
		{
			var meetingGeneralView = new MeetingGeneralView(meetingViewModel, this) {Dock = DockStyle.Fill};
			meetingGeneralView.ParticipantSelectionRequested += composerParticipantSelectionRequested;
			
			return meetingGeneralView;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IMeetingImpactView getImpactView()
		{
			return new MeetingImpactView(_meetingComposerPresenter.Model, _meetingComposerPresenter.SchedulingScreenState, this, 
				_resourceOptimizationHelper, _skillPriorityProvider, _staffingCalculatorServiceFacade, _resourceCalculationContextFactory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IMeetingDetailView createMeetingSchedulesView(IMeetingViewModel model, SchedulingScreenState holder)
		{
			var meetingSchedulesView = new MeetingSchedulesView(model, holder, this);
			return meetingSchedulesView;
		}

		private void toolStripButtonImpactClick(object sender, EventArgs e)
		{
			changeView(ViewType.ImpactView);
		}

		private void backgroundWorkerLoadStateHolderDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			_meetingComposerPresenter.OnSchedulerStateHolderRequested();
		}

		private void backgroundWorkerLoadStateHolderRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;

			_meetingComposerPresenter.OnSchedulerStateHolderLoaded();
		}

		private void ensureSchedulesViewIsLoaded()
		{
			if (_meetingDetailViews.Count == 1)
				_meetingDetailViews.Add(createMeetingSchedulesView(_meetingComposerPresenter.Model, _meetingComposerPresenter.SchedulingScreenState));
		}
		
		private void ensureImpactViewIsLoaded()
		{
			ensureSchedulesViewIsLoaded();
			if (_meetingDetailViews.Count == 2)
				_meetingDetailViews.Add(getImpactView());
		}

		private void releaseManagedResources()
		{
			_meetingComposerPresenter.Dispose();
		}

		private void unhookEvents()
		{
			if (_meetingDetailViews.Count==0) return;
			var generalView = _meetingDetailViews[0] as MeetingGeneralView;
			if (generalView!=null)
			{
				generalView.ParticipantSelectionRequested -= composerParticipantSelectionRequested;
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
