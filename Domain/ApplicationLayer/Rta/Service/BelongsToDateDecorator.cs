using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class BelongsToDateDecorator : IRtaEventDecorator
	{
		public void Decorate(StateInfo info, IEvent @event)
		{
			dynamic x = @event;
			x.BelongsToDate = info.Schedule.BelongsToDate;
			if (@event is PersonShiftEndEvent)
				x.BelongsToDate = info.PreviousActivity.BelongsToDate;
		}
	}
}