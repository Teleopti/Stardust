using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class ShiftExchangeOfferRepositoryTest : RepositoryTest<IShiftExchangeOffer>
	{
		private IScheduleDay schedule;

		protected override void ConcreteSetup()
		{
			schedule = ScheduleDayFactory.Create(new DateOnly(2014, 10, 28));
			schedule.Person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersistAndRemoveFromUnitOfWork(schedule.Person);
			PersistAndRemoveFromUnitOfWork(schedule.Scenario);
		}

		protected override IShiftExchangeOffer CreateAggregateWithCorrectBusinessUnit()
		{
			return new ShiftExchangeOffer(schedule,
				new ShiftExchangeCriteria(new DateOnly(2026, 3, 4), new DateTimePeriod(2014, 10, 28, 2014, 10, 29)), ShiftExchangeOfferStatus.Pending);
		}

		protected override void VerifyAggregateGraphProperties(IShiftExchangeOffer loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Date.Should().Be.EqualTo(new DateOnly(2014, 10, 28));
		}

		protected override Repository<IShiftExchangeOffer> TestRepository(IUnitOfWork unitOfWork)
		{
			return new ShiftExchangeOfferRepository(unitOfWork);
		}
	}
}
