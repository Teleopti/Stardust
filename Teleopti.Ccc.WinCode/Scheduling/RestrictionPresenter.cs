using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class RestrictionPresenter : SchedulePresenterBase
    {
        private bool _useRotation;
        private bool _useAvailability;
        private bool _useStudent;
        private bool _usePreference;
        private bool _useSchedule;

        public RestrictionPresenter(IScheduleViewBase view, SchedulerStateHolder schedulerState, GridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback,
            defaultScheduleTag)
        {
        }

        public bool UseRotation
        {
            get { return _useRotation; }
            set { _useRotation = value; }
        }

        public bool UseAvailability
        {
            get { return _useAvailability; }
            set { _useAvailability = value; }
        }

        public bool UseStudent
        {
            get { return _useStudent; }
            set { _useStudent = value; }
        }

        public bool UsePreference
        {
            get { return _usePreference; }
            set { _usePreference = value; }
        }

        public bool UseSchedule
        {
            get { return _useSchedule; }
            set { _useSchedule = value; }
        }

        public override void QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
        {
            base.QueryCellInfo(sender, e);

            if (e.RowIndex > View.RowHeaders && e.ColIndex >= (int)ColumnType.StartScheduleColumns)
            {
                IScheduleDay part = (IScheduleDay) e.Style.CellValue;
                RestrictionCellValue value = new RestrictionCellValue(part,UseRotation, UseAvailability, UseStudent, UsePreference, UseSchedule);
                e.Style.CellValue = value;
                e.Style.CellType = "RestrictionCell";
            }
        }
    }
}