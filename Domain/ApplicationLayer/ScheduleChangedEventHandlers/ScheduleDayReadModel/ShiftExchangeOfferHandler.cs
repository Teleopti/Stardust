using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ShiftExchangeOfferHandlerHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly ShiftExchangeOfferThingy _shiftExchangeOfferThingy;

		public ShiftExchangeOfferHandlerHangfire(ShiftExchangeOfferThingy shiftExchangeOfferThingy)
		{
			_shiftExchangeOfferThingy = shiftExchangeOfferThingy;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			if (!@event.IsDefaultScenario)
				return;

			var dateAndChecksums = @event.ScheduleDays.Select(x => (Date: x.Date, CheckSum: x.CheckSum));
			_shiftExchangeOfferThingy.Execute(@event.PersonId, dateAndChecksums);
		}
	}
}