using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PersonAbsenceDeletedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (IPersonAbsence)
		};

		public PersonAbsenceDeletedEventPublisher(IEventPopulatingPublisher eventsPublisher)
		{
			_eventsPublisher = eventsPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var allRoots = modifiedRoots.Where(modifiedRoot=> modifiedRoot.Status == DomainUpdateType.Delete).ToList();

			var affectedInterfaces = from r in allRoots
				from i in r.Root.GetType().GetInterfaces()
				where _triggerInterfaces.Contains(i)
				select i;

			if (!affectedInterfaces.Any()) return;

			var personAbsences = allRoots.Select(r => r.Root).OfType<IPersonAbsence>();
			foreach (var personAbsence in personAbsences)
			{
				var message = new PersonAbsenceRemovedEvent()
				{
					PersonId = personAbsence.Person.Id.GetValueOrDefault(),
					ScenarioId = personAbsence.Scenario.Id.GetValueOrDefault(),
					StartDateTime = personAbsence.Period.StartDateTime,
					EndDateTime = personAbsence.Period.EndDateTime,
					LogOnBusinessUnitId = personAbsence.Scenario.BusinessUnit.Id.GetValueOrDefault()
				};

				if (personAbsence.AbsenceRequest != null)
				{
					message.AbsenceRequestId = personAbsence.AbsenceRequest.Id.GetValueOrDefault();
				}

				_eventsPublisher.Publish(message);
			}
		}
	}
}