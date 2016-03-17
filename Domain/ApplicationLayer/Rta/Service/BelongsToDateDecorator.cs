using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class BelongsToDateDecorator : IRtaEventDecorator
	{
		public void Decorate(Context info, IEvent @event)
		{
			dynamic x = @event;
			x.BelongsToDate = info.Schedule.BelongsToDate;
			if (@event is PersonShiftEndEvent)
				x.BelongsToDate = info.Schedule.PreviousActivity().BelongsToDate;
		}
	}
}