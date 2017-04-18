using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class RestrictionSchedulingOptions: SchedulingOptions
    {
        private bool _useScheduling;
        public bool UseScheduling
        {
            get { return _useScheduling; }
            set
            {
                if (value == false)
                    _useScheduling = false;

                _useScheduling = value;
            }
        }
    }
}