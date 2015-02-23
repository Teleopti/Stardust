using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class BelongsToDateDecorator : IRtaEventDecorator
	{
		public void Decorate(StateInfo info, IEvent @event)
		{
			dynamic x = @event;
			x.BelongsToDate = info.BelongsToDate;
		}
	}
}