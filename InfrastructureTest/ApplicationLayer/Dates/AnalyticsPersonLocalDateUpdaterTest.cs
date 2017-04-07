using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Dates;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Dates
{
	[InfrastructureTest]
	[AnalyticsDatabaseTest]
	public class AnalyticsPersonLocalDateUpdaterTest : ISetup
	{
		public AnalyticsPersonLocalDateUpdater Target;
		public IAnalyticsPersonPeriodRepository PersonPeriodRepository;
		public AnalyticsDatabase AnalyticsDatabase;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsPersonLocalDateUpdater>();
		}

		[Test]
		public void ShouldOnlyUpdatePeriodsWithValidToMaxDate()
		{
			// Create dates
			// Create 2 PPs, one with validto = -2 and one with validTo= something
			var personCode = Guid.NewGuid();
			var businessUnitCode = Guid.NewGuid();
			AnalyticsDatabase
				.WithEternityAndNotDefinedDate()
				.WithDatesFrom(new DateTime(2017, 01, 01), new DateTime(2017, 01, 31))
				.WithAgent(new Person(1, personCode, Guid.NewGuid(), "1", "1", new DateTime(2017, 01, 01),
					new DateTime(2017, 01, 05), 0, 4, 0, businessUnitCode, new ExistingDatasources(new UtcAndCetTimeZones()), false, 0))
				.WithAgent(new Person(2, personCode, Guid.NewGuid(), "1", "1", AnalyticsDate.Eternity.DateDate,
					new DateTime(2017, 01, 05), 0, AnalyticsDate.Eternity.DateId, 0, businessUnitCode, new ExistingDatasources(new UtcAndCetTimeZones()), false, 0));

			// Call handler
			Target.Handle(new AnalyticsDatesChangedEvent());


			// Verify that -2 has local date updated to database max, other is still same 
			var periods = WithAnalyticsUnitOfWork.Get(() => PersonPeriodRepository.GetPersonPeriods(personCode));
			var limitedPeriod = periods.First(p => p.PersonId == 1);
			var eternityPeriod = periods.First(p => p.PersonId == 2);

			limitedPeriod.ValidToDateIdLocal.Should().Be.EqualTo(4);
			eternityPeriod.ValidToDateIdLocal.Should().Be.EqualTo(29);
		}
	}
}