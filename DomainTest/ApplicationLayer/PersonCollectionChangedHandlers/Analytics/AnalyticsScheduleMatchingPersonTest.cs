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
	public class AnalyticsScheduleMatchingPersonTest
	{
		[Test]
		public void ShouldUpdateUnlinkedPersonids()
		{
			var analyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			var personId = Guid.NewGuid();
			var @event = new AnalyticsPersonPeriodRangeChangedEvent { LogOnBusinessUnitId = Guid.NewGuid(), PersonIdCollection = { personId } };
			var personPeriodId = 21;
			analyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonCode = personId,
				PersonId = personPeriodId
			});
			var analyticsScheduleRepository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			var target = new AnalyticsScheduleMatchingPerson(analyticsPersonPeriodRepository, analyticsScheduleRepository);

			target.Handle(@event);

			analyticsScheduleRepository.AssertWasCalled(x => x.UpdateUnlinkedPersonids(new[] {personPeriodId}));
		}
	}
}