using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Dates
{
	public class AnalyticsDateChangedHandler : 
		IHandleEvent<AnalyticsDatesChangedEvent>, 
		IRunOnHangfire
	{
		public void Handle(AnalyticsDatesChangedEvent @event)
		{

		}
	}
}