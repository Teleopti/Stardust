using System;
using System.Globalization;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class DayPresenterNew : SchedulePresenterBase
    {
        private readonly ISchedulerStateHolder _schedulerState;
        private readonly IGridlockManager _lockManager;
        private readonly IDayPresenterScaleCalculator _scaleCalculator;
        private DateOnly _selectedDate;
        private DateTimePeriod _scalePeriod;
        private const int timeLineHeaderIndex = 1; 

        public DayPresenterNew(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager, 
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IDayPresenterScaleCalculator scaleCalculator, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer)
        {
            _schedulerState = schedulerState;
            _lockManager = lockManager;
            _scaleCalculator = scaleCalculator;
        }

        public override int ColCount => (int)ColumnType.StartScheduleColumns;

		public DateTimePeriod ScalePeriod => _scalePeriod;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public override void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex - View.RowHeaders > SchedulerState.FilteredCombinedAgentsDictionary.Count)
            {
                // Bug fix:
                // Select last row and change to a narrower filter
                return;
            }

            // Overview header (top and sub header)
            CreateOverviewHeader(e);

            // Day sub header
            CreateDayHeader(e);

            //// Week top header
            //CreateWeekHeader(e);

            // Agent row header
            CreateAgentHeader(e);

            // Overview cell
            QueryOverviewCellInfo(e);

            // ISchedulePart cell
            QuerySchedulePartCellInfo(e);

            e.Handled = true;
        }

        public void SelectDate(DateOnly dateOnly)
        {
            _selectedDate = dateOnly;
            _scalePeriod = _scaleCalculator.CalculateScalePeriod(_schedulerState, _selectedDate);
            SelectedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly,dateOnly.AddDays(1)), TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
        }



        protected override void CreateDayHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < (int)ColumnType.StartScheduleColumns)
            {
                return;
            }

            if (e.RowIndex == 0)
            {
                // Text date header;
				var dayOfWeek = _selectedDate.DayOfWeek;
				string dayOfWeekName;
				switch (dayOfWeek)
				{
					case DayOfWeek.Monday: dayOfWeekName = Resources.Monday; break;
					case DayOfWeek.Tuesday: dayOfWeekName = Resources.Tuesday; break;
					case DayOfWeek.Wednesday: dayOfWeekName = Resources.Wednesday; break;
					case DayOfWeek.Thursday: dayOfWeekName = Resources.Thursday; break;
					case DayOfWeek.Friday: dayOfWeekName = Resources.Friday; break;
					case DayOfWeek.Saturday: dayOfWeekName = Resources.Saturday; break;
					case DayOfWeek.Sunday: dayOfWeekName = Resources.Sunday; break;
					default: dayOfWeekName = Resources.NA; break;
				}
				e.Style.Text = dayOfWeekName + " " + _selectedDate.ToShortDateString();
                e.Style.Tag = _selectedDate;
                e.Style.CellTipText =
                    DateHelper.WeekNumber(_selectedDate.Date, CultureInfo.CurrentCulture)
                        .ToString(CultureInfo.CurrentCulture);

                View.SetCellBackTextAndBackColor(e, _selectedDate, false, true, null);
            }
            else if (e.RowIndex == timeLineHeaderIndex)
            {
                e.Style.Tag = _selectedDate;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected void QuerySchedulePartCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex > View.RowHeaders && e.ColIndex >= (int)ColumnType.StartScheduleColumns)
            {
                e.Style.MergeCell = GridMergeCellDirection.None;
                if (_schedulerState.FilteredCombinedAgentsDictionary.Count > 0)
                {
					var agent = _schedulerState.FilteredCombinedAgentsDictionary.Values.ElementAt(e.RowIndex - (View.RowHeaders + 1));

					IScheduleRange totalScheduleRange = _schedulerState.Schedules[agent];
                    IScheduleDay daySchedule = totalScheduleRange.ScheduledDay(_selectedDate);

                    if (!daySchedule.FullAccess)
                        _lockManager.AddLock(daySchedule, LockType.Authorization);

                    if (!IsInsidePersonPeriod(daySchedule))
                        _lockManager.AddLock(daySchedule, LockType.OutsidePersonPeriod);

                    if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
                        if (!daySchedule.IsFullyPublished)
                            _lockManager.AddLock(daySchedule, LockType.UnpublishedSchedule);

                    FilterSchedulePart(daySchedule);

                    //set value to schedule day
                    e.Style.CellValue = daySchedule;
                    //set tag to local current date
                    e.Style.Tag = _selectedDate;
                    //set tip text
                    if (daySchedule.FullAccess)
                        e.Style.CellTipText = ViewBaseHelper.GetToolTip(daySchedule);
                    //set background color
                    View.SetCellBackTextAndBackColor(e, _selectedDate, true, false, daySchedule);
                }
            }
        }
    }
}