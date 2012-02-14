using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class AbsenceRequestOpenDatePeriod : AbsenceRequestOpenPeriod
    {
        private DateOnlyPeriod _period;

        public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
        {
            return _period;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
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