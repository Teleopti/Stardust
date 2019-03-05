using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	public partial class MeetingImpactView : BaseUserControl, IMeetingImpactView
	{
		private readonly MeetingImpactPresenter _presenter;
		private readonly MeetingComposerView _meetingComposerView;
		private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private SkillIntraDayGridControl _skillIntradayGridControl;
		private readonly TransparentMeetingMeetingControl _transparentMeetingMeetingControl;
		private readonly MeetingStateHolderLoaderHelper _meetingStateHolderLoaderHelper;

		public MeetingImpactView()
		{
			InitializeComponent();
		}

		public MeetingImpactView(IMeetingViewModel meetingViewModel, 
								SchedulingScreenState schedulerStateHolder, 
								MeetingComposerView meetingComposerView, 
								IResourceCalculation resourceOptimizationHelper,
								ISkillPriorityProvider skillPriorityProvider,
								IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade,
								CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
								ITimeZoneGuard timeZoneGuard)
			: this()
		{
			_transparentMeetingMeetingControl = new TransparentMeetingMeetingControl();
			_skillIntradayGridControl = new SkillIntraDayGridControl("MeetingSkillIntradayGridAndChart", _skillPriorityProvider, _timeZoneGuard);

			_meetingComposerView = meetingComposerView;
			_skillPriorityProvider = skillPriorityProvider;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			_timeZoneGuard = timeZoneGuard;
			outlookTimePickerStartTime.CreateAndBindList();
			outlookTimePickerEndTime.CreateAndBindList();

			office2007OutlookTimePickerStartSlotPeriod.CreateAndBindList();
			office2007OutlookTimePickerEndSlotPeriod.CreateAndBindList();

			var stateHolderLoader = new SchedulerStateLoader(schedulerStateHolder, new RepositoryFactory(), UnitOfWorkFactory.Current, new LazyLoadingManagerWrapper(), new ScheduleStorageFactory(PersonAssignmentRepository.DONT_USE_CTOR(CurrentUnitOfWork.Make())));
			var slotCalculator = new MeetingSlotImpactCalculator(schedulerStateHolder.SchedulerStateHolder.SchedulingResultState, new AllLayersAreInWorkTimeSpecification());
			var slotFinder = new BestSlotForMeetingFinder(slotCalculator);
			var decider = new PeopleAndSkillLoaderDecider(PersonRepository.DONT_USE_CTOR(new FromFactory(() =>UnitOfWorkFactory.Current), null, null), new PairMatrixService<Guid>(new PairDictionaryFactory<Guid>()));
			var gridHandler = new MeetingImpactSkillGridHandler(this, meetingViewModel, schedulerStateHolder.SchedulerStateHolder,
																UnitOfWorkFactory.Current, decider);
			var transparentWindowHandler = new MeetingImpactTransparentWindowHandler(this, meetingViewModel,
																					 schedulerStateHolder.SchedulerStateHolder.SchedulingResultState);
			_meetingStateHolderLoaderHelper = new MeetingStateHolderLoaderHelper(decider, schedulerStateHolder.SchedulerStateHolder, stateHolderLoader, UnitOfWorkFactory.Current, _staffingCalculatorServiceFacade, _timeZoneGuard);
			_presenter = new MeetingImpactPresenter(schedulerStateHolder.SchedulerStateHolder, this, meetingViewModel, _meetingStateHolderLoaderHelper,
				slotFinder, new MeetingImpactCalculator(schedulerStateHolder.SchedulerStateHolder, resourceOptimizationHelper, meetingViewModel.Meeting, resourceCalculationContextFactory),
				gridHandler, transparentWindowHandler, UnitOfWorkFactory.Current);
			SetTexts();

			dateTimePickerAdvStartDate.ValueChanged += meetingDateChanged;
			dateTimePickerAdvStartDate.PopupClosed += meetingDateChanged;

			tabControlSkillResultGrid.TabPages.Clear();
			tabControlSkillResultGrid.ImageList = imageListSkillTypeIcons;

			tabControlSkillResultGrid.SelectedIndexChanged += tabControlSkillResultGridSelectedIndexChanged;
			tabControlSkillResultGrid.SizeChanged += TabControlSkillResultGridSizeChanged;

			outlookTimePickerStartTime.Leave += outlookTimePickerStartTimeLeave;
			outlookTimePickerEndTime.Leave += outlookTimePickerEndTimeLeave;

			outlookTimePickerStartTime.KeyDown += outlookTimePickerStartTimeKeyDown;
			outlookTimePickerEndTime.KeyDown += outlookTimePickerEndTimeKeyDown;

			outlookTimePickerStartTime.SelectedIndexChanged += outlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged += outlookTimePickerEndTimeSelectedIndexChanged;

			office2007OutlookTimePickerStartSlotPeriod.Leave += office2007OutlookTimePickerStartSlotPeriodLeave;
			office2007OutlookTimePickerStartSlotPeriod.KeyDown += office2007OutlookTimePickerStartSlotPeriodKeyDown;

			office2007OutlookTimePickerEndSlotPeriod.Leave += office2007OutlookTimePickerEndSlotPeriodLeave;
			office2007OutlookTimePickerEndSlotPeriod.KeyDown += office2007OutlookTimePickerEndSlotPeriodKeyDown;

			dateTimePickerAdvEndDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdvStartDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdvEndSlotPeriod.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdvStartSlotPeriod.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			
			dateTimePickerAdvStartDate.SetSafeBoundary();
			dateTimePickerAdvEndDate.SetSafeBoundary();
			
			dateTimePickerAdvStartSlotPeriod.SetSafeBoundary();
			dateTimePickerAdvEndSlotPeriod.SetSafeBoundary();

			dateTimePickerAdvEndSlotPeriod.ValueChanged += dateTimePickerAdvEndSlotPeriodValueChanged;
			dateTimePickerAdvStartSlotPeriod.ValueChanged += dateTimePickerAdvStartSlotPeriodValueChanged;
			Paint += meetingImpactViewPaint;

			office2007OutlookTimePickerStartSlotPeriod.Text = dateTimePickerAdvStartSlotPeriod.MinValue.ToShortTimeString();
			office2007OutlookTimePickerEndSlotPeriod.Text = dateTimePickerAdvEndSlotPeriod.MinValue.ToShortTimeString();

		}

		void meetingImpactViewPaint(object sender, EventArgs e)
		{
			_transparentMeetingMeetingControl.Validate();
		}

		public void TabControlSkillResultGridSizeChanged(object sender, EventArgs e)
		{
			_presenter.UpdateMeetingControl();
		}

		void tabControlSkillResultGridSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.SkillTabChange();
		}

		public void ScrollMeetingIntoView(int pos)
		{
			_skillIntradayGridControl.ScrollCellInView(0, pos);
		}

		public void PositionControl()
		{
			var tab = tabControlSkillResultGrid.TabPages[tabControlSkillResultGrid.SelectedIndex];
			tab.Controls.Add(_skillIntradayGridControl);
			_skillIntradayGridControl.Dock = DockStyle.Fill;
			_presenter.UpdateMeetingControl();

		}

		public void RemoveAllSkillTabs()
		{
			tabControlSkillResultGrid.TabPages.Clear();
		}

		public void ClearTabPages()
		{
			foreach (TabPageAdv tabPage in tabControlSkillResultGrid.TabPages)
			{
				tabPage.Controls.Clear();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void DrawIntraday(ISkill skill, ISchedulerStateHolder schedulerStateHolder, IList<ISkillStaffPeriod> skillStaffPeriods)
		{
			autoLabelMeetingDate.Text = dateTimePickerAdvStartDate.Value.ToShortDateString();
			if (_skillIntradayGridControl != null)
				_skillIntradayGridControl.LeftColChanged -= skillIntradayGridControlLeftColChanged;

			_skillIntradayGridControl = new SkillIntraDayGridControl("SchedulerSkillIntradayGridAndChart", _skillPriorityProvider, TimeZoneGuardForDesktop_DONOTUSE.Instance_DONTUSE) { DefaultColWidth = 45 };
			_skillIntradayGridControl.SetupDataSource(skillStaffPeriods, skill, schedulerStateHolder);
			_skillIntradayGridControl.TurnoffHelp();
			_skillIntradayGridControl.SetRowsAndCols();
			_skillIntradayGridControl.ResizeColsBehavior = GridResizeCellsBehavior.None;

			_skillIntradayGridControl.LeftColChanged += skillIntradayGridControlLeftColChanged;
		}

		void skillIntradayGridControlLeftColChanged(object sender, GridRowColIndexChangedEventArgs e)
		{
			_presenter.OnLeftColChanged();
		}

		public bool IsRightToLeft
		{
			get { return _skillIntradayGridControl.IsRightToLeft(); }
		}

		public bool FindButtonEnabled
		{
			get { return buttonAdvPickBest.Enabled; }
			set { buttonAdvPickBest.Enabled = value; }
		}

		public void RefreshMeetingControl()
		{
			_transparentMeetingMeetingControl.RefreshControl();
			Invalidate(true);
			Update();
			_skillIntradayGridControl.ColStyles[0].Enabled = true;
			//_transparentMeetingMeetingControl.RefreshControl();	
		}

		public int GetCurrentHScrollPixelPos
		{
			get { return _skillIntradayGridControl.GetCurrentHScrollPixelPos(); }
		}

		public int IntervalsTotalWidth
		{
			get { return _skillIntradayGridControl.ColWidths.GetTotal(1, _skillIntradayGridControl.ColCount); }
		}

		public int ColsHeaderWidth
		{
			get { return _skillIntradayGridControl.ColWidths.GetSize(0); }
		}

		public int RowsHeight
		{
			get { return _skillIntradayGridControl.RowHeights.GetTotal(1, _skillIntradayGridControl.RowCount); }
		}

		public int RowHeaderHeight
		{
			get { return _skillIntradayGridControl.RowHeights.GetSize(0); }
		}

		public TimeSpan IntervalStartValue()
		{
			return _skillIntradayGridControl.StartInterval();
		}

		public bool HasStartInterval()
		{
			if (_skillIntradayGridControl.StartInterval() != TimeSpan.MinValue)
				return true;

			return false;
		}

		public int GridColCount
		{
			get { return _skillIntradayGridControl.ColCount; }
		}

		public int ClientRectangleLeft
		{
			get { return _skillIntradayGridControl.ClientRectangle.Left; }
		}

		public int ClientRectangleRight
		{
			get { return _skillIntradayGridControl.ClientRectangle.Right; }
		}

		public int ClientRectangleTop
		{
			get { return _skillIntradayGridControl.ClientRectangle.Top; }
		}

		public object ResultGrid
		{
			get { return _skillIntradayGridControl; }
		}

		public void ShowMeeting(TransparentMeetingControlModel transparentMeetingControlModel, TransparentControlMeetingHelper transparentControlMeetingHelper)
		{
			if (transparentMeetingControlModel != null)
				transparentMeetingControlModel.Parent = _skillIntradayGridControl;

			_transparentMeetingMeetingControl.InitControl(transparentMeetingControlModel, transparentControlMeetingHelper);
			_transparentMeetingMeetingControl.Show();

			_transparentMeetingMeetingControl.InvalidateParent();
			_skillIntradayGridControl.RefreshGrid();

			var rc = new Rectangle(0, _skillIntradayGridControl.ClientRectangle.Top, ColsHeaderWidth, _skillIntradayGridControl.ClientRectangle.Height);
			//_skillIntradayGridControl.Invalidate(rc, true);
			_skillIntradayGridControl.Invalidate(rc);

			RefreshMeetingControl();
		}

		public ISkill SelectedSkill()
		{
			if (tabControlSkillResultGrid.SelectedIndex >= 0)
			{
				var tab = tabControlSkillResultGrid.TabPages[tabControlSkillResultGrid.SelectedIndex];
				var skill = (ISkill)tab.Tag;

				return skill;
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void AddSkillTab(string name, string description, int imageIndex, ISkill skill)
		{
			var tab = ColorHelper.CreateTabPage(name, description);
			tab.Tag = skill;
			tab.ImageIndex = imageIndex;
			tabControlSkillResultGrid.TabPages.Add(tab);
		}

		void meetingDateChanged(object sender, EventArgs e)
		{
			_presenter.MeetingDateChanged();
		}

		public void OnParticipantsSet()
		{
			tabControlSkillResultGrid.SelectedIndexChanged -= tabControlSkillResultGridSelectedIndexChanged;
			_presenter.UpdateView();
			tabControlSkillResultGrid.SelectedIndexChanged += tabControlSkillResultGridSelectedIndexChanged;
		}

		public void OnDisableWhileLoadingStateHolder()
		{
		}

		public void OnEnableAfterLoadingStateHolder()
		{
		}

		public void OnMeetingDatesChanged()
		{
			//MeetingDateChanged(null, null);
		}

		public void OnMeetingTimeChanged()
		{
			//_presenter.MeetingTimeChanged();
		}

		public IMeetingDetailPresenter Presenter
		{
			get { return _presenter; }
		}

		public void ResetSelection()
		{
			tabControlSkillResultGrid.Select();
		}

		public void SetStartDate(DateOnly startDate)
		{
			dateTimePickerAdvStartDate.Value = startDate.Date;
			autoLabelMeetingDate.Text = startDate.ToShortDateString(CultureInfo.CurrentCulture);
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

		public void ShowWaiting()
		{
			_meetingComposerView.DisableWhileLoadingStateHolder();
			Enabled = false;
			progressBarLoading.Visible = true;
			Cursor = Cursors.WaitCursor;
		}

		public void HideWaiting()
		{
			_meetingComposerView.EnableAfterLoadingStateHolder();
			Enabled = true;
			progressBarLoading.Visible = false;
			Cursor = Cursors.Default;
		}

		public DateTime BestSlotSearchPeriodStart
		{
			get
			{
				var date = dateTimePickerAdvStartSlotPeriod.Value;
				return date.Add(office2007OutlookTimePickerStartSlotPeriod.TimeValue());
			}
		}

		public DateTime BestSlotSearchPeriodEnd
		{
			get
			{
				var date = dateTimePickerAdvEndSlotPeriod.Value;
				return date.Add(office2007OutlookTimePickerEndSlotPeriod.TimeValue());
			}
		}

		public DateOnly StartDate
		{
			get { return new DateOnly(dateTimePickerAdvStartDate.Value); }
		}

		public DateOnly EndDate
		{
			get { return new DateOnly(dateTimePickerAdvEndDate.Value); }
		}

		public TimeSpan StartTime
		{
			get { return outlookTimePickerStartTime.TimeValue(); }
		}

		public TimeSpan EndTime
		{
			get { return outlookTimePickerEndTime.TimeValue(); }
		}

		public void SetSearchStartDate(DateOnly startDate)
		{
			dateTimePickerAdvStartSlotPeriod.Value = startDate.Date;
		}

		public void SetSearchEndDate(DateOnly endDate)
		{
			dateTimePickerAdvEndSlotPeriod.Value = endDate.Date;
		}

		public void SetSearchInfo(string searchInfo)
		{
			autoLabelPickResult.Text = searchInfo;
		}

		public void SetPreviousState(bool state)
		{
			buttonAdvPrevious.Enabled = state;
		}

		public void SetNextState(bool state)
		{
			buttonAdvNext.Enabled = state;
		}

		private void buttonAdvPickBestClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			buttonAdvPickBest.Cursor = Cursors.WaitCursor;

			_presenter.FindBestMeetingSlot();

			Cursor = Cursors.Default;
			buttonAdvPickBest.Cursor = Cursors.Default;
		}

		private void buttonAdvPreviousClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			buttonAdvPrevious.Cursor = Cursors.WaitCursor;

			_presenter.GoToPreviousSlot();

			Cursor = Cursors.Default;
			buttonAdvPrevious.Cursor = Cursors.Default;
		}

		private void buttonAdvNextClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			buttonAdvNext.Cursor = Cursors.WaitCursor;

			_presenter.GoToNextSlot();

			Cursor = Cursors.Default;
			buttonAdvNext.Cursor = Cursors.Default;
		}

		void outlookTimePickerStartTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (outlookTimePickerStartTime.Disposing)
				return;
			_presenter.OnMeetingTimeChange(outlookTimePickerStartTime.Text);
		}

		void outlookTimePickerEndTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (outlookTimePickerEndTime.Disposing)
				return;
			_presenter.OnMeetingTimeChange(outlookTimePickerEndTime.Text);
		}

		void outlookTimePickerStartTimeKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				_presenter.OnMeetingTimeChange(outlookTimePickerStartTime.Text);
		}

		void outlookTimePickerEndTimeKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				_presenter.OnMeetingTimeChange(outlookTimePickerEndTime.Text);
		}

		void outlookTimePickerStartTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnMeetingTimeChange(outlookTimePickerStartTime.Text);
		}

		void outlookTimePickerEndTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnMeetingTimeChange(outlookTimePickerEndTime.Text);
		}

		void office2007OutlookTimePickerEndSlotPeriodKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				_presenter.OnSlotTimeChange(office2007OutlookTimePickerEndSlotPeriod.Text);
		}

		void office2007OutlookTimePickerStartSlotPeriodKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				_presenter.OnSlotTimeChange(office2007OutlookTimePickerStartSlotPeriod.Text);
		}

		void office2007OutlookTimePickerEndSlotPeriodLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (office2007OutlookTimePickerEndSlotPeriod.Disposing)
				return;
			_presenter.OnSlotTimeChange(office2007OutlookTimePickerEndSlotPeriod.Text);
		}

		void office2007OutlookTimePickerStartSlotPeriodLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (office2007OutlookTimePickerStartSlotPeriod.Disposing)
				return;
			_presenter.OnSlotTimeChange(office2007OutlookTimePickerStartSlotPeriod.Text);
		}

		public void SetSlotStartTime(TimeSpan timeSpan)
		{
			office2007OutlookTimePickerStartSlotPeriod.SetTimeValue(timeSpan);
		}

		public void SetSlotEndTime(TimeSpan timeSpan)
		{
			office2007OutlookTimePickerEndSlotPeriod.SetTimeValue(timeSpan);
		}

		void dateTimePickerAdvEndSlotPeriodValueChanged(object sender, EventArgs e)
		{
			_presenter.OnSlotDateChange(dateTimePickerAdvStartSlotPeriod.Value, dateTimePickerAdvEndSlotPeriod.Value);
		}

		void dateTimePickerAdvStartSlotPeriodValueChanged(object sender, EventArgs e)
		{
			_presenter.OnSlotDateChange(dateTimePickerAdvStartSlotPeriod.Value, dateTimePickerAdvEndSlotPeriod.Value);
		}

		public void SetSlotEndDate(DateTime dateTime)
		{
			dateTimePickerAdvEndSlotPeriod.Value = dateTime;
		}

		public void SetSlotStartDate(DateTime dateTime)
		{
			dateTimePickerAdvStartSlotPeriod.Value = dateTime;
		}

		public string GetStartTimeText
		{
			get { return outlookTimePickerStartTime.Text; }
		}

		public string GetEndTimeText
		{
			get { return outlookTimePickerEndTime.Text; }
		}
	}
}
