using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface ISchedulePresenterBase
    {
        IScheduleTag DefaultScheduleTag { get; set; }

        Dictionary<int, int> ColWeekMap { get; }

        ISchedulerStateHolder SchedulerState { get; }
        SchedulePartFilter SchedulePartFilter { get; }
        DateTime Now { get; set; }

        int ColCount { get; }
        int RowCount { get; }

        DateDateTimePeriodDictionary TimelineSpan { get; }

        int VisibleWeeks { get; set; }

        IDateOnlyPeriodAsDateTimePeriod SelectedPeriod { get; set; }
        IScheduleDay LastUnsavedSchedulePart { get; set; }
        IGridlockManager LockManager { get; }
        ClipHandler<IScheduleDay> ClipHandlerSchedule { get; }

        bool IsAscendingSort { get; }

        int CurrentSortColumn { get; }
        IScheduleSortCommand SortCommand { get; set; }

        bool SortColumn(int column);

        void SortOnTime(DateOnly dateOnly);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e);

        void MergeHeaders();

        void QueryOverviewStyleInfo(GridStyleInfo styleInfo, IPerson person, ColumnType columnType);

        bool ModifySchedulePart(IList<IScheduleDay> theParts);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        bool TryModify(IList<IScheduleDay> scheduleParts);
        bool TryModify(IList<IScheduleDay> scheduleParts, IScheduleTag scheduleTag);

        void UpdateNoteFromEditor();
        void UpdatePublicNoteFromEditor();
        void UpdateFromEditor();
        void AddActivity(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod, IActivity defaultActivity);
        void AddActivity();
        void AddOvertime(IList<IMultiplicatorDefinitionSet> definitionSets);
        void AddOvertime(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod, IList<IMultiplicatorDefinitionSet> definitionSets);
        void AddAbsence(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod);
        void AddAbsence();
        void AddPersonalShift(IList<IScheduleDay> schedules, DateTimePeriod? defaultPeriod);
        void AddPersonalShift();
    }
}