using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsPreferenceMatchingPersonTest
	{
		[Test]
		public void ShouldUpdateUnlinkedPersonids()
		{
			var analyticsPersonPeriodRepository = MockRepository.GenerateMock<IAnalyticsPersonPeriodRepository>();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = Guid.NewGuid() };
			var personId = Guid.NewGuid();
			@event.SetPersonIdCollection(new[] { personId });
			var personPeriodId = 21;
			analyticsPersonPeriodRepository.Stub(x => x.GetPersonPeriods(personId)).Return(new[] { new AnalyticsPersonPeriod
			{
				PersonCode = personId,
				PersonId = personPeriodId
			} });
			var analyticsPreferenceRepository = MockRepository.GenerateMock<IAnalyticsPreferenceRepository>();
			var target = new AnalyticsPreferenceMatchingPerson(analyticsPersonPeriodRepository, analyticsPreferenceRepository);

			target.Handle(@event);

			analyticsPreferenceRepository.AssertWasCalled(x => x.UpdateUnlinkedPersonids(new[] { personPeriodId }));
		}
	}
}