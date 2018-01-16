using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsPersonTerminalDateChangedEventHandler :
		IHandleEvent<PersonTerminalDateChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;

		public AnalyticsPersonTerminalDateChangedEventHandler(IAnalyticsScheduleRepository analyticsScheduleRepository)
		{
			_analyticsScheduleRepository = analyticsScheduleRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(2)]
		public virtual void Handle(PersonTerminalDateChangedEvent @event)
		{
			if (@event.TerminationDate.HasValue)
			{
				_analyticsScheduleRepository.DeleteFactScheduleAfterTerminalDate(@event.PersonId, @event.TerminationDate.Value);
			}
		}
	}
}