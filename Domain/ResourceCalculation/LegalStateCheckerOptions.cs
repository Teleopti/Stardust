using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class LegalStateCheckerOptions : ILegalStateCheckerOptions
    {
        private bool _useRotation;
        private bool _useAvailability;
        private bool _useStudent;
        private bool _usePreference;
        private bool _useSchedule;

        public bool UseRotations
        {
            get { return _useRotation; }
            set { _useRotation = value; }
        }

        public bool UseAvailability
        {
            get { return _useAvailability; }
            set { _useAvailability = value; }
        }

        public bool UseStudentAvailability
        {
            get { return _useStudent; }
            set { _useStudent = value; }
        }

        public bool UsePreferences
        {
            get { return _usePreference; }
            set { _usePreference = value; }
        }

        public bool UseSchedule
        {
            get { return _useSchedule; }
            set { _useSchedule = value; }
        }
    }
}