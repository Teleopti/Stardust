using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class AbsenceRequestOpenRollingPeriod : AbsenceRequestOpenPeriod
    {
        private MinMax<int> _betweenDays;

        public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
        {
            return new DateOnlyPeriod(viewpointDateOnly.AddDays(_betweenDays.Minimum), viewpointDateOnly.AddDays(_betweenDays.Maximum));
        }

        public virtual MinMax<int> BetweenDays
        {
            get 
            {
                return _betweenDays;
            }
            set 
            {
                _betweenDays = value;
            }
        }
    }
}