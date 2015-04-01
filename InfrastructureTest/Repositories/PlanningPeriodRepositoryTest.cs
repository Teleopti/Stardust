using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class PlanningPeriodRepositoryTest : RepositoryTest<IPlanningPeriod>
	{
		protected override IPlanningPeriod CreateAggregateWithCorrectBusinessUnit()
		{
			return new PlanningPeriod(new TestableNow(new DateTime(2015,4,1)));
		}

		protected override void VerifyAggregateGraphProperties(IPlanningPeriod loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Range.Should().Be.EqualTo(CreateAggregateWithCorrectBusinessUnit().Range);
		}

		protected override Repository<IPlanningPeriod> TestRepository(IUnitOfWork unitOfWork)
		{
			return new PlanningPeriodRepository(unitOfWork);
		}
	}
}
