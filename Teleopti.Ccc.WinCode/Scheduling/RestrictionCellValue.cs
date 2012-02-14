using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class RestrictionCellValue
    {
        private readonly IScheduleDay _schedulePart;
        private readonly bool _useRotation;
        private readonly bool _useAvailability;
        private readonly bool _useStudentAvailability;
        private readonly bool _usePreference;
        private readonly bool _useSchedule;

        public RestrictionCellValue(IScheduleDay schedulePart,
                                    bool useRotation,
                                    bool useAvailability,
                                    bool useStudentAvailability,
                                    bool usePreference,
                                    bool useSchedule)
        {
            _schedulePart = schedulePart;
            _useRotation = useRotation;
            _useAvailability = useAvailability;
            _useStudentAvailability = useStudentAvailability;
            _usePreference = usePreference;
            _useSchedule = useSchedule;
        }

        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }

        public bool UseRotation
        {
            get { return _useRotation; }
        }

        public bool UseAvailability
        {
            get { return _useAvailability; }
        }

        public bool UseStudentAvailability
        {
            get { return _useStudentAvailability; }
        }

        public bool UsePreference
        {
            get { return _usePreference; }
        }

        public bool UseSchedule
        {
            get { return _useSchedule; }
        }
    }
}