using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsHourlyAvailabilityMatchingPersonTest
	{
		[Test]
		public void ShouldUpdateUnlinkedPersonids()
		{
			var analyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			var personId = Guid.NewGuid();
			var @event = new AnalyticsPersonPeriodRangeChangedEvent { LogOnBusinessUnitId = Guid.NewGuid(), PersonIdCollection = new[] { personId } };
			var personPeriodId = 21;
			analyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = personId,
				PersonId = personPeriodId
			});
			var analyticsHourlyAvailabilityRepository = MockRepository.GenerateMock<IAnalyticsHourlyAvailabilityRepository>();
			var target = new AnalyticsHourlyAvailabilityMatchingPerson(analyticsPersonPeriodRepository, analyticsHourlyAvailabilityRepository);

			target.Handle(@event);

			analyticsHourlyAvailabilityRepository.AssertWasCalled(x => x.UpdateUnlinkedPersonids(new[] { personPeriodId }));
		}
	}
}