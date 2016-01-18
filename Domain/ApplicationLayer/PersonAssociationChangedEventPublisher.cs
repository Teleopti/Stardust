using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[UseOnToggle(Toggles.RTA_TerminatedPersons_36042)]
	public class PersonAssociationChangedEventPublisher : 
		IHandleEvent<HourlyTickEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _persons;
		private readonly IEventPublisher _eventPublisher;
		private readonly INow _now;

		public PersonAssociationChangedEventPublisher(
			IPersonRepository persons,
			IEventPublisher eventPublisher,
			INow now)
		{
			_persons = persons;
			_eventPublisher = eventPublisher;
			_now = now;
		}

		[UnitOfWork]
		[RecurringId("PersonAssociationChangedEventPublisher:::HourlyTickEvent")]
		public virtual void Handle(HourlyTickEvent @event)
		{
			_persons.LoadAll()
				.Where(p => p.TerminalDate.HasValue)
				.ForEach(person =>
				{
					var terminatedAt = TimeZoneInfo.ConvertTimeToUtc(
						person.TerminalDate.Value.Date.AddDays(1),
						person.PermissionInformation.DefaultTimeZone()
						);
					var now = _now.UtcDateTime();

					if (terminatedAt > now)
						return;
					if (terminatedAt < now.AddDays(-1))
						return;

					_eventPublisher.Publish(new PersonAssociationChangedEvent
					{
						PersonId = person.Id.Value,
						Timestamp = now
					});
				});
		}
	}

	public class PersonAssociationChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid? TeamId { get; set; }
	}
}