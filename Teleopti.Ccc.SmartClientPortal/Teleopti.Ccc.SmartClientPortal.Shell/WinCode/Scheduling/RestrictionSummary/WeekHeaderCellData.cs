using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary
{
    public class WeekHeaderCellData:IWeekHeaderCellData
    {
        private readonly TimeSpan _minimumWeekWorkTime;
        private readonly TimeSpan _maximumWeekWorkTime;
        private readonly bool _validated;
        private readonly bool _invalid;
        private readonly bool _alert;
        private readonly int _weekNumber;
        public WeekHeaderCellData(){}

        public WeekHeaderCellData(bool invalid)
        {
            _invalid = invalid;
        }

        public WeekHeaderCellData(TimeSpan minimumWeekWorkTime, TimeSpan maximumWeekWorkTime, bool alert, int weekNumber)
        {
            _minimumWeekWorkTime = minimumWeekWorkTime;
            _maximumWeekWorkTime = maximumWeekWorkTime;
            _validated = true;
            _alert = alert;
            _weekNumber = weekNumber;
        }

        public TimeSpan MinimumWeekWorkTime
        {
            get { return _minimumWeekWorkTime; }
        }

        public TimeSpan MaximumWeekWorkTime
        {
            get { return _maximumWeekWorkTime; }
        }

        public bool Validated
        {
            get { return _validated; }
        }

        public bool Invalid
        {
            get { return _invalid; }
        }

        public bool Alert
        {
            get { return _alert; }
        }

        public int WeekNumber
        {
            get { return _weekNumber; }
        }
    }
}