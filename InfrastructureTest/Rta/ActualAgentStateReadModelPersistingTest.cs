using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	[RtaDatabaseTest]
	public class ActualAgentStateReadModelPersistingTest
	{
		public IDatabaseWriter Target;
		public IAgentStateReadModelReader Reader;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateReadModelForTest();

			Target.PersistActualAgentReadModel(state);

			var result = Reader.GetCurrentActualAgentState(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {BusinessUnitId = businessUnitId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {TeamId = teamId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {SiteId = siteId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.PersistActualAgentReadModel(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				TeamId = null,
				SiteId = null,
				PlatformTypeId = Guid.NewGuid(),
				OriginalDataSourceId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				BatchId = null,

				StateCode = null,
				StateId = null,
				StateStart = null,
				State = null,

				ScheduledId = null,
				Scheduled = null,
				ScheduledNextId = null,
				ScheduledNext = null,
				NextStart = null,

				AlarmId = null,
				AlarmName = null,
				AlarmStart = null,
				StaffingEffect = null,
				Adherence = null,
				Color = null,
			});

			var result = Reader.GetCurrentActualAgentState(personId);
			result.Should().Not.Be.Null();
		}

	}
}