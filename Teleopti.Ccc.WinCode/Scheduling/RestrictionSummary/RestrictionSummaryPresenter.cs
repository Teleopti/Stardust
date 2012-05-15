using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public class RestrictionSummaryPresenter:SchedulePresenterBase
    {
        private readonly RestrictionSummaryModel _model;
        private readonly IRestrictionSummaryView _view;
        private readonly ClipHandler<IScheduleDay> _cellDataClipHandler;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public RestrictionSummaryPresenter(IRestrictionSummaryView view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, RestrictionSummaryModel model,
            IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback, defaultScheduleTag)
        {
            _model = model;
            _view = view;
            _cellDataClipHandler = clipHandler;
            _model.SchedulerLoadedPeriod = schedulerState.RequestedPeriod.Period();
        }
        public Dictionary<int, IPreferenceCellData> CellDataCollection
        {
            get { return _model.CellDataCollection; }
        }

        public void GetNextPeriod(AgentInfoHelper agentInfoHelper)
        {
            _model.GetNextPeriod(agentInfoHelper);
            _view.CellDataLoaded();
            _view.UpdateRowCount();
        }

        public ClipHandler<IScheduleDay> CellDataClipHandler
        {
            get { return _cellDataClipHandler; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public bool OnQueryCellInfo(int colIndex, int rowIndex, out IPreferenceCellData cellData)
        {
            cellData = new PreferenceCellData();
            if (rowIndex < 1 || colIndex < 1)
                return false;
            int currentCell = ((rowIndex - 1) * 7) + colIndex;

            if (CellDataCollection.TryGetValue(currentCell - 1, out cellData))
                return true;

            return false;
        }
        public override int RowCount
        {
            get
            {
                int rows = CellDataCollection.Count / 7;
                if (CellDataCollection.Count % 7 > 0)
                    rows = _view.RestrictionGridRowCount + 1;
                return rows;
            }
        }
        public override int ColCount
        {
            get
            {
                return 7;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "colIndex-1")]
        public string OnQueryColumnHeaderText(int colIndex)
        {
            int index = colIndex - 1;
            IPreferenceCellData cellData;
            CellDataCollection.TryGetValue(index, out cellData);
            if (cellData == null)
                return "";

            return _model.CurrentUICultureInfo().DateTimeFormat.GetDayName(_model.CurrentCultureInfo().Calendar.GetDayOfWeek(cellData.TheDate));
        }
        public override void QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColIndex < 0)
                return;

            e.Style.CellValue = "";
            IPreferenceCellData cellData;
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = OnQueryColumnHeaderText(e.ColIndex);
                return;
            }

            if (e.RowIndex > 0 && e.ColIndex == 0)
            {
                e.Style.CellType = "RestrictionWeekHeaderViewCellModel";
                e.Style.CellValue = OnQueryWeekHeader(e.RowIndex);
                e.Style.CultureInfo = _model.CurrentCultureInfo();
                return;
            }

            if (OnQueryCellInfo(e.ColIndex, e.RowIndex, out cellData))
            {
                e.Style.CellType = "RestrictionSummaryViewCellModel";
                if (e.Style.CellModel != null)
                    ((IRestrictionSummaryViewCellModel) e.Style.CellModel).RestrictionSummaryPresenter = this;
                e.Style.CellValue = cellData.SchedulePart;
                e.Style.Tag = cellData.TheDate;

                if (cellData.SchedulePart.FullAccess)
                    e.Style.CellTipText = ViewBaseHelper.GetToolTip(cellData.SchedulePart);

                if(cellData.ViolatesNightlyRest)
                {
                    var sb = new StringBuilder(e.Style.CellTipText);
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(Resources.RestrictionViolatesNightRest);
                    e.Style.CellTipText = sb.ToString();
                }
                if (cellData.NoShiftsCanBeFound)
                {
                    var sb = new StringBuilder(e.Style.CellTipText);
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(Resources.NoShiftsFound);
                    e.Style.CellTipText = sb.ToString();
                }
                return;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
        public IWeekHeaderCellData OnQueryWeekHeader(int rowIndex)
        {
            int stop = ((rowIndex - 1) * 7) + 7;
            var minTime = new TimeSpan();
            var maxTime = new TimeSpan();
            IPreferenceCellData cellData;

            CultureInfo culture = TeleoptiPrincipal.Current.Regional.Culture;

            Calendar myCal = culture.Calendar;

            CalendarWeekRule myCwr = culture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDow = culture.DateTimeFormat.FirstDayOfWeek;


            int weekNumber = 0;

            var weekMax = new TimeSpan(0);
            for (int index = ((rowIndex - 1) * 7); index < stop; index++)
            {
                if (index >= CellDataCollection.Count)
                    break;
                CellDataCollection.TryGetValue(index, out cellData);

                if (cellData.EffectiveRestriction != null)
                {
                    if (cellData.EffectiveRestriction.WorkTimeLimitation.HasValue())
                    {
                        if (cellData.EffectiveRestriction.WorkTimeLimitation.StartTime.HasValue)
                            minTime = minTime.Add(cellData.EffectiveRestriction.WorkTimeLimitation.StartTime.Value);
                        if (cellData.EffectiveRestriction.WorkTimeLimitation.EndTime.HasValue)
                            maxTime = maxTime.Add(cellData.EffectiveRestriction.WorkTimeLimitation.EndTime.Value);
                    }
                }

                weekMax = cellData.WeeklyMax;
                weekNumber = myCal.GetWeekOfYear(cellData.TheDate, myCwr, myFirstDow);
            }
            bool weekIsLegal = minTime <= weekMax;
            IWeekHeaderCellData weekHeaderCell = new WeekHeaderCellData(minTime, maxTime, !weekIsLegal, weekNumber);

            return weekHeaderCell;
        }
    }
}