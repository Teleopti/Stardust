using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ShiftExchangeOfferHandlerNew :
		IHandleEvent<ProjectionChangedEventForShiftExchangeOffer>,
		IRunOnHangfire
	{
		private readonly ShiftExchangeOfferThingy _shiftExchangeOfferThingy;

		public ShiftExchangeOfferHandlerNew(ShiftExchangeOfferThingy shiftExchangeOfferThingy)
		{
			_shiftExchangeOfferThingy = shiftExchangeOfferThingy;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForShiftExchangeOffer @event)
		{
			var dateAndChecksums = @event.Days.Select(x => (Date: x.Date, CheckSum: x.Checksum));
			_shiftExchangeOfferThingy.Execute(@event.PersonId, dateAndChecksums);
		}
	}
}