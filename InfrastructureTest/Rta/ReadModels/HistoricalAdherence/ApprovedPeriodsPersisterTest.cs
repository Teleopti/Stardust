using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ApprovedPeriodsPersisterTest
	{
		public IApprovedPeriodsPersister Target;
		public IApprovedPeriodsReader Reader;

		[Test]
		public void ShouldPersistApprovedPeriod()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 15:00".Utc(),
				EndTime = "2018-01-30 16:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());
			result.Single().PersonId.Should().Be(personId);
			result.Single().StartTime.Should().Be("2018-01-30 15:00".Utc());
			result.Single().EndTime.Should().Be("2018-01-30 16:00".Utc());
		}
	}
}