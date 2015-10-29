﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class SchedulePresenterBase : ISchedulePresenterBase
    {
        private readonly IScheduleViewBase _view;

        private readonly ISchedulerStateHolder _schedulerState;

        public const int ProjectionHeight = 20;

        private readonly Dictionary<int, int> _colWeekMap = new Dictionary<int, int>();
        private readonly DateDateTimePeriodDictionary _timelineSpan = new DateDateTimePeriodDictionary();
        private readonly Dictionary<int, TimeSpan> _timelineHourPositionDictionary = new Dictionary<int, TimeSpan>();

        private readonly IGridlockManager _lockManager;
        private readonly ClipHandler<IScheduleDay> _clipHandlerSchedule;
        private readonly SchedulePartFilter _schedulePartFilter;
        private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SchedulePresenterBase(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager, 
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
        {
            VisibleWeeks = 4;
            IsAscendingSort = true;
            CurrentSortColumn = 1;
            _view = view;
            _schedulerState = schedulerState;
            _lockManager = lockManager;
            _clipHandlerSchedule = clipHandler;
            _schedulePartFilter = schedulePartFilter;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            DefaultScheduleTag = defaultScheduleTag;
            Now = DateTime.UtcNow;
            SelectedPeriod = SchedulerState.RequestedPeriod;
            SortCommand = new NoSortCommand(_schedulerState);
        }

        protected IScheduleViewBase View
        {
            get { return _view; }
        }

        public IScheduleTag DefaultScheduleTag { get; set; }

        /// <summary>
        /// Column week mapping dictionary
        /// </summary>
        public Dictionary<int, int> ColWeekMap
        {
            get { return _colWeekMap; }
        }

        public Dictionary<int, TimeSpan> TimelineHourPositions
        {
            get { return _timelineHourPositionDictionary; }
        }

        public ISchedulerStateHolder SchedulerState
        {
            get { return _schedulerState; }
        }

        public SchedulePartFilter SchedulePartFilter
        {
            get { return _schedulePartFilter; }
        }

        public DateTime Now { get; set; }

        /// <summary>
        /// Get days
        /// </summary>
        private int Days
        {
            get
            {
                TimeSpan timeSpan = SelectedPeriod.Period().ElapsedTime();
                return (int)(Math.Round(timeSpan.TotalDays));
            }
        }

        /// <summary>
        /// Number of columns
        /// </summary>
        public virtual int ColCount
        {
            get
            {
                return Days - 1 + (int)ColumnType.StartScheduleColumns;
            }
        }

        /// <summary>
        /// Number of rows
        /// </summary>
        public virtual int RowCount
        {
            get
            {
                return SchedulerState.FilteredPersonDictionary.Count + 1;
            }
        }

        /// <summary>
        /// Get dictionary with timespans(min start to max end) for each date
        /// </summary>
        public DateDateTimePeriodDictionary TimelineSpan
        {
            get { return _timelineSpan; }
        }

        /// <summary>
        /// Number of max weeks to show in a certain view
        /// </summary>
        public int VisibleWeeks { get; set; }

        public virtual IDateOnlyPeriodAsDateTimePeriod SelectedPeriod { get; set; }

        public IScheduleDay LastUnsavedSchedulePart { get; set; }

        public IGridlockManager LockManager
        {
            get { return _lockManager; }
        }

        public ClipHandler<IScheduleDay> ClipHandlerSchedule
        {
            get { return _clipHandlerSchedule; }
        }

        #region Sort

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private object columnTextFromPerson(IPerson person, ColumnType columnType)
        {
            var styleInfo = new GridStyleInfo();
            
                QueryOverviewStyleInfo(styleInfo, person, columnType);

                switch (columnType)
                {
                    case ColumnType.RowHeaderColumn:
                        return SchedulerState.CommonNameDescription.BuildCommonNameDescription(person);
                    default:
                        {
                            if (styleInfo.CellValue == null || styleInfo.CellValue.ToString() == "N/A")
                                return null;

                            return styleInfo.CellValue; 
                        }
                }
        }

        public bool IsAscendingSort { get; set; }

        public int CurrentSortColumn { get; set; }

        public IScheduleSortCommand SortCommand { get; set; }

        public void ApplyGridSort()
		{
			List<KeyValuePair<Guid, IPerson>> sortedFilteredPersonDictionary;
		
			var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);

			if (IsAscendingSort)
			{
				sortedFilteredPersonDictionary = SchedulerState.FilteredAgentsDictionary.OrderBy(p => columnTextFromPerson(p.Value, (ColumnType) CurrentSortColumn), comparer).ToList();
			}
			else
			{
				sortedFilteredPersonDictionary = SchedulerState.FilteredAgentsDictionary.OrderByDescending(p => columnTextFromPerson(p.Value, (ColumnType) CurrentSortColumn), comparer).ToList();	
			}

			SchedulerState.FilteredAgentsDictionary.Clear();
			foreach (var keyValuePair in sortedFilteredPersonDictionary)
			{
				SchedulerState.FilteredAgentsDictionary.Add(keyValuePair);
			}

		}

        /// <summary>
        /// Sort the column ascending or desceding
        /// </summary>
        /// <param name="column">Column to sort on</param>
        /// <returns>Returns true if the rows are reordered</returns>
        public bool SortColumn(int column)
        {
            List<KeyValuePair<Guid, IPerson>> sortedFilteredPersonDictionary;

            if (CurrentSortColumn != column || !IsAscendingSort)
            {
                var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);

				if ((ColumnType)column == ColumnType.CurrentContractTimeColumn || (ColumnType)column == ColumnType.TargetContractTimeColumn)
					comparer = new ContractTimeComparer(loggedOnCulture);

				if ((ColumnType)column == ColumnType.CurrentDayOffColumn || (ColumnType)column == ColumnType.TargetDayOffColumn)
					comparer = new DayOffCountComparer(loggedOnCulture);

                sortedFilteredPersonDictionary = SchedulerState.FilteredPersonDictionary.OrderBy(p => columnTextFromPerson(p.Value, (ColumnType)column), comparer).ToList();
		 
                IsAscendingSort = true;
            }
            else
            {
                var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);

				if ((ColumnType)column == ColumnType.CurrentContractTimeColumn || (ColumnType)column == ColumnType.TargetContractTimeColumn)
					comparer = new ContractTimeComparer(loggedOnCulture);

				if ((ColumnType)column == ColumnType.CurrentDayOffColumn || (ColumnType)column == ColumnType.TargetDayOffColumn)
					comparer = new DayOffCountComparer(loggedOnCulture);

				sortedFilteredPersonDictionary =
					SchedulerState.FilteredAgentsDictionary.OrderByDescending(p => columnTextFromPerson(p.Value, (ColumnType)column), comparer).ToList();

                IsAscendingSort = false;
            }
            CurrentSortColumn = column;
            SortCommand = new NoSortCommand(_schedulerState);

			if (sortedFilteredPersonDictionary.SequenceEqual(SchedulerState.FilteredAgentsDictionary))
			{
				return false;
			}

			SchedulerState.FilteredAgentsDictionary.Clear();
			foreach (var keyValuePair in sortedFilteredPersonDictionary)
			{
				SchedulerState.FilteredAgentsDictionary.Add(keyValuePair);
			}

            return true;
        }

        public void SortOnTime(DateOnly dateOnly)
        {
            SortCommand.Execute(dateOnly);
        }

        #endregion

        #region QueryStyleInfo

        /// <summary>
        /// Handler cell info.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: claess /moved by zoet
        /// Created date: 2007-11-15
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public virtual void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex - View.RowHeaders > SchedulerState.FilteredPersonDictionary.Count)
            {
                // Bug fix:
                // Select last row and change to a narrower filter
                return;
            }

            // to avoid recursive????????????
            e.Handled = true;

            // Overview header (top and sub header)
            CreateOverviewHeader(e);

            // Day sub header
            CreateDayHeader(e);

            // Week top header
            CreateWeekHeader(e);

            // Agent row header
            CreateAgentHeader(e);

            // Overview cell
            QueryOverviewCellInfo(e);

            // ISchedulePart cell
            querySchedulePartCellInfo(e);
        }

        private void querySchedulePartCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex > View.RowHeaders && e.ColIndex >= (int)ColumnType.StartScheduleColumns)
            {
                e.Style.MergeCell = GridMergeCellDirection.None;
                if (_schedulerState.FilteredPersonDictionary.Count > 0)
                {
                    IPerson agent = _schedulerState.FilteredPersonDictionary.ElementAt(e.RowIndex - (View.RowHeaders + 1)).Value;

					var localDate = _schedulerState.RequestedPeriod.DateOnlyPeriod.StartDate;
                    localDate = localDate.AddDays(e.ColIndex - (int)ColumnType.StartScheduleColumns);

                    IScheduleRange totalScheduleRange = _schedulerState.Schedules[agent];
                    IScheduleDay daySchedule = totalScheduleRange.ScheduledDay(localDate);
                    
                    if (!daySchedule.FullAccess)
                        _lockManager.AddLock(daySchedule, LockType.Authorization);
                    
                    if (!IsInsidePersonPeriod(daySchedule))
                        _lockManager.AddLock(daySchedule, LockType.OutsidePersonPeriod);

                    if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
                        if (!daySchedule.IsFullyPublished)
                            _lockManager.AddLock(daySchedule, LockType.UnpublishedSchedule);

                    FilterSchedulePart(daySchedule);

                    //set value to schedule day
                    e.Style.CellValue = daySchedule;
                    //set tag to local current date
                    e.Style.Tag = localDate;
                    //set tip text
                    if (daySchedule.FullAccess)
                        e.Style.CellTipText = ViewBaseHelper.GetToolTip(daySchedule);
                    //set background color
                    View.SetCellBackTextAndBackColor(e, localDate, true, false, daySchedule);
                }
            }
        }

        internal void CreateAgentHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex > View.RowHeaders && e.ColIndex == View.ColHeaders)
            {
                if (e.ColIndex == (int)ColumnType.RowHeaderColumn)
                {
                    //add person to tag and set text to name
                    if (_schedulerState.FilteredPersonDictionary.Count > 0)
                    {
                        e.Style.Tag = _schedulerState.FilteredPersonDictionary.ElementAt(e.RowIndex - (View.RowHeaders + 1)).Value;
                        e.Style.Text = _schedulerState.CommonAgentName((IPerson)e.Style.Tag);
                    }
                }
            }
        }

        internal static bool IsInsidePersonPeriod(IScheduleDay schedulePart)
        {
            return schedulePart.Person.IsAgent(schedulePart.DateOnlyAsPeriod.DateOnly);
        }

        /// <summary>
        /// CreateProjection a date header
        /// </summary>
        /// <param name="e"></param>
        protected virtual void CreateDayHeader(GridQueryCellInfoEventArgs e)
        {
            var columnType = (ColumnType)e.ColIndex;
            if (e.RowIndex == 1 && columnType >= ColumnType.StartScheduleColumns)
            {
                //text date header
                var localDate = SelectedPeriod.DateOnlyPeriod.StartDate.AddDays(e.ColIndex - (int)ColumnType.StartScheduleColumns);

                e.Style.Text = CultureInfo.CurrentCulture.Calendar.
                    GetDayOfMonth(localDate.Date).ToString(CultureInfo.CurrentCulture);

                e.Style.Tag = localDate;
                e.Style.CellTipText = View.DayHeaderTooltipText(e.Style, localDate);

                View.SetCellBackTextAndBackColor(e, localDate, false, true, null);
            }
        }

        /// <summary>
        /// Merge headers
        /// </summary>
        /// <remarks>
        /// Writes the same information in the header if its within the same week
        /// Headers will be merged if they show same info.
        /// </remarks>
        public virtual void MergeHeaders()
        {
            const int col = (int)ColumnType.StartScheduleColumns;
            DateTime startDate = SelectedPeriod.Period().StartDateTimeLocal(_schedulerState.TimeZoneInfo).Date;
            for (int i = 0; i < Days; i++)
            {
                ColWeekMap.Add(col + i, DateHelper.WeekNumber(startDate.AddDays(i), CultureInfo.CurrentCulture));
            }
        }

        private static void createOverviewTopHeader(GridQueryCellInfoEventArgs e)
        {
            var columnType = (ColumnType)e.ColIndex;
            if (columnType <= ColumnType.RowHeaderColumn)
            {
                // Empty
            }
            else if (columnType < ColumnType.StartTargetColumns)
            {
                e.Style.Text = Resources.Current;
            }
            else if (columnType < ColumnType.StartScheduleColumns)
            {
                e.Style.Text = Resources.Target;
            }
        }

        private static void createOverviewSubHeader(GridQueryCellInfoEventArgs e)
        {
            var columnType = (ColumnType)e.ColIndex;
            switch (columnType)
            {
                case ColumnType.CurrentContractTimeColumn:
                    e.Style.CellTipText = Resources.ContractScheduledTime;
                    e.Style.Text = Resources.ScheduledTime;
                    break;
                case ColumnType.CurrentDayOffColumn:
                    e.Style.CellTipText = Resources.ScheduledDaysOff;
                    e.Style.Text = Resources.ScheduledDaysOff;
                    break;
                case ColumnType.TargetContractTimeColumn:
                    e.Style.CellTipText = Resources.ContractTargetTime;
                    e.Style.Text = Resources.TargetTime;
                    break;
                case ColumnType.TargetDayOffColumn:
                    e.Style.CellTipText = Resources.TargetDaysOff;
                    e.Style.Text = Resources.TargetDaysOff;
                    break;
            }
        }

        protected static void CreateOverviewHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex == 0)
            {
                createOverviewTopHeader(e);
            }
            else if (e.RowIndex == 1)
            {
                createOverviewSubHeader(e);
            }
        }

        /// <summary>
        /// Create week top header
        /// </summary>
        /// <param name="e"></param>
		internal void CreateWeekHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex == 0)
            {
                if (e.ColIndex >= (int)ColumnType.StartScheduleColumns)
                {
                    int week;

                    if (ColWeekMap.TryGetValue(e.ColIndex, out week))
                    {
                        e.Style.WrapText = false;
                        var period = ViewBaseHelper.WeekHeaderDates(week, SelectedPeriod.DateOnlyPeriod);
	                    e.Style.Tag =
		                    new DateOnly(
			                    SelectedPeriod.Period()
				                    .StartDateTimeLocal(_schedulerState.TimeZoneInfo)
				                    .Date.AddDays(e.ColIndex - (int) ColumnType.StartScheduleColumns)
				                    .Date);
	                    e.Style.Text = string.Format(CultureInfo.CurrentCulture, Resources.WeekAbbreviationDot,
		                    week, period.StartDate.ToShortDateString());

                    }
                }
                e.Style.MergeCell = GridMergeCellDirection.ColumnsInRow;
            }
        }

        public virtual void QueryOverviewStyleInfo(GridStyleInfo styleInfo, IPerson person, ColumnType columnType)
        {
            // when running the sheduling on background thread we can get a conflict if we try to get that schedulepart
            if (!View.TheGrid.Enabled)
                return;

			if (_view.IsOverviewColumnsHidden)
				return;

            var period = _schedulerState.RequestedPeriod.DateOnlyPeriod;
            
            switch (columnType)
            {
                case ColumnType.CurrentContractTimeColumn:
                    ViewBaseHelper.StyleCurrentContractTimeCell(styleInfo, SchedulerState.Schedules[person], period);
                    break;
                case ColumnType.CurrentDayOffColumn:
                    ViewBaseHelper.StyleCurrentTotalDayOffCell(styleInfo, SchedulerState.Schedules[person], period);
                    break;
                case ColumnType.TargetContractTimeColumn:
            		var virtualSchedulePeriod = person.VirtualSchedulePeriod(period.StartDate);
					if (!virtualSchedulePeriod.IsValid)
						return;

					ViewBaseHelper.StyleTargetScheduleContractTimeCell(styleInfo, person, new DateOnlyPeriod(period.StartDate, period.EndDate),
																	   SchedulerState.SchedulingResultState,
																	   SchedulerState.Schedules[person]);
                    break;
                case ColumnType.TargetDayOffColumn:
					ViewBaseHelper.StyleTargetScheduleDaysOffCell(styleInfo, person, new DateOnlyPeriod(period.StartDate, period.EndDate), SchedulerState.Schedules[person]);
                    break;
            }
        }

        /// <summary>
        /// Handles setting of the OverviewColumns
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-10-16
        /// </remarks>
        internal void QueryOverviewCellInfo(GridQueryCellInfoEventArgs e)
        {
            //TODO:henrika 2008-10-14 refactor this check, this should not be called with these params
            // Happens when switching views (sometimes)
            if (!View.TheGrid.Enabled)
                return;

            if (SelectedPeriod.DateOnlyPeriod.StartDate > DateOnly.MinValue)
            {
                if (e.ColIndex > (int)ColumnType.RowHeaderColumn &&
                    e.ColIndex < (int)ColumnType.StartScheduleColumns &&
                    e.RowIndex > View.ColHeaders &&
                    SchedulerState.FilteredPersonDictionary.Count > 0 &&
                    (e.RowIndex - (View.RowHeaders + 1)) >= 0)
                {
                    IPerson person = SchedulerState.FilteredPersonDictionary.ElementAt(e.RowIndex - (View.RowHeaders + 1)).Value;
                    QueryOverviewStyleInfo(e.Style, person, (ColumnType)e.ColIndex);
                }
            }
        }

        #endregion


		protected void FilterSchedulePart(IScheduleDay schedulePart)
        {
            if (_schedulePartFilter == SchedulePartFilter.Meetings)
            {
                schedulePart.Clear<IPersonAbsence>();
                schedulePart.Clear<IPersonAssignment>();
                schedulePart.BusinessRuleResponseCollection.Clear();
            }
        }

        /// <summary>
        /// Modifies the schedule part.
        /// </summary>
        /// <param name="theParts">The part.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-08-14    
        /// </remarks>
        public bool ModifySchedulePart(IList<IScheduleDay> theParts)
        {
            for (int i = theParts.Count - 1; i >= 0; i--)
            {
                GridlockDictionary lockDictionary = _lockManager.Gridlocks(theParts[i].Person, theParts[i].DateOnlyAsPeriod.DateOnly);
                if (lockDictionary != null && lockDictionary.Count != 0)
                {
                    // if it only is one lock and that is WriteProtected AND the user is allowed to change those
                    // Don't remove it the user can change it
                    var gridlock = new Gridlock(theParts[i], LockType.WriteProtected);
                    if (lockDictionary.Count == 1 && lockDictionary.ContainsKey(gridlock.Key) && PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
                    {

                    }
                    else
                    {
                        theParts.RemoveAt(i);
                    }
                }
            }

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving parts");
            bool result;
            try
            {
                result = TryModify(theParts);
            }
            catch (Exception)
            {
                undoRedoContainer.RollbackBatch();
                throw;
            }
            
            undoRedoContainer.CommitBatch();
            
            return result;
        }

        public bool TryModify(IList<IScheduleDay> scheduleParts, IScheduleTag scheduleTag)
        {
            if(scheduleParts == null) throw new ArgumentNullException("scheduleParts");

            _view.TheGrid.Invalidate();
            var rulesToRun = _schedulerState.SchedulingResultState.GetRulesToRun();
            foreach (IScheduleDay part in scheduleParts)
            {
	            IPersonAssignment assignment = part.PersonAssignment();
				if (assignment != null)
                {
                    assignment.CheckRestrictions();
                }
            }

            foreach (var overriddenBusinessRule in _overriddenBusinessRulesHolder.OverriddenRules)
            {
                rulesToRun.Remove(overriddenBusinessRule.TypeOfRule);
            }
            var lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
            var lstBusinessRuleResponse = _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, scheduleParts, rulesToRun, _scheduleDayChangeCallback, new ScheduleTagSetter(scheduleTag));
            if (!lstBusinessRuleResponse.Any())
                return true;
            var handleBusinessRules = new HandleBusinessRules(View.HandleBusinessRuleResponse, View, _overriddenBusinessRulesHolder);
            lstBusinessRuleResponseToOverride.AddRange(handleBusinessRules.Handle(lstBusinessRuleResponse, lstBusinessRuleResponseToOverride));

            // try again with overriden
            if (lstBusinessRuleResponseToOverride.Count > 0)
            {
                lstBusinessRuleResponseToOverride.ForEach(rulesToRun.Remove);
                lstBusinessRuleResponse = _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, scheduleParts, rulesToRun, _scheduleDayChangeCallback, new ScheduleTagSetter(scheduleTag));
                lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
                foreach (var response in lstBusinessRuleResponse)
                {
                    if (!response.Overridden)
                        lstBusinessRuleResponseToOverride.Add(response);
                }
            }
            else
            {
                return false;
            }
            //if it's more than zero now. Cancel!!!
            if (lstBusinessRuleResponseToOverride.Count > 0)
            {
                // show a MessageBox, another not overridable rule (Mandatory) might have been found later in the SheduleRange
                // will probably not happen
                View.ShowErrorMessage(lstBusinessRuleResponse.First().Message, Resources.ViolationOfABusinessRule);
                return false;
            }

            return true;    
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool TryModify(IList<IScheduleDay> scheduleParts)
		{
		    return TryModify(scheduleParts, DefaultScheduleTag);
		}

        public void UpdateNoteFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;
            IList<IScheduleDay> theParts = new List<IScheduleDay> { LastUnsavedSchedulePart };

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving note");

            _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, theParts, NewBusinessRuleCollection.AllForScheduling(_schedulerState.SchedulingResultState), _scheduleDayChangeCallback, new ScheduleTagSetter(DefaultScheduleTag));

            undoRedoContainer.CommitBatch();
            LastUnsavedSchedulePart = null;
        }

        public void UpdatePublicNoteFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;
            IList<IScheduleDay> theParts = new List<IScheduleDay> { LastUnsavedSchedulePart };

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving public note");

            _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, theParts, NewBusinessRuleCollection.AllForScheduling(_schedulerState.SchedulingResultState), _scheduleDayChangeCallback, new ScheduleTagSetter(DefaultScheduleTag));

            undoRedoContainer.CommitBatch();
            LastUnsavedSchedulePart = null;
        }

        public void UpdateFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;

            try
            {
				IPersonAssignment assignment = LastUnsavedSchedulePart.PersonAssignment();
				if (assignment != null)
                {
                    assignment.CheckRestrictions();
                }
            }
            catch (ValidationException ex)
            {
                View.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, ex.Message), Resources.ValidationError);
                LastUnsavedSchedulePart = null;
                return;
            }
            if (!ModifySchedulePart(new List<IScheduleDay> { LastUnsavedSchedulePart }))
            {
                LastUnsavedSchedulePart = null;
                return;
            }

            LastUnsavedSchedulePart = null;
            View.OnPasteCompleted(); //This is probably not needed as we already do a ResourceCalculateMarkedDays
        }

        public void AddActivity(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod)
        {
            var command = new AddActivityCommand(_schedulerState,_view,this,schedules);
            if (defaultPeriod.HasValue)
                command.DefaultPeriod = defaultPeriod;

            try
            {
                command.Execute();
            }
            catch (ValidationException ex)
            {
				View.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, ex.Message), Resources.ValidationError);
            }
        }

        public void AddActivity()
        {
            AddActivity(null, null);
        }

        public void AddOvertime(IList<IMultiplicatorDefinitionSet> definitionSets)
        {
            AddOvertime(null, null, definitionSets);
        }

        public void AddOvertime(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod, IList<IMultiplicatorDefinitionSet> definitionSets)
        {
            var command = new AddOvertimeCommand(_schedulerState, _view, this, definitionSets, schedules, new EditableShiftMapper());
            if (defaultPeriod.HasValue)
            {
                command.DefaultPeriod = defaultPeriod;
                command.DefaultIsSet = true;
            }
            try
            {
                command.Execute();
            }
            catch (ValidationException ex)
            {
                View.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, ex.Message), Resources.ValidationError);
            }
        }

        public void AddAbsence(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod)
        {
			var command = new AddAbsenceCommand(_schedulerState, _view, this, schedules, PrincipalAuthorization.Instance())
                              {DefaultPeriod = defaultPeriod};
            command.Execute();
        }

        public void AddAbsence()
        {
            AddAbsence(null, null);
        }

        public void AddPersonalShift(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod)
        {
            var command = new AddPersonalShiftCommand(_schedulerState, _view, this, schedules);
            if (defaultPeriod.HasValue)
                command.DefaultPeriod = defaultPeriod;

            try
            {
                command.Execute();
            }
            catch (ValidationException validationException)
            {
                View.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, validationException.Message), Resources.ValidationError);
            }
        }

        public void AddPersonalShift()
        {
            AddPersonalShift(null, null);
        }

		
    }


    // added as bugfix: 15718
    public sealed class PersonNameComparer : IComparer<object>
    {
        private readonly CultureInfo _cultureInfo;

        public PersonNameComparer()
        {
            _cultureInfo = CultureInfo.CurrentCulture;
        }

        public PersonNameComparer(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo;
        }


        #region IComparer<Name> Members

        public int Compare(object x, object y)
        {
            if (x == null && y == null)
                return 0;
            
            if (x == null)
                return -1;

            if (y == null)
                return 1;

            string stringX = x.ToString();
            string stringY = y.ToString();

            return string.Compare(stringX, stringY, true, _cultureInfo);
        }

        #endregion
    }

	public sealed class ContractTimeComparer : IComparer<object>
	{
		private readonly CultureInfo _cultureInfo;

		public ContractTimeComparer(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo;
        }

		public int Compare(object x, object y)
		{
			if (x == null && y == null)
				return 0;

			if (x == null)
				return -1;

			if (y == null)
				return 1;

			if (x is TimeSpan && y is TimeSpan)
			{
				var timeSpanX = (TimeSpan) x;
				var timeSpanY = (TimeSpan) y;
				return TimeSpan.Compare(timeSpanX, timeSpanY);
			}
			
			var stringX = x.ToString();
			var stringY = y.ToString();
			return string.Compare(stringX, stringY, true, _cultureInfo);	
			
		}
	}

	public sealed class DayOffCountComparer : IComparer<object>
	{
		private readonly CultureInfo _cultureInfo;

		public DayOffCountComparer(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}

		public int Compare(object x, object y)
		{
			if (x == null && y == null)
				return 0;

			if (x == null)
				return -1;

			if (y == null)
				return 1;

			if (x is int && y is int)
			{
				var intX = (int)x;
				var intY = (int)y;
				return intX.CompareTo(intY);
			}

			var stringX = x.ToString();
			var stringY = y.ToString();
			return string.Compare(stringX, stringY, true, _cultureInfo);

		}
	}
}
