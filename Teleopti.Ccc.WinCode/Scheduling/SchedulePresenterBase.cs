using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface ISchedulePresenterBase
    {
        IScheduleTag DefaultScheduleTag { get; set; }

        /// <summary>
        /// Column week mapping dictionary
        /// </summary>
        Dictionary<int, int> ColWeekMap { get; }

        Dictionary<int, TimeSpan> TimelineHourPositions { get; }
        ISchedulerStateHolder SchedulerState { get; }
        SchedulePartFilter SchedulePartFilter { get; }
        DateTime Now { get; set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        int ColCount { get; }

        /// <summary>
        /// Number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Get dictionary with timespans(min start to max end) for each date
        /// </summary>
        DateDateTimePeriodDictionary TimelineSpan { get; }

        /// <summary>
        /// Number of max weeks to show in a certain view
        /// </summary>
        int VisibleWeeks { get; set; }

        IDateOnlyPeriodAsDateTimePeriod SelectedPeriod { get; set; }
        IScheduleDay LastUnsavedSchedulePart { get; set; }
        IGridlockManager LockManager { get; }
        ClipHandler<IScheduleDay> ClipHandlerSchedule { get; }

        /// <summary>
        /// Sort order for the current sorted column
        /// </summary>
        bool IsAscendingSort { get; }

        int CurrentSortColumn { get; }
        IScheduleSortCommand SortCommand { get; set; }

        /// <summary>
        /// Sort the column ascending or desceding
        /// </summary>
        /// <param name="column">Column to sort on</param>
        /// <returns>Returns true if the rows are reordered</returns>
        bool SortColumn(int column);

        void SortOnTime(DateOnly dateOnly);

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
        void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e);

        /// <summary>
        /// Merge headers
        /// </summary>
        /// <remarks>
        /// Writes the same information in the header if its within the same week
        /// Headers will be merged if they show same info.
        /// </remarks>
        void MergeHeaders();

        void QueryOverviewStyleInfo(GridStyleInfo styleInfo, IPerson person, ColumnType columnType);

        /// <summary>
        /// Modifies the schedule part.
        /// </summary>
        /// <param name="theParts">The part.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-08-14    
        /// </remarks>
        bool ModifySchedulePart(IList<IScheduleDay> theParts);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        bool TryModify(IList<IScheduleDay> scheduleParts);
        bool TryModify(IList<IScheduleDay> scheduleParts, IScheduleTag scheduleTag);

        void UpdateRestriction();
        void UpdateNoteFromEditor();
        void UpdatePublicNoteFromEditor();
        void UpdateFromEditor();
        void AddActivity(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod);
        void AddActivity();
        void AddOvertime(IList<IMultiplicatorDefinitionSet> definitionSets);
        void AddOvertime(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod, IList<IMultiplicatorDefinitionSet> definitionSets);
        void AddAbsence(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod);
        void AddAbsence();
        void AddPersonalShift(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod);
        void AddPersonalShift();
    }

    /// <summary>
    /// The ScheduleViewBase presenter in the MVP pattern
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class SchedulePresenterBase : ISchedulePresenterBase
    {
        private readonly IScheduleViewBase _view;

        private readonly ISchedulerStateHolder _schedulerState;

        public const int ProjectionHeight = 20;

        private readonly Dictionary<int, int> _colWeekMap = new Dictionary<int, int>();
        private readonly DateDateTimePeriodDictionary _timelineSpan = new DateDateTimePeriodDictionary();
        private readonly Dictionary<int, TimeSpan> _timelineHourPositionDictionary = new Dictionary<int, TimeSpan>();

        private int _visibleWeeks = 4;
        private int _currentSortColumn = 1;
        private bool _isAscendingSort = true;

        private readonly IGridlockManager _lockManager;
        private readonly ClipHandler<IScheduleDay> _clipHandlerSchedule;
        private readonly SchedulePartFilter _schedulePartFilter;
        private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private IScheduleTag _defaultScheduleTag;

        private IScheduleSortCommand _sortCommand;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SchedulePresenterBase(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager, 
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
        {
            _view = view;
            _schedulerState = schedulerState;
            _lockManager = lockManager;
            _clipHandlerSchedule = clipHandler;
            _schedulePartFilter = schedulePartFilter;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _defaultScheduleTag = defaultScheduleTag;
            Now = DateTime.UtcNow;
            SelectedPeriod = SchedulerState.RequestedPeriod;
            _sortCommand = new NoSortCommand(_schedulerState);
        }

        protected IScheduleViewBase View
        {
            get { return _view; }
        }

        public IScheduleTag DefaultScheduleTag
        {
            get { return _defaultScheduleTag; }
            set { _defaultScheduleTag = value; }
        }

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
        public int VisibleWeeks
        {
            get { return _visibleWeeks; }
            set { _visibleWeeks = value; }
        }

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
                            if (styleInfo.CellValue.ToString() == "N/A")
                                return null;

                            return styleInfo.CellValue; 
                        }
                }
        }

        /// <summary>
        /// Sort order for the current sorted column
        /// </summary>
        public bool IsAscendingSort
        {
            get
            {
                return _isAscendingSort;
            }
            set
            {
                _isAscendingSort = value;
            }
        }

        public int CurrentSortColumn
        {
            get { return _currentSortColumn; }
			set { _currentSortColumn = value; }
        }

        public IScheduleSortCommand SortCommand
        {
            get { return _sortCommand; }
            set { _sortCommand = value; }
        }

		public void ApplyGridSort()
		{
			List<KeyValuePair<Guid, IPerson>> sortedFilteredPersonDictionary;

			var loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;
			IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);

			if (IsAscendingSort)
				sortedFilteredPersonDictionary = SchedulerState.FilteredPersonDictionary.OrderBy(p => columnTextFromPerson(p.Value, (ColumnType)_currentSortColumn), comparer).ToList();
			else
				sortedFilteredPersonDictionary = SchedulerState.FilteredPersonDictionary.OrderByDescending(p => columnTextFromPerson(p.Value, (ColumnType)_currentSortColumn), comparer).ToList();


			SchedulerState.FilteredPersonDictionary.Clear();
			foreach (var keyValuePair in sortedFilteredPersonDictionary)
			{
				SchedulerState.FilteredPersonDictionary.Add(keyValuePair);
			}
		}

        /// <summary>
        /// Sort the column ascending or desceding
        /// </summary>
        /// <param name="column">Column to sort on</param>
        /// <returns>Returns true if the rows are reordered</returns>
        public bool SortColumn(int column)
        {
            // Resort _schedulerState.FilteredPersonDictionary
            List<KeyValuePair<Guid, IPerson>> sortedFilteredPersonDictionary;
            if (_currentSortColumn != column || !_isAscendingSort)
            {
                // removed as bugfix: 15718
                //sortedFilteredPersonDictionary = (from p in SchedulerState.FilteredPersonDictionary
                //                                  orderby ColumnTextFromPerson(p.Value, (ColumnType)column) ascending
                //                                  select p).ToList();

                // added as bugfix: 15718
                CultureInfo loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;
                IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);
                sortedFilteredPersonDictionary =
                    SchedulerState.FilteredPersonDictionary.OrderBy(p => columnTextFromPerson(p.Value, (ColumnType)column), comparer).ToList();
                
                IsAscendingSort = true;
            }
            else
            {
                // removed as bugfix: 15718
                //sortedFilteredPersonDictionary = (from p in SchedulerState.FilteredPersonDictionary
                //                                  orderby ColumnTextFromPerson(p.Value, (ColumnType)column) descending
                //                                  select p).ToList();

                // added as bugfix: 15718
                CultureInfo loggedOnCulture = TeleoptiPrincipal.Current.Regional.Culture;
                IComparer<object> comparer = new PersonNameComparer(loggedOnCulture);
                sortedFilteredPersonDictionary =
                    SchedulerState.FilteredPersonDictionary.OrderByDescending(p => columnTextFromPerson(p.Value, (ColumnType)column), comparer).ToList();
 
                IsAscendingSort = false;
            }
            _currentSortColumn = column;
            SortCommand = new NoSortCommand(_schedulerState);

            if (sortedFilteredPersonDictionary.SequenceEqual(SchedulerState.FilteredPersonDictionary))
            {
                return false;
            }

            SchedulerState.FilteredPersonDictionary.Clear();
            foreach (var keyValuePair in sortedFilteredPersonDictionary)
            {
                SchedulerState.FilteredPersonDictionary.Add(keyValuePair);
            }

            return true;
        }

        public void SortOnTime(DateOnly dateOnly)
        {
            _sortCommand.Execute(dateOnly);
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
                    View.SetCellBackTextAndBackColor(e, localDate.Date, true, false, daySchedule);
                    //set zorder
                    SetAssignmentZorder(daySchedule);
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
            return (schedulePart.Person.Period(schedulePart.DateOnlyAsPeriod.DateOnly) != null);
        }

        //To ensure that visible assignment is still on top if we change other assignments on that day
        internal static void SetAssignmentZorder(IScheduleDay part)
        {
            //check if we have set the zorder on any assignment
            var personAssignments = part.PersonAssignmentCollection();
            IPersonAssignment assignmentWithZOrderSet = personAssignments.FirstOrDefault(pa => pa.ZOrder != DateTime.MinValue);

            //if we havent set zorder we set first to have highest zorder
            IPersonAssignment firstPersonAssignment = personAssignments.FirstOrDefault();
            if (assignmentWithZOrderSet == null && firstPersonAssignment != null)
                firstPersonAssignment.ZOrder = DateTime.MinValue.AddTicks(1);

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
                    GetDayOfMonth(localDate).ToString(CultureInfo.CurrentCulture);

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
                        e.Style.Tag = new DateOnly(SelectedPeriod.Period().StartDateTimeLocal(_schedulerState.TimeZoneInfo).Date.AddDays(e.ColIndex - (int)ColumnType.StartScheduleColumns).Date);
                        e.Style.Text = string.Concat(Resources.WeekAbbreviationDot, " ", week.ToString(CultureInfo.CurrentCulture),
                                                     " ", period.StartDate.ToShortDateString(CultureInfo.CurrentCulture));

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

            if (SelectedPeriod.DateOnlyPeriod.StartDate != DateTime.MinValue)
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


        protected void FilterSchedulePart(ISchedulePart schedulePart)
        {
            if (_schedulePartFilter == SchedulePartFilter.Meetings)
            {
                schedulePart.Clear<IPersonAbsence>();
                schedulePart.Clear<IPersonAssignment>();
                schedulePart.PersonAssignmentConflictCollection.Clear();
                schedulePart.Clear<IPersonDayOff>();
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
            return ModifySchedulePart(theParts, false);
        }

        /// <summary>
        /// Modifies the schedule part without the undo container
        /// NOTE: It is only used in meeting where the save is already done.
        /// </summary>
        /// <param name="theParts">The part.</param>
        /// <param name="autoRedo">The undo commit should be done or not</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Asad
        /// Created date: 2013-10-08 
        /// </remarks>
        public  bool ModifySchedulePart(IList<IScheduleDay> theParts, bool autoRedo)
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
            if (autoRedo)
            {
                //only when meetings are added
                undoRedoContainer.RedoCollection();
                undoRedoContainer.Clear();
            }
                
            return result;
        }

        public bool TryModify(IList<IScheduleDay> scheduleParts, IScheduleTag scheduleTag)
        {
            if(scheduleParts == null) throw new ArgumentNullException("scheduleParts");

            _view.TheGrid.Invalidate();
            var rulesToRun = _schedulerState.SchedulingResultState.GetRulesToRun();
            foreach (IScheduleDay part in scheduleParts)
            {
                foreach (IPersonAssignment assignment in part.PersonAssignmentCollection())
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
            if (lstBusinessRuleResponse.Count() == 0)
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
		    return TryModify(scheduleParts, _defaultScheduleTag);
		}

        public void UpdateRestriction()
        {
            if (LastUnsavedSchedulePart == null) return;
            IList<IScheduleDay> theParts =  new List<IScheduleDay> { LastUnsavedSchedulePart };

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving restriction");

            _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, theParts, NewBusinessRuleCollection.AllForScheduling(_schedulerState.SchedulingResultState), _scheduleDayChangeCallback, new ScheduleTagSetter(_defaultScheduleTag));

            undoRedoContainer.CommitBatch();
            LastUnsavedSchedulePart = null;
        }

        public void UpdateNoteFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;
            IList<IScheduleDay> theParts = new List<IScheduleDay> { LastUnsavedSchedulePart };

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving note");

            _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, theParts, NewBusinessRuleCollection.AllForScheduling(_schedulerState.SchedulingResultState), _scheduleDayChangeCallback, new ScheduleTagSetter(_defaultScheduleTag));

            undoRedoContainer.CommitBatch();
            LastUnsavedSchedulePart = null;
        }

        public void UpdatePublicNoteFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;
            IList<IScheduleDay> theParts = new List<IScheduleDay> { LastUnsavedSchedulePart };

            IUndoRedoContainer undoRedoContainer = _schedulerState.UndoRedoContainer ?? new UndoRedoContainer(500);
            undoRedoContainer.CreateBatch("Saving public note");

            _schedulerState.Schedules.Modify(ScheduleModifier.Scheduler, theParts, NewBusinessRuleCollection.AllForScheduling(_schedulerState.SchedulingResultState), _scheduleDayChangeCallback, new ScheduleTagSetter(_defaultScheduleTag));

            undoRedoContainer.CommitBatch();
            LastUnsavedSchedulePart = null;
        }

        public void UpdateFromEditor()
        {
            if (LastUnsavedSchedulePart == null) return;

            try
            {
                foreach (IPersonAssignment assignment in LastUnsavedSchedulePart.PersonAssignmentCollection())
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
            var command = new AddOvertimeCommand(_schedulerState, _view, this, definitionSets, schedules);
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


    public class HandleBusinessRules
    {
        private readonly IHandleBusinessRuleResponse _handleBusinessRuleResponse;
        private readonly IViewBase _viewBase;
        private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;

        public HandleBusinessRules(IHandleBusinessRuleResponse handleBusinessRuleResponse, IViewBase viewBase, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder)
        {
            _handleBusinessRuleResponse = handleBusinessRuleResponse;
            _viewBase = viewBase;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
        }

        public IEnumerable<IBusinessRuleResponse> Handle(IEnumerable<IBusinessRuleResponse> listBusinessRuleResponse, IList<IBusinessRuleResponse> listBusinessRuleResponseToOverride)
        {
            var ret = new List<IBusinessRuleResponse>(listBusinessRuleResponseToOverride);
            var internalList = new List<IBusinessRuleResponse>();
            foreach (var businessRuleResponse in listBusinessRuleResponse)
            {
                if(!businessRuleResponse.Overridden)
                    internalList.Add(businessRuleResponse);
            }
            if (!internalList.IsEmpty() && listBusinessRuleResponseToOverride.Count == 0)
            {
                foreach (IBusinessRuleResponse response in internalList)
                {
                    if (response.Mandatory)
                    {
                        _viewBase.ShowErrorMessage(response.Message, Resources.ViolationOfABusinessRule);
                        return ret;
                    }
                }
                //show dialog to override rules
                _handleBusinessRuleResponse.SetResponse(internalList);
                if (_handleBusinessRuleResponse.DialogResult == DialogResult.Cancel)
                {
                    return ret;
                }
                // we want to override them
                foreach (IBusinessRuleResponse response in internalList)
                {
                    response.Overridden = true;
                    if(_handleBusinessRuleResponse.ApplyToAll)
                        _overriddenBusinessRulesHolder.AddOverriddenRule(response);
                }

                return internalList.Concat(ret);
            }
            return ret;
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
}
