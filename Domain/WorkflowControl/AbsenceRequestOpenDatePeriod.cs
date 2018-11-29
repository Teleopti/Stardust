using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class AbsenceRequestOpenDatePeriod : AbsenceRequestOpenPeriod
    {
        private DateOnlyPeriod _period;

        public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
        {
            return _period;
        }
		
        public virtual DateOnlyPeriod Period
        {
            get {
                return _period;
            }
            set {
                _period = value;
            }
        }
    }
}