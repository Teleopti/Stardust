using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	//I have very particular sets of skills so If you move this class i wil track you and hunt you down. 
	public class IntradayResourceCalculationForAbsenceHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
    {
        public virtual void Handle(TenantMinuteTickEvent @event)
        {
            // If there are any IntradayResourceCalculationForAbsenceHandler leftovers in the queue after update
        }
    }
}